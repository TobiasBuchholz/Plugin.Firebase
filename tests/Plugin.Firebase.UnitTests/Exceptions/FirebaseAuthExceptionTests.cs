using Plugin.Firebase.Core.Exceptions;

namespace Plugin.Firebase.UnitTests.Exceptions;

public class FirebaseAuthExceptionTests
{
    [Fact]
    public void Ctor_WithReason_SetsReason()
    {
        var ex = new FirebaseAuthException(FIRAuthError.InvalidEmail);

        Assert.Equal(FIRAuthError.InvalidEmail, ex.Reason);
    }

    [Fact]
    public void Ctor_WithReasonAndMessage_SetsReasonAndMessage()
    {
        var ex = new FirebaseAuthException(FIRAuthError.WrongPassword, "nope");

        Assert.Equal(FIRAuthError.WrongPassword, ex.Reason);
        Assert.Equal("nope", ex.Message);
    }

    [Fact]
    public void Ctor_WithReasonMessageAndInner_SetsAll()
    {
        var inner = new Exception("inner");
        var ex = new FirebaseAuthException(FIRAuthError.UserDisabled, "disabled", inner);

        Assert.Equal(FIRAuthError.UserDisabled, ex.Reason);
        Assert.Equal("disabled", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }

}