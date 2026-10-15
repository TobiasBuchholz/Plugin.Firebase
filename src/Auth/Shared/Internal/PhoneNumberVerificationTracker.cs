namespace Plugin.Firebase.Auth.Internal;

/// <summary>
/// Tracks phone number verifications whose progress the native SDK reports through callbacks, as Android's
/// <c>PhoneAuthProvider.OnVerificationStateChangedCallbacks</c> does.
/// </summary>
/// <remarks>
/// State is kept per phone number to mirror the native session map (firebase-auth 23.x), which keys sessions by phone
/// number. A request within <see cref="NativeSessionWindow"/> of the session's last registration reuses it: native
/// attaches the new callbacks without repeating <c>onCodeSent</c>, or drops them while app verification is still
/// running. A completed, failed or timed-out callback ends the session. Only the callbacks of a number's current
/// session change that number's state.
/// </remarks>
/// <typeparam name="TCredential">The native phone auth credential type.</typeparam>
internal sealed class PhoneNumberVerificationTracker<TCredential> where TCredential : class
{
    /// <summary>
    /// How long the native SDK keeps a phone number's session for reuse after it was last registered. A pending
    /// verification that hears nothing within this window faults with <see cref="TimeoutException"/>.
    /// </summary>
    public static readonly TimeSpan NativeSessionWindow = TimeSpan.FromSeconds(120);

    private readonly object _gate = new();
    private readonly TimeProvider _timeProvider;
    private readonly Dictionary<string, PhoneNumberState> _phoneNumbers = new(StringComparer.Ordinal);
    private PhoneNumberState? _current;

    public PhoneNumberVerificationTracker()
        : this(TimeProvider.System)
    {
    }

    public PhoneNumberVerificationTracker(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    /// <summary>
    /// Starts or joins the verification for <paramref name="phoneNumber"/> and makes it the one used for sign-in.
    /// </summary>
    /// <remarks>
    /// <paramref name="startNativeVerification"/> receives the session that the native callbacks must report to. It runs
    /// under the tracker's lock, so if it throws, the tracker is restored before any other request or callback sees the
    /// change. The native call only schedules work; its callbacks are delivered later.
    /// </remarks>
    /// <returns>
    /// A task that completes when a code was sent or the number was verified without a code, faults when verification
    /// fails or the native session window passes without a result, and is canceled when a request for another phone
    /// number comes first.
    /// </returns>
    public Task Start(string phoneNumber, Action<Session> startNativeVerification)
    {
        lock(_gate) {
            var now = _timeProvider.GetUtcNow();
            var isNewPhoneNumber = !_phoneNumbers.TryGetValue(phoneNumber, out var state);
            state ??= new PhoneNumberState();
            var previousSession = state.Session;
            var session = previousSession is not null && previousSession.IsOpen(now)
                ? previousSession
                : new Session(this, state);

            if(isNewPhoneNumber) {
                _phoneNumbers.Add(phoneNumber, state);
            }
            state.Session = session;
            try {
                startNativeVerification(session);
            } catch {
                state.Session = previousSession;
                if(isNewPhoneNumber) {
                    _phoneNumbers.Remove(phoneNumber);
                }
                throw;
            }

            if(previousSession is not null && !ReferenceEquals(previousSession, session)) {
                previousSession.ExpireWaiter();
            }
            if(!ReferenceEquals(_current, state)) {
                _current?.Session?.CancelWaiter();
                _current = state;
            }
            return session.Join(now);
        }
    }

    /// <summary>
    /// Returns the credential for the phone number of the most recent request. If its latest verification completed
    /// without a code, that credential is returned once and <paramref name="verificationCode"/> is ignored. Otherwise the
    /// credential is created from the latest verification ID and <paramref name="verificationCode"/>; a repeated
    /// verification that is still pending or failed doesn't replace that ID.
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
            if(_current.InstantCredential is { } instantCredential) {
                _current.InstantCredential = null;
                return instantCredential;
            }
            verificationId = _current.VerificationId
                ?? throw new InvalidOperationException(
                    "Phone number verification hasn't produced a verification ID or an unused instant credential. Await VerifyPhoneNumberAsync before signing in or linking."
                );
        }
        return createCredential(verificationId, verificationCode);
    }

    internal sealed class PhoneNumberState
    {
        public Session? Session { get; set; }

        public string? VerificationId { get; set; }

        public TCredential? InstantCredential { get; set; }
    }

    /// <summary>
    /// One native verification session for a phone number. Requests that join it share its task.
    /// </summary>
    internal sealed class Session
    {
        private readonly PhoneNumberVerificationTracker<TCredential> _owner;
        private readonly PhoneNumberState _state;
        private DateTimeOffset _lastRegisteredAt;
        private bool _codeSent;
        private bool _resolved;
        private bool _ended;
        private TaskCompletionSource? _waiter;
        private ITimer? _deadline;

        internal Session(PhoneNumberVerificationTracker<TCredential> owner, PhoneNumberState state)
        {
            _owner = owner;
            _state = state;
        }

        private bool IsCurrent => ReferenceEquals(_state.Session, this);

        public void OnCodeSent(string verificationId)
        {
            TaskCompletionSource? waiter;
            lock(_owner._gate) {
                if(!IsCurrent) {
                    return;
                }
                SetVerificationId(verificationId);
                // native registered the session's operation right before the code was sent
                _lastRegisteredAt = _owner._timeProvider.GetUtcNow();
                waiter = TakeWaiter();
            }
            waiter?.TrySetResult();
        }

        public void OnVerificationCompleted(TCredential credential)
        {
            TaskCompletionSource? waiter;
            lock(_owner._gate) {
                if(!IsCurrent) {
                    return;
                }
                _ended = true;
                if(_codeSent) {
                    // the SMS was auto-retrieved after the code was sent; sign-in keeps using the verification ID and code
                    return;
                }
                _state.InstantCredential = credential;
                _resolved = true;
                waiter = TakeWaiter();
            }
            waiter?.TrySetResult();
        }

        public void OnVerificationFailed(Exception exception)
        {
            TaskCompletionSource? waiter;
            lock(_owner._gate) {
                if(!IsCurrent) {
                    return;
                }
                _ended = true;
                waiter = TakeWaiter();
            }
            waiter?.TrySetException(exception);
        }

        public void OnCodeAutoRetrievalTimeOut(string verificationId)
        {
            TaskCompletionSource? waiter;
            lock(_owner._gate) {
                if(!IsCurrent) {
                    return;
                }
                _ended = true;
                if(!_codeSent) {
                    // onCodeSent went to callbacks the native SDK dropped; these callbacks were attached afterwards
                    SetVerificationId(verificationId);
                }
                waiter = TakeWaiter();
            }
            waiter?.TrySetResult();
        }

        internal bool IsOpen(DateTimeOffset now)
        {
            return !_ended && now - _lastRegisteredAt < NativeSessionWindow;
        }

        internal Task Join(DateTimeOffset now)
        {
            // native registers the session again when it attaches callbacks to it
            _lastRegisteredAt = now;
            if(_resolved) {
                return Task.CompletedTask;
            }
            _waiter ??= new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            if(_deadline is null) {
                _deadline = _owner._timeProvider.CreateTimer(
                    static session => ((Session) session!).OnDeadline(),
                    this,
                    NativeSessionWindow,
                    Timeout.InfiniteTimeSpan
                );
            } else {
                _deadline.Change(NativeSessionWindow, Timeout.InfiniteTimeSpan);
            }
            return _waiter.Task;
        }

        internal void CancelWaiter()
        {
            TakeWaiter()?.TrySetCanceled();
        }

        internal void ExpireWaiter()
        {
            TakeWaiter()?.TrySetException(CreateTimeoutException());
        }

        private void OnDeadline()
        {
            lock(_owner._gate) {
                if(_waiter is null || IsOpen(_owner._timeProvider.GetUtcNow())) {
                    return;
                }
                ExpireWaiter();
            }
        }

        private void SetVerificationId(string verificationId)
        {
            _state.VerificationId = verificationId;
            _state.InstantCredential = null;
            _codeSent = true;
            _resolved = true;
        }

        private TaskCompletionSource? TakeWaiter()
        {
            var waiter = _waiter;
            _waiter = null;
            _deadline?.Dispose();
            _deadline = null;
            return waiter;
        }

        private static TimeoutException CreateTimeoutException()
        {
            return new TimeoutException(
                $"Phone number verification reported no sent code, completion or failure within {NativeSessionWindow.TotalSeconds:0} seconds. The native SDK may have dropped its callbacks, for example because the app went to the background before the code was sent. Call VerifyPhoneNumberAsync again."
            );
        }
    }
}