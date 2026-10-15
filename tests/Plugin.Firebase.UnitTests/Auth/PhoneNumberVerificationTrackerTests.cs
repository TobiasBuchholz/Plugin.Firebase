using Plugin.Firebase.Auth.Internal;
using Session = Plugin.Firebase.Auth.Internal.PhoneNumberVerificationTracker<string>.Session;

namespace Plugin.Firebase.UnitTests.Auth;

public class PhoneNumberVerificationTrackerTests
{
    private const string PhoneNumberA = "+15555550100";
    private const string PhoneNumberB = "+15555550101";
    private static readonly TimeSpan Window = PhoneNumberVerificationTracker<string>.NativeSessionWindow;

    private readonly ManualTimeProvider _time = new();
    private readonly PhoneNumberVerificationTracker<string> _sut;

    public PhoneNumberVerificationTrackerTests()
    {
        _sut = new PhoneNumberVerificationTracker<string>(_time);
    }

    [Fact]
    public void code_sent_completes_task_and_credential_uses_verification_id_and_code()
    {
        var (task, session) = Verify(PhoneNumberA);
        Assert.False(task.IsCompleted);

        session.OnCodeSent("vid-a");

        Assert.True(task.IsCompletedSuccessfully);
        Assert.Equal("vid-a:123456", GetCredential("123456"));
        Assert.Equal(0, _time.ActiveTimerCount);
    }

    [Fact]
    public void verification_failure_faults_task_with_reported_exception()
    {
        var failure = new InvalidOperationException("invalid phone number");
        var (task, session) = Verify(PhoneNumberA);

        session.OnVerificationFailed(failure);

        var thrown = AssertFaultedWith<InvalidOperationException>(task);
        Assert.Same(failure, thrown);
        Assert.Throws<InvalidOperationException>(() => GetCredential("123456"));
    }

    [Fact]
    public void verification_completed_without_code_completes_task_and_credential_is_used_once()
    {
        var (task, session) = Verify(PhoneNumberA);

        session.OnVerificationCompleted("instant");

        Assert.True(task.IsCompletedSuccessfully);
        Assert.Equal("instant", GetCredential("ignored"));
        Assert.Throws<InvalidOperationException>(() => GetCredential("ignored"));
    }

    [Fact]
    public void auto_retrieval_after_code_sent_keeps_using_verification_id_and_code()
    {
        var (_, session) = Verify(PhoneNumberA);
        session.OnCodeSent("vid-a");

        session.OnVerificationCompleted("auto-retrieved");

        Assert.Equal("vid-a:123456", GetCredential("123456"));
        Assert.Equal("vid-a:654321", GetCredential("654321"));
    }

    [Fact]
    public void duplicate_call_for_same_number_while_pending_shares_task()
    {
        var (first, firstSession) = Verify(PhoneNumberA);

        var (second, secondSession) = Verify(PhoneNumberA);

        Assert.Same(firstSession, secondSession);
        Assert.Same(first, second);
        Assert.False(second.IsCompleted);

        // native drops the second request's callbacks; the first request's callbacks complete both calls
        firstSession.OnCodeSent("vid-a");

        Assert.True(second.IsCompletedSuccessfully);
        Assert.Equal("vid-a:123456", GetCredential("123456"));
    }

    [Fact]
    public void repeat_call_for_same_number_within_window_completes_without_new_code()
    {
        var (_, first) = Verify(PhoneNumberA);
        first.OnCodeSent("vid-a");
        _time.Advance(Window - TimeSpan.FromSeconds(1));

        // native attaches the new callbacks to the open session but doesn't repeat onCodeSent
        var (task, second) = Verify(PhoneNumberA);

        Assert.Same(first, second);
        Assert.True(task.IsCompletedSuccessfully);
        Assert.Equal("vid-a:123456", GetCredential("123456"));
    }

    [Fact]
    public void repeat_call_refreshes_native_session_window()
    {
        var (_, first) = Verify(PhoneNumberA);
        first.OnCodeSent("vid-a");
        _time.Advance(TimeSpan.FromSeconds(100));
        Verify(PhoneNumberA);
        _time.Advance(TimeSpan.FromSeconds(100));

        var (task, third) = Verify(PhoneNumberA);

        Assert.Same(first, third);
        Assert.True(task.IsCompletedSuccessfully);
    }

    [Fact]
    public void repeat_call_after_window_starts_new_verification_and_keeps_earlier_id_until_new_code_is_sent()
    {
        var (_, first) = Verify(PhoneNumberA);
        first.OnCodeSent("vid-a1");
        _time.Advance(Window);

        var (task, second) = Verify(PhoneNumberA);

        Assert.NotSame(first, second);
        Assert.False(task.IsCompleted);
        Assert.Equal("vid-a1:123456", GetCredential("123456"));

        second.OnCodeSent("vid-a2");

        Assert.True(task.IsCompletedSuccessfully);
        Assert.Equal("vid-a2:654321", GetCredential("654321"));
    }

