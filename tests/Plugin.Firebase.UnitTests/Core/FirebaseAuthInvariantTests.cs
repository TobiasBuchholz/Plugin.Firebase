using Plugin.Firebase.Auth;
using Plugin.Firebase.Core.Exceptions;

namespace Plugin.Firebase.UnitTests.Core;

public class FirebaseAuthInvariantTests
{
    [Fact]
    public void EnsureNotNull_ReturnsSameInstance_WhenValueIsPresent()
    {
        var value = new object();

        var result = FirebaseAuthInvariant.EnsureNotNull(value, "should not throw");

        Assert.Same(value, result);
    }

    [Fact]
    public void EnsureNotNull_ThrowsFirebaseException_WhenValueIsNull()
    {
        var ex = Assert.Throws<FirebaseException>(
            () => FirebaseAuthInvariant.EnsureNotNull<object>(null, "Auth returned a null user.")
        );

        Assert.Equal("Auth returned a null user.", ex.Message);
    }
}
