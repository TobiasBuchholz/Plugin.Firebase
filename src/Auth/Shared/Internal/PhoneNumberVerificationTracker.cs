namespace Plugin.Firebase.Auth.Internal;

/// <summary>
/// Tracks phone number verification attempts whose progress the native SDK reports through callbacks, as Android's
/// <c>PhoneAuthProvider.OnVerificationStateChangedCallbacks</c> does. Only the current attempt changes state, so
/// callbacks from a superseded attempt can't overwrite a newer verification.
/// </summary>
/// <typeparam name="TCredential">The native phone auth credential type.</typeparam>
internal sealed class PhoneNumberVerificationTracker<TCredential> where TCredential : class
{
    private readonly object _gate = new();
    private Attempt? _current;

    /// <summary>
    /// Returns the attempt that the callbacks of a new native verification request report to.
    /// </summary>
    /// <remarks>
    /// A request for the current attempt's phone number reuses that attempt while its native session is open. The native
    /// SDK reuses the session for the same number: it doesn't attach the new callbacks while app verification is still
    /// running, and it doesn't repeat <c>onCodeSent</c> once the code was sent. Any other request starts a new attempt and
    /// cancels the previous attempt's task if it is still pending.
    /// </remarks>
    public Attempt Start(string phoneNumber)
    {
        Attempt? superseded;
        Attempt attempt;
        lock(_gate) {
            if(_current is { } current && current.CanBeReusedFor(phoneNumber)) {
                return current;
            }
            superseded = _current;
            attempt = new Attempt(this, phoneNumber);
            _current = attempt;
        }
        superseded?.Cancel();
        return attempt;
    }

    /// <summary>
    /// Returns the credential for the current attempt. Once a code was sent, the credential is always created from the
    /// verification ID and <paramref name="verificationCode"/>. If the number was verified instantly without sending a
    /// code, the instant credential is returned once and <paramref name="verificationCode"/> is ignored.
    /// </summary>
    public TCredential GetCredential(
        string verificationCode,
        Func<string, string, TCredential> createCredential)
    {
        string verificationId;
        lock(_gate) {
            if(_current is null) {
                throw new InvalidOperationException(
                    "VerifyPhoneNumberAsync must be called before GetCredentialAsync."
                );
            }
            if(_current.VerificationId is null) {
                var instantCredential = _current.InstantCredential;
                if(instantCredential is null) {
                    throw new InvalidOperationException(
                        "Phone number verification hasn't produced a verification ID or an unused instant credential. Await VerifyPhoneNumberAsync before signing in or linking."
                    );
                }
                _current.InstantCredential = null;
                return instantCredential;
            }
            verificationId = _current.VerificationId;
        }
        return createCredential(verificationId, verificationCode);
    }

    /// <summary>
    /// One verification request, or several requests for the same phone number that share a native session.
    /// </summary>
    internal sealed class Attempt
    {
        private readonly PhoneNumberVerificationTracker<TCredential> _owner;
        private readonly TaskCompletionSource _completion =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        private bool _nativeSessionEnded;

        public Attempt(PhoneNumberVerificationTracker<TCredential> owner, string phoneNumber)
        {
            _owner = owner;
            PhoneNumber = phoneNumber;
        }

        public string PhoneNumber { get; }

        /// <summary>
        /// Completes when a code was sent or the number was verified instantly, faults when verification failed, and is
        /// canceled when a request for another phone number supersedes this attempt first.
        /// </summary>
        public Task Task => _completion.Task;

        internal string? VerificationId { get; private set; }

        internal TCredential? InstantCredential { get; set; }

        private bool IsCurrent => ReferenceEquals(_owner._current, this);

        public void OnCodeSent(string verificationId)
        {
            lock(_owner._gate) {
                if(!IsCurrent) {
                    return;
                }
                VerificationId = verificationId;
            }
            _completion.TrySetResult();
        }

        public void OnVerificationCompleted(TCredential credential)
        {
            lock(_owner._gate) {
                if(!IsCurrent) {
                    return;
                }
                _nativeSessionEnded = true;
                if(VerificationId is not null) {
                    // the SMS was auto-retrieved after a code was sent; sign-in keeps using the verification ID and code
                    return;
                }
                InstantCredential = credential;
            }
            _completion.TrySetResult();
        }

        public void OnVerificationFailed(Exception exception)
        {
            lock(_owner._gate) {
                if(!IsCurrent) {
                    return;
                }
                _nativeSessionEnded = true;
            }
            _completion.TrySetException(exception);
        }

        public void OnCodeAutoRetrievalTimeOut(string verificationId)
        {
            lock(_owner._gate) {
                if(!IsCurrent) {
                    return;
                }
                _nativeSessionEnded = true;
                // normally onCodeSent already set it; this covers callbacks attached after onCodeSent fired
                VerificationId ??= verificationId;
            }
            _completion.TrySetResult();
        }

        internal bool CanBeReusedFor(string phoneNumber)
        {
            return !_nativeSessionEnded && string.Equals(PhoneNumber, phoneNumber, StringComparison.Ordinal);
        }

        internal void Cancel()
        {
            _completion.TrySetCanceled();
        }
    }
}