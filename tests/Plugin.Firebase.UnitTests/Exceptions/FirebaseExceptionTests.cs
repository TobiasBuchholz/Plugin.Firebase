using Plugin.Firebase.Core.Exceptions;

namespace Plugin.Firebase.UnitTests.Exceptions;

public class FirebaseExceptionTests
{
    [Fact]
    public void Ctor_WithMessage_SetsMessage()
    {
        var ex = new FirebaseException("hello");

        Assert.Equal("hello", ex.Message);
    }

    [Fact]
    public void Ctor_WithMessageAndInner_SetsInner()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new FirebaseException("outer", inner);

        Assert.Equal("outer", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }
}