    [Fact]
    public void failed_repeat_verification_faults_its_task_and_keeps_earlier_id()
    {
        var (_, first) = Verify(PhoneNumberA);
        first.OnCodeSent("vid-a1");
        _time.Advance(Window);
        var (task, second) = Verify(PhoneNumberA);

        second.OnVerificationFailed(new InvalidOperationException("quota exceeded"));

        AssertFaultedWith<InvalidOperationException>(task);
        Assert.Equal("vid-a1:123456", GetCredential("123456"));
    }

    [Fact]
    public void late_callbacks_from_replaced_session_are_ignored()
    {
        var (_, first) = Verify(PhoneNumberA);
        first.OnCodeSent("vid-a1");
        _time.Advance(Window);
        var (_, second) = Verify(PhoneNumberA);
        second.OnCodeSent("vid-a2");

        first.OnCodeAutoRetrievalTimeOut("vid-a1");
        first.OnVerificationCompleted("stale-credential");
        first.OnCodeSent("vid-stale");

        Assert.Equal("vid-a2:123456", GetCredential("123456"));
    }

    [Fact]
    public void call_for_same_number_after_native_session_ended_starts_new_verification()
    {
        var (_, first) = Verify(PhoneNumberA);
        first.OnCodeSent("vid-a1");
        first.OnCodeAutoRetrievalTimeOut("vid-a1");

        var (task, second) = Verify(PhoneNumberA);

        Assert.NotSame(first, second);
        Assert.False(task.IsCompleted);
    }

    [Fact]
    public void call_for_same_number_after_failure_starts_new_verification()
    {
        var (_, first) = Verify(PhoneNumberA);
        first.OnVerificationFailed(new InvalidOperationException("quota exceeded"));

        var (task, second) = Verify(PhoneNumberA);

        Assert.NotSame(first, second);
        Assert.False(task.IsCompleted);
    }

    [Fact]
    public void call_for_different_number_cancels_pending_task()
    {
        var (taskA, sessionA) = Verify(PhoneNumberA);

        var (taskB, _) = Verify(PhoneNumberB);
        sessionA.OnCodeSent("vid-a");

        Assert.True(taskA.IsCanceled);
        Assert.False(taskB.IsCompleted);
        Assert.Throws<InvalidOperationException>(() => GetCredential("123456"));
    }

    [Fact]
    public void callbacks_for_other_number_do_not_change_current_number_credential()
    {
        var (_, sessionA) = Verify(PhoneNumberA);
        sessionA.OnCodeSent("vid-a");
        var (_, sessionB) = Verify(PhoneNumberB);
        sessionB.OnCodeSent("vid-b");

        sessionA.OnCodeAutoRetrievalTimeOut("vid-a");
        sessionA.OnVerificationCompleted("account-a");

        Assert.Equal("vid-b:123456", GetCredential("123456"));
    }

    [Fact]
    public void returning_to_earlier_number_within_window_reuses_its_verification_id()
    {
        var (_, sessionA) = Verify(PhoneNumberA);
        sessionA.OnCodeSent("vid-a");
        Verify(PhoneNumberB);

        var (task, sessionA2) = Verify(PhoneNumberA);

        Assert.Same(sessionA, sessionA2);
        Assert.True(task.IsCompletedSuccessfully);
        Assert.Equal("vid-a:123456", GetCredential("123456"));
    }

    [Fact]
    public void returning_to_earlier_number_while_pending_completes_when_its_code_is_sent()
    {
        var (firstA, sessionA) = Verify(PhoneNumberA);
        var (taskB, _) = Verify(PhoneNumberB);

        var (secondA, sessionA2) = Verify(PhoneNumberA);

        Assert.Same(sessionA, sessionA2);
        Assert.True(firstA.IsCanceled);
        Assert.True(taskB.IsCanceled);
        Assert.False(secondA.IsCompleted);

        // the first request's callbacks still report to the session for this number
        sessionA.OnCodeSent("vid-a");

        Assert.True(secondA.IsCompletedSuccessfully);
        Assert.Equal("vid-a:123456", GetCredential("123456"));
    }

    [Fact]
    public void pending_verification_faults_with_timeout_after_native_session_window()
    {
        var (task, _) = Verify(PhoneNumberA);

        _time.Advance(Window - TimeSpan.FromSeconds(1));
        Assert.False(task.IsCompleted);
        _time.Advance(TimeSpan.FromSeconds(1));

        var ex = AssertFaultedWith<TimeoutException>(task);
        Assert.Contains("VerifyPhoneNumberAsync", ex.Message);
        Assert.Throws<InvalidOperationException>(() => GetCredential("123456"));
        Assert.Equal(0, _time.ActiveTimerCount);
    }

    [Fact]
    public void repeat_call_while_pending_extends_timeout()
    {
        var (task, _) = Verify(PhoneNumberA);
        _time.Advance(TimeSpan.FromSeconds(100));
        Verify(PhoneNumberA);

        _time.Advance(TimeSpan.FromSeconds(100));
        Assert.False(task.IsCompleted);
        _time.Advance(TimeSpan.FromSeconds(20));

        AssertFaultedWith<TimeoutException>(task);
    }

    [Fact]
    public void call_after_timeout_starts_new_verification()
    {
        var (_, first) = Verify(PhoneNumberA);
        _time.Advance(Window);

        var (task, second) = Verify(PhoneNumberA);
        second.OnCodeSent("vid-a");

        Assert.NotSame(first, second);
        Assert.True(task.IsCompletedSuccessfully);
        Assert.Equal("vid-a:123456", GetCredential("123456"));
    }

    [Fact]
    public void code_sent_after_timeout_is_still_used()
    {
        var (task, session) = Verify(PhoneNumberA);
        _time.Advance(Window);

        session.OnCodeSent("vid-a");

        Assert.True(task.IsFaulted);
        Assert.Equal("vid-a:123456", GetCredential("123456"));
        var (repeat, repeatSession) = Verify(PhoneNumberA);
        Assert.Same(session, repeatSession);
        Assert.True(repeat.IsCompletedSuccessfully);
    }

    [Fact]
    public void synchronous_native_failure_leaves_earlier_verification_in_place()
    {
        var (_, sessionA) = Verify(PhoneNumberA);
        sessionA.OnCodeSent("vid-a");

        // the native call throws before it schedules any work
        Assert.Throws<ArgumentException>(() => StartWithSynchronousFailure(""));

        Assert.Equal("vid-a:123456", GetCredential("123456"));
        var (task, sessionA2) = Verify(PhoneNumberA);
        Assert.Same(sessionA, sessionA2);
        Assert.True(task.IsCompletedSuccessfully);
    }

    [Fact]
    public void synchronous_native_failure_does_not_cancel_pending_task()
    {
        var (task, session) = Verify(PhoneNumberA);

        Assert.Throws<ArgumentException>(() => StartWithSynchronousFailure(PhoneNumberB));
        session.OnCodeSent("vid-a");

        Assert.True(task.IsCompletedSuccessfully);
        Assert.Equal("vid-a:123456", GetCredential("123456"));
    }

    [Fact]
    public void timeout_without_code_sent_callback_completes_task_with_its_verification_id()
    {
        var (task, session) = Verify(PhoneNumberA);

        // onCodeSent went to callbacks that native dropped; callbacks attached later still get the timeout
        session.OnCodeAutoRetrievalTimeOut("vid-a");

        Assert.True(task.IsCompletedSuccessfully);
        Assert.Equal("vid-a:123456", GetCredential("123456"));
    }

    [Fact]
    public void timeout_after_code_sent_keeps_code_sent_verification_id()
    {
        var (_, session) = Verify(PhoneNumberA);
        session.OnCodeSent("vid-a");

        session.OnCodeAutoRetrievalTimeOut("vid-other");

        Assert.Equal("vid-a:123456", GetCredential("123456"));
    }

    [Fact]
    public void failure_after_code_sent_does_not_change_completed_task()
    {
        var (task, session) = Verify(PhoneNumberA);
        session.OnCodeSent("vid-a");

        session.OnVerificationFailed(new InvalidOperationException("late failure"));

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

    private (Task Task, Session Session) Verify(string phoneNumber)
    {
        Session? session = null;
        var task = _sut.Start(phoneNumber, x => session = x);
        return (task, session!);
    }

    private static TException AssertFaultedWith<TException>(Task task) where TException : Exception
    {
        // checked synchronously so a regression fails the test instead of hanging it
        Assert.True(task.IsFaulted, $"Expected a faulted task, but it is {task.Status}.");
        return Assert.IsType<TException>(task.Exception!.InnerException);
    }

    private void StartWithSynchronousFailure(string phoneNumber)
    {
        _ = _sut.Start(phoneNumber, _ => throw new ArgumentException("native verification failed to start"));
    }

    private string GetCredential(string verificationCode)
    {
        return _sut.GetCredential(verificationCode, (id, code) => $"{id}:{code}");
    }
}