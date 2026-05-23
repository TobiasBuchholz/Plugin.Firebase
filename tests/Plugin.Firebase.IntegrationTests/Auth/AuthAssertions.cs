using Plugin.Firebase.Auth;
using Plugin.Firebase.Core.Exceptions;

namespace Plugin.Firebase.IntegrationTests.Auth;

internal static class AuthAssertions
{
    public static void NativeAuthExceptionCaptured(CrossPlatformFirebaseAuthException exception)
    {
        Assert.NotNull(exception.InnerException);
        Assert.False(string.IsNullOrWhiteSpace(exception.NativeExceptionTypeName));
        Assert.False(string.IsNullOrWhiteSpace(exception.NativeErrorMessage));
    }

    public static void NestedCustomClaims(IAuthTokenResult idTokenResult)
    {
        Assert.True(idTokenResult.GetClaim<bool>("is_awesome"));
        var nestedObject = Assert.IsAssignableFrom<IDictionary<string, object>>(
            idTokenResult.Claims["nested_object"]
        );
        NestedClaimAssertions.AssertNestedCustomClaim(nestedObject);

        var typedNestedObject = idTokenResult.GetClaim<IDictionary<string, object>>(
            "nested_object"
        );
        NestedClaimAssertions.AssertNestedCustomClaim(typedNestedObject);

        var concreteNestedObject = idTokenResult.GetClaim<Dictionary<string, object>>(
            "nested_object"
        );
        NestedClaimAssertions.AssertNestedCustomClaim(concreteNestedObject);

        var objectNestedObject = Assert.IsAssignableFrom<IDictionary<string, object>>(
            idTokenResult.GetClaim<object>("nested_object")
        );
        NestedClaimAssertions.AssertNestedCustomClaim(objectNestedObject);

        var nestedArray = Assert.IsAssignableFrom<IList<object>>(
            idTokenResult.Claims["nested_array"]
        );
        NestedClaimAssertions.AssertNestedCustomArray(nestedArray);

        var typedNestedArray = idTokenResult.GetClaim<IList<object>>("nested_array");
        NestedClaimAssertions.AssertNestedCustomArray(typedNestedArray);

        var concreteNestedArray = idTokenResult.GetClaim<List<object>>("nested_array");
        NestedClaimAssertions.AssertNestedCustomArray(concreteNestedArray);

        var objectNestedArray = Assert.IsAssignableFrom<IList<object>>(
            idTokenResult.GetClaim<object>("nested_array")
        );
        NestedClaimAssertions.AssertNestedCustomArray(objectNestedArray);

        Assert.True(Assert.IsType<bool>(idTokenResult.GetClaim<object>("is_awesome")));
    }
}