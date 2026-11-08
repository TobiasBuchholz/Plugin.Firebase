namespace Plugin.Firebase.Auth.Internal;

/// <summary>
/// Tracks phone number verification requests whose progress the native SDK reports through callbacks, as Android's
/// <c>PhoneAuthProvider.OnVerificationStateChangedCallbacks</c> does. Each request has its own task and callbacks, and
/// only the latest request's callbacks change the state that sign-in and link read.
/// </summary>
/// <typeparam name="TCredential">The native phone auth credential type.</typeparam>
/// <typeparam name="TResendToken">The native token that forces a new verification for a phone number.</typeparam>
internal sealed class PhoneNumberVerificationTracker<TCredential, TResendToken>
    where TCredential : class
    where TResendToken : class
{
    /// <summary>
    /// How long a request waits for the native SDK to report a sent code, a completed verification or a failure. The wait
    /// includes any reCAPTCHA challenge the native SDK shows before it sends the code.
    /// </summary>
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(5);

    private readonly object _gate = new();
    private readonly TimeProvider _timeProvider;
    private readonly TimeSpan _timeout;
    private readonly Dictionary<string, TResendToken> _resendTokens = new(StringComparer.Ordinal);
    private Request? _latest;

    public PhoneNumberVerificationTracker()
        : this(TimeProvider.System, DefaultTimeout)
    {
    }

    public PhoneNumberVerificationTracker(TimeProvider timeProvider, TimeSpan timeout)
    {
        _timeProvider = timeProvider;
        _timeout = timeout;
    }

    /// <summary>
    /// Starts a verification request for <paramref name="phoneNumber"/>, or returns the task of the latest request if it
    /// is for the same number and still pending.
    /// </summary>
    /// <remarks>
    /// <paramref name="startNativeVerification"/> receives the request its callbacks report to, and the resend token from
    /// the last code sent to this number, if any. It runs under the tracker's lock: if it throws, the previous request
    /// stays the latest. The native call only schedules work; its callbacks are delivered later.
    /// </remarks>
    public Task Start(string phoneNumber, Action<Request, TResendToken?> startNativeVerification)
    {
        lock(_gate) {
            if(_latest is { IsPending: true } pending && pending.PhoneNumber == phoneNumber) {
                return pending.Task;
            }
            var previous = _latest;
            var request = new Request(this, phoneNumber);
            if(previous?.PhoneNumber == phoneNumber) {
                // until the new code is sent, the previous code for this number stays usable
                request.VerificationId = previous.VerificationId;
            }
            _resendTokens.TryGetValue(phoneNumber, out var resendToken);
            _latest = request;
            try {
                startNativeVerification(request, resendToken);
            } catch {
                _latest = previous;
                throw;
            }
            previous?.Cancel();
            request.StartTimeout();
            return request.Task;
        }
    }

    /// <summary>
    /// Returns the credential for the latest request: its instant credential if it was verified without a code,
    /// otherwise one created from its verification ID and <paramref name="verificationCode"/>.
    /// </summary>
    public TCredential GetCredential(
        string verificationCode,
        Func<string, string, TCredential> createCredential)
    {
        string verificationId;
        lock(_gate) {
            if(_latest is null) {
                throw new InvalidOperationException(
                    "VerifyPhoneNumberAsync must be called before GetCredentialAsync."
                );
            }
            if(_latest.InstantCredential is { } instantCredential) {
                return instantCredential;
            }
            verificationId = _latest.VerificationId
                ?? throw new InvalidOperationException(
                    "Phone number verification hasn't produced a verification ID. Await VerifyPhoneNumberAsync before signing in or linking."
                );
        }
        return createCredential(verificationId, verificationCode);
    }

    /// <summary>
    /// One <c>VerifyPhoneNumberAsync</c> request and the native callbacks bound to it.
    /// </summary>
    internal sealed class Request
    {
        private readonly PhoneNumberVerificationTracker<TCredential, TResendToken> _owner;
        private readonly TaskCompletionSource _completion =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        private ITimer? _timeoutTimer;
        private bool _codeSent;
        private bool _ended;

        internal Request(PhoneNumberVerificationTracker<TCredential, TResendToken> owner, string phoneNumber)
        {
            _owner = owner;
            PhoneNumber = phoneNumber;
        }

        public string PhoneNumber { get; }

        public Task Task => _completion.Task;

        internal string? VerificationId { get; set; }

        internal TCredential? InstantCredential { get; private set; }

        internal bool IsPending => !_completion.Task.IsCompleted;

        private bool IsLatest => ReferenceEquals(_owner._latest, this);

        public void OnCodeSent(string verificationId, TResendToken resendToken)
        {
            lock(_owner._gate) {
                // any token for this number makes the next request for it start a new native verification
                _owner._resendTokens[PhoneNumber] = resendToken;
                if(!IsLatest) {
                    return;
                }
                VerificationId = verificationId;
                _codeSent = true;
            }
            Finish(x => x.TrySetResult());
        }

        public void OnVerificationCompleted(TCredential credential)
        {
            lock(_owner._gate) {
                if(!IsLatest || _ended) {
                    return;
                }
                _ended = true;
                if(!_codeSent) {
                    // completed without a code; once a code was sent, the code is used even if the SMS was auto-retrieved
                    InstantCredential = credential;
                }
            }
            Finish(x => x.TrySetResult());
        }

        public void OnVerificationFailed(Exception exception)
        {
            lock(_owner._gate) {
                if(!IsLatest || _ended) {
                    return;
                }
                _ended = true;
            }
            Finish(x => x.TrySetException(exception));
        }

        public void OnCodeAutoRetrievalTimeOut(string verificationId)
        {
            lock(_owner._gate) {
                if(!IsLatest || _ended) {
                    return;
                }
                _ended = true;
                if(!_codeSent) {
                    // the native SDK attached these callbacks to a session whose onCodeSent had already fired
                    VerificationId = verificationId;
                    _codeSent = true;
                }
            }
            Finish(x => x.TrySetResult());
        }

        internal void StartTimeout()
        {
            if(IsPending) {
                _timeoutTimer = _owner._timeProvider.CreateTimer(
                    static request => ((Request) request!).OnTimeout(),
                    this,
                    _owner._timeout,
                    Timeout.InfiniteTimeSpan
                );
            }
        }

        internal void Cancel()
        {
            Finish(x => x.TrySetCanceled());
        }

        private void Finish(Action<TaskCompletionSource> complete)
        {
            Interlocked.Exchange(ref _timeoutTimer, null)?.Dispose();
            complete(_completion);
        }

        private void OnTimeout()
        {
            // no clock comparison: the timer firing is the deadline
            var exception = new TimeoutException(
                $"Phone number verification reported no sent code, completion or failure within {_owner._timeout.TotalSeconds:0} seconds. The native SDK may have dropped its callbacks, for example because the app went to the background before the code was sent. Call VerifyPhoneNumberAsync again."
            );
            Finish(x => x.TrySetException(exception));
        }
    }
}