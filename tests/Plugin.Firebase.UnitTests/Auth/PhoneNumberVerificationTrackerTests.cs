using Plugin.Firebase.Auth.Internal;

namespace Plugin.Firebase.UnitTests.Auth;

public class PhoneNumberVerificationTrackerTests
{
    private const string PhoneNumberA = "+15555550100";
    private const string PhoneNumberB = "+15555550101";

    private readonly PhoneNumberVerificationTracker<FakeCredential> _sut = new();

    [Fact]
    public void code_sent_completes_task_and_credential_uses_verification_id_and_code()
    {
        var attempt = _sut.Start(PhoneNumberA);
        Assert.False(attempt.Task.IsCompleted);

        attempt.OnCodeSent("vid-a");

        Assert.True(attempt.Task.IsCompletedSuccessfully);
        Assert.Equal(FromCode("vid-a", "123456"), GetCredential("123456"));
    }

    [Fact]
    public async Task verification_failure_faults_task_with_reported_exception()
    {
        var failure = new InvalidOperationException("invalid phone number");
        var attempt = _sut.Start(PhoneNumberA);

        attempt.OnVerificationFailed(failure);

        var thrown = await Assert.ThrowsAsync<InvalidOperationException>(() => attempt.Task);
        Assert.Same(failure, thrown);
        Assert.Throws<InvalidOperationException>(() => GetCredential("123456"));
    }

    [Fact]
    public void instant_verification_completes_task_and_credential_is_used_once()
    {
        var instantCredential = new FakeCredential("instant");
        var attempt = _sut.Start(PhoneNumberA);

        attempt.OnVerificationCompleted(instantCredential);

        Assert.True(attempt.Task.IsCompletedSuccessfully);
        Assert.Same(instantCredential, GetCredential("ignored"));
        Assert.Throws<InvalidOperationException>(() => GetCredential("ignored"));
    }

    [Fact]
    public void auto_retrieval_after_code_sent_keeps_using_verification_id_and_code()
    {
        var attempt = _sut.Start(PhoneNumberA);
        attempt.OnCodeSent("vid-a");

        attempt.OnVerificationCompleted(new FakeCredential("auto-retrieved"));

        Assert.Equal(FromCode("vid-a", "123456"), GetCredential("123456"));
        Assert.Equal(FromCode("vid-a", "654321"), GetCredential("654321"));
    }

    [Fact]
    public void timeout_from_superseded_attempt_does_not_overwrite_newer_verification_id()
    {
        var attemptA = _sut.Start(PhoneNumberA);
        attemptA.OnCodeSent("vid-a");
        var attemptB = _sut.Start(PhoneNumberB);
        attemptB.OnCodeSent("vid-b");

        attemptA.OnCodeAutoRetrievalTimeOut("vid-a");

        Assert.Equal(FromCode("vid-b", "123456"), GetCredential("123456"));
    }

    [Fact]
    public void instant_verification_from_superseded_attempt_is_ignored()
    {
        var attemptA = _sut.Start(PhoneNumberA);
        var attemptB = _sut.Start(PhoneNumberB);

        attemptA.OnVerificationCompleted(new FakeCredential("account-a"));
        attemptB.OnCodeSent("vid-b");

        Assert.Equal(FromCode("vid-b", "123456"), GetCredential("123456"));
    }

    [Fact]
    public void call_for_different_number_cancels_pending_attempt()
    {
        var attemptA = _sut.Start(PhoneNumberA);

        var attemptB = _sut.Start(PhoneNumberB);
        attemptA.OnCodeSent("vid-a");

        Assert.NotSame(attemptA, attemptB);
        Assert.True(attemptA.Task.IsCanceled);
        Assert.False(attemptB.Task.IsCompleted);
        Assert.Throws<InvalidOperationException>(() => GetCredential("123456"));
    }

    [Fact]
    public void duplicate_call_for_same_number_while_pending_shares_pending_task()
    {
        var first = _sut.Start(PhoneNumberA);

        var second = _sut.Start(PhoneNumberA);

        Assert.Same(first, second);
        Assert.False(second.Task.IsCompleted);

        // the native SDK drops the second request's callbacks; the first request's callbacks complete both calls
        first.OnCodeSent("vid-a");

        Assert.True(second.Task.IsCompletedSuccessfully);
        Assert.Equal(FromCode("vid-a", "123456"), GetCredential("123456"));
    }

    [Fact]
    public void repeat_call_for_same_number_after_code_sent_completes_without_new_code_sent_callback()
    {
        var first = _sut.Start(PhoneNumberA);
        first.OnCodeSent("vid-a");

        // the native SDK attaches the new callbacks to the open session but doesn't repeat onCodeSent
        var second = _sut.Start(PhoneNumberA);

        Assert.True(second.Task.IsCompletedSuccessfully);
        Assert.Equal(FromCode("vid-a", "123456"), GetCredential("123456"));
    }

    [Fact]
    public void call_for_same_number_after_native_session_ended_starts_new_attempt()
    {
        var first = _sut.Start(PhoneNumberA);
        first.OnCodeSent("vid-a1");
        first.OnCodeAutoRetrievalTimeOut("vid-a1");

        var second = _sut.Start(PhoneNumberA);
        first.OnCodeSent("vid-stale");

        Assert.NotSame(first, second);
        Assert.False(second.Task.IsCompleted);

        second.OnCodeSent("vid-a2");

        Assert.True(second.Task.IsCompletedSuccessfully);
        Assert.Equal(FromCode("vid-a2", "123456"), GetCredential("123456"));
    }

    [Fact]
    public void call_for_same_number_after_failure_starts_new_attempt()
    {
        var first = _sut.Start(PhoneNumberA);
        first.OnVerificationFailed(new InvalidOperationException("quota exceeded"));

        var second = _sut.Start(PhoneNumberA);

        Assert.NotSame(first, second);
        Assert.False(second.Task.IsCompleted);
    }

    [Fact]
    public void timeout_without_code_sent_callback_completes_attempt_with_its_verification_id()
    {
        var attempt = _sut.Start(PhoneNumberA);

        attempt.OnCodeAutoRetrievalTimeOut("vid-a");

        Assert.True(attempt.Task.IsCompletedSuccessfully);
        Assert.Equal(FromCode("vid-a", "123456"), GetCredential("123456"));
    }

    [Fact]
    public void timeout_after_code_sent_keeps_code_sent_verification_id()
    {
        var attempt = _sut.Start(PhoneNumberA);
        attempt.OnCodeSent("vid-a");

        attempt.OnCodeAutoRetrievalTimeOut("vid-other");

        Assert.Equal(FromCode("vid-a", "123456"), GetCredential("123456"));
    }

    [Fact]
    public void failure_after_code_sent_does_not_change_completed_task()
    {
        var attempt = _sut.Start(PhoneNumberA);
        attempt.OnCodeSent("vid-a");

        attempt.OnVerificationFailed(new InvalidOperationException("late failure"));

        Assert.True(attempt.Task.IsCompletedSuccessfully);
        Assert.Equal(FromCode("vid-a", "123456"), GetCredential("123456"));
    }

    [Fact]
    public async Task code_sent_after_failure_does_not_change_faulted_task()
    {
        var attempt = _sut.Start(PhoneNumberA);
        attempt.OnVerificationFailed(new InvalidOperationException("invalid phone number"));

        attempt.OnCodeSent("vid-a");

        await Assert.ThrowsAsync<InvalidOperationException>(() => attempt.Task);
    }

    [Fact]
    public void get_credential_before_verification_throws()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => GetCredential("123456"));

        Assert.Contains("VerifyPhoneNumberAsync", ex.Message);
    }

    [Fact]
    public void get_credential_while_verification_is_pending_throws()
    {
        _sut.Start(PhoneNumberA);

        Assert.Throws<InvalidOperationException>(() => GetCredential("123456"));
    }

    private FakeCredential GetCredential(string verificationCode)
    {
        return _sut.GetCredential(verificationCode, FromCode);
    }

    private static FakeCredential FromCode(string verificationId, string verificationCode)
    {
        return new FakeCredential($"{verificationId}:{verificationCode}");
    }

    private sealed record FakeCredential(string Value);
}