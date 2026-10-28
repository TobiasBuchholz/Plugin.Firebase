using System.Diagnostics;
using Microsoft.Extensions.Time.Testing;
using Request = Plugin.Firebase.Auth.Internal.PhoneNumberVerificationTracker<string, string>.Request;
using Tracker = Plugin.Firebase.Auth.Internal.PhoneNumberVerificationTracker<string, string>;

namespace Plugin.Firebase.UnitTests.Auth;

public class PhoneNumberVerificationTrackerTests
{
    private const string PhoneNumberA = "+15555550100";
    private const string PhoneNumberB = "+15555550101";
    private static readonly TimeSpan VerificationTimeout = TimeSpan.FromMinutes(5);

    private readonly FakeTimeProvider _time = new();
    private readonly List<string?> _resendTokensPassed = new();
    private readonly Tracker _sut;

    public PhoneNumberVerificationTrackerTests()
    {
        _sut = new Tracker(_time, VerificationTimeout);
    }

    [Fact]
    public void code_sent_completes_task_and_credential_uses_verification_id_and_code()
    {
        var (task, request) = Verify(PhoneNumberA);
        Assert.False(task.IsCompleted);

        request.OnCodeSent("vid-a", "token-a");

        Assert.True(task.IsCompletedSuccessfully);
        Assert.Equal("vid-a:123456", GetCredential("123456"));
    }

    [Fact]
    public void verification_failure_faults_task_with_reported_exception()
    {
        var failure = new InvalidOperationException("invalid phone number");
        var (task, request) = Verify(PhoneNumberA);

        request.OnVerificationFailed(failure);

        Assert.Same(failure, AssertFaultedWith<InvalidOperationException>(task));
        Assert.Throws<InvalidOperationException>(() => GetCredential("123456"));
    }

    [Fact]
    public void verification_completed_without_code_gives_credential_until_next_request()
    {
        var (task, request) = Verify(PhoneNumberA);

        request.OnVerificationCompleted("instant");

        Assert.True(task.IsCompletedSuccessfully);
        Assert.Equal("instant", GetCredential("ignored"));
        Assert.Equal("instant", GetCredential("ignored"));

        Verify(PhoneNumberA);

        Assert.Throws<InvalidOperationException>(() => GetCredential("ignored"));
    }

    [Fact]
    public void auto_retrieval_after_code_sent_keeps_using_verification_id_and_code()
    {
        var (_, request) = Verify(PhoneNumberA);
        request.OnCodeSent("vid-a", "token-a");

        request.OnVerificationCompleted("auto-retrieved");

        Assert.Equal("vid-a:123456", GetCredential("123456"));
    }

    [Fact]
    public void duplicate_terminal_callbacks_are_ignored()
    {
        var (failedTask, failed) = Verify(PhoneNumberA);
        failed.OnVerificationFailed(new InvalidOperationException("first"));
        failed.OnVerificationCompleted("late");
        AssertFaultedWith<InvalidOperationException>(failedTask);
        Assert.Throws<InvalidOperationException>(() => GetCredential("123456"));

        var (_, completed) = Verify(PhoneNumberA);
        completed.OnVerificationCompleted("first");
        completed.OnVerificationCompleted("second");
        completed.OnCodeAutoRetrievalTimeOut("vid-late");
        Assert.Equal("first", GetCredential("ignored"));
    }

    [Fact]
    public void double_tap_for_same_number_while_pending_returns_same_task_without_new_native_request()
    {
        var (first, request) = Verify(PhoneNumberA);

        var second = _sut.Start(PhoneNumberA, (_, _) => throw new InvalidOperationException("not expected"));

        Assert.Same(first, second);
        request.OnCodeSent("vid-a", "token-a");
        Assert.True(second.IsCompletedSuccessfully);
        Assert.Single(_resendTokensPassed);
    }

    [Fact]
    public void first_request_for_number_passes_no_resend_token()
    {
        Verify(PhoneNumberA);

        Assert.Equal(new string?[] { null }, _resendTokensPassed);
    }

    [Fact]
    public void repeat_request_after_code_sent_starts_new_verification_with_resend_token()
    {
        var (_, first) = Verify(PhoneNumberA);
        first.OnCodeSent("vid-a1", "token-a1");

        var (task, second) = Verify(PhoneNumberA);

        Assert.Equal(new[] { null, "token-a1" }, _resendTokensPassed);
        Assert.False(task.IsCompleted);
        // until the new code is sent, the previous code stays usable
        Assert.Equal("vid-a1:123456", GetCredential("123456"));

        second.OnCodeSent("vid-a2", "token-a2");

        Assert.True(task.IsCompletedSuccessfully);
        Assert.Equal("vid-a2:654321", GetCredential("654321"));
    }

    [Fact]
    public void resend_after_sign_in_starts_new_verification_with_resend_token()
    {
        // native clears its session map when a phone credential is used, so the resend must not wait for a reused session
        var (_, first) = Verify(PhoneNumberA);
        first.OnCodeSent("vid-a1", "token-a1");
        Assert.Equal("vid-a1:000000", GetCredential("000000"));

        var (task, second) = Verify(PhoneNumberA);
        second.OnVerificationFailed(new InvalidOperationException("quota exceeded"));

        Assert.Equal("token-a1", _resendTokensPassed[^1]);
        AssertFaultedWith<InvalidOperationException>(task);
        Assert.Equal("vid-a1:123456", GetCredential("123456"));
    }

    [Fact]
    public void late_callbacks_from_earlier_request_are_ignored()
    {
        var (_, first) = Verify(PhoneNumberA);
        first.OnCodeSent("vid-a1", "token-a1");
        var (_, second) = Verify(PhoneNumberA);
        second.OnCodeSent("vid-a2", "token-a2");

        first.OnCodeAutoRetrievalTimeOut("vid-a1");
        first.OnVerificationCompleted("stale-credential");
        first.OnCodeSent("vid-stale", "token-stale");

        Assert.Equal("vid-a2:123456", GetCredential("123456"));
    }

    [Fact]
    public void request_for_different_number_cancels_pending_task_and_ignores_its_callbacks()
    {
        var (taskA, requestA) = Verify(PhoneNumberA);

        var (taskB, _) = Verify(PhoneNumberB);
        requestA.OnCodeSent("vid-a", "token-a");

        Assert.True(taskA.IsCanceled);
        Assert.False(taskB.IsCompleted);
        Assert.Throws<InvalidOperationException>(() => GetCredential("123456"));
    }

    [Fact]
    public void request_for_different_number_does_not_use_previous_number_verification_id()
    {
        var (_, requestA) = Verify(PhoneNumberA);
        requestA.OnCodeSent("vid-a", "token-a");

        Verify(PhoneNumberB);

        Assert.Throws<InvalidOperationException>(() => GetCredential("123456"));
    }

    [Fact]
    public void callbacks_for_other_number_do_not_change_current_credential()
    {
        var (_, requestA) = Verify(PhoneNumberA);
        requestA.OnCodeSent("vid-a", "token-a");
        var (_, requestB) = Verify(PhoneNumberB);
        requestB.OnCodeSent("vid-b", "token-b");

        requestA.OnCodeAutoRetrievalTimeOut("vid-a");
        requestA.OnVerificationCompleted("account-a");

        Assert.Equal("vid-b:123456", GetCredential("123456"));
    }

    [Fact]
    public void returning_to_earlier_number_passes_its_resend_token()
    {
        var (_, requestA) = Verify(PhoneNumberA);
        Verify(PhoneNumberB);
        // the superseded request's late code still records the number's token
        requestA.OnCodeSent("vid-a", "token-a");

        var (task, _) = Verify(PhoneNumberA);

        Assert.Equal(new[] { null, null, "token-a" }, _resendTokensPassed);
        Assert.False(task.IsCompleted);
    }

    [Fact]
    public void pending_verification_faults_with_timeout()
    {
        var (task, _) = Verify(PhoneNumberA);

        _time.Advance(VerificationTimeout - TimeSpan.FromTicks(1));
        Assert.False(task.IsCompleted);
        _time.Advance(TimeSpan.FromTicks(1));

        var ex = AssertFaultedWith<TimeoutException>(task);
        Assert.Contains("VerifyPhoneNumberAsync", ex.Message);
        Assert.Throws<InvalidOperationException>(() => GetCredential("123456"));
    }

    [Fact]
    public void request_after_timeout_starts_new_verification()
    {
        var (first, _) = Verify(PhoneNumberA);
        _time.Advance(VerificationTimeout);

        var (second, request) = Verify(PhoneNumberA);
        request.OnCodeSent("vid-a", "token-a");

        Assert.NotSame(first, second);
        Assert.Equal(2, _resendTokensPassed.Count);
        Assert.True(second.IsCompletedSuccessfully);
    }

    [Fact]
    public void code_sent_after_timeout_is_still_used()
    {
        var (task, request) = Verify(PhoneNumberA);
        _time.Advance(VerificationTimeout);

        request.OnCodeSent("vid-a", "token-a");

        AssertFaultedWith<TimeoutException>(task);
        Assert.Equal("vid-a:123456", GetCredential("123456"));
    }

    [Fact]
    public void completed_request_does_not_time_out_later()
    {
        var (task, request) = Verify(PhoneNumberA);
        request.OnCodeSent("vid-a", "token-a");

        _time.Advance(VerificationTimeout * 2);

        Assert.True(task.IsCompletedSuccessfully);
    }

    [Fact]
    public void timeout_fires_even_if_wall_clock_has_not_reached_due_time()
    {
        // real timers run on the tick count and can fire before GetUtcNow reaches the due time
        var sut = new Tracker(new FrozenWallClock(_time), VerificationTimeout);
        var task = sut.Start(PhoneNumberA, (_, _) => { });

        _time.Advance(VerificationTimeout);

        AssertFaultedWith<TimeoutException>(task);
    }

    [Fact]
    public async Task every_pending_request_times_out_with_system_timers()
    {
        var timeout = TimeSpan.FromMilliseconds(20);
        var random = new Random(1);
        var tasks = new List<Task>();
        for(var i = 0; i < 200; i++) {
            tasks.Add(new Tracker(TimeProvider.System, timeout).Start(PhoneNumberA, (_, _) => { }));
            // stagger the starts so the timers don't all fire in one batch
            var stagger = Stopwatch.StartNew();
            var staggerTicks = random.Next(0, (int) (Stopwatch.Frequency / 1000));
            while(stagger.ElapsedTicks < staggerTicks) {
            }
        }

        var allDone = Task.WhenAll(tasks.Select(x => x.ContinueWith(_ => { }, TaskScheduler.Default)));
        await allDone.WaitAsync(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken);

        Assert.All(tasks, x => AssertFaultedWith<TimeoutException>(x));
    }

    [Fact]
    public void synchronous_native_failure_keeps_previous_request()
    {
        var (_, requestA) = Verify(PhoneNumberA);
        requestA.OnCodeSent("vid-a", "token-a");

        // the native call throws before it schedules any work
        Assert.Throws<ArgumentException>(() => StartWithSynchronousFailure(""));

        Assert.Equal("vid-a:123456", GetCredential("123456"));
    }

    [Fact]
    public void synchronous_native_failure_does_not_cancel_pending_task()
    {
        var (task, request) = Verify(PhoneNumberA);

        Assert.Throws<ArgumentException>(() => StartWithSynchronousFailure(PhoneNumberB));
        request.OnCodeSent("vid-a", "token-a");

        Assert.True(task.IsCompletedSuccessfully);
        Assert.Equal("vid-a:123456", GetCredential("123456"));
    }

    [Fact]
    public void timeout_callback_without_code_sent_supplies_verification_id()
    {
        var (task, request) = Verify(PhoneNumberA);

        request.OnCodeAutoRetrievalTimeOut("vid-a");

        Assert.True(task.IsCompletedSuccessfully);
        Assert.Equal("vid-a:123456", GetCredential("123456"));
    }

    [Fact]
    public void failure_after_code_sent_does_not_change_completed_task()
    {
        var (task, request) = Verify(PhoneNumberA);
        request.OnCodeSent("vid-a", "token-a");

        request.OnVerificationFailed(new InvalidOperationException("late failure"));

        Assert.True(task.IsCompletedSuccessfully);
        Assert.Equal("vid-a:123456", GetCredential("123456"));
    }

    [Fact]
    public void get_credential_before_verification_throws()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => GetCredential("123456"));

        Assert.Contains("VerifyPhoneNumberAsync", ex.Message);
    }

    [Fact]
    public void get_credential_while_first_verification_is_pending_throws()
    {
        Verify(PhoneNumberA);

        Assert.Throws<InvalidOperationException>(() => GetCredential("123456"));
    }

    private (Task Task, Request Request) Verify(string phoneNumber)
    {
        Request? request = null;
        var task = _sut.Start(phoneNumber, (x, resendToken) => {
            request = x;
            _resendTokensPassed.Add(resendToken);
        });
        return (task, request!);
    }

    private void StartWithSynchronousFailure(string phoneNumber)
    {
        _ = _sut.Start(phoneNumber, (_, _) => throw new ArgumentException("native verification failed to start"));
    }

    private string GetCredential(string verificationCode)
    {
        return _sut.GetCredential(verificationCode, (id, code) => $"{id}:{code}");
    }

    private sealed class FrozenWallClock : TimeProvider
    {
        private readonly FakeTimeProvider _timers;
        private readonly DateTimeOffset _now;

        public FrozenWallClock(FakeTimeProvider timers)
        {
            _timers = timers;
            _now = timers.GetUtcNow();
        }

        public override DateTimeOffset GetUtcNow() => _now;

        public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
        {
            return _timers.CreateTimer(callback, state, dueTime, period);
        }
    }

    private static TException AssertFaultedWith<TException>(Task task) where TException : Exception
    {
        // checked synchronously so a regression fails the test instead of hanging it
        Assert.True(task.IsFaulted, $"Expected a faulted task, but it is {task.Status}.");
        return Assert.IsType<TException>(task.Exception!.InnerException);
    }
}