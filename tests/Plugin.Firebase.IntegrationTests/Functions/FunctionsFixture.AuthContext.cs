using System.Text.Json;
using Plugin.Firebase.Auth;
using Plugin.Firebase.Functions;

namespace Plugin.Firebase.IntegrationTests.Functions;

public sealed partial class FunctionsFixture
{
    [Fact]
    public async Task exposes_unauthenticated_callable_context()
    {
        await CrossFirebaseAuth.Current.SignOutAsync();

        var response = await CrossFirebaseFunctions.Current
            .GetHttpsCallable("echoAuthContext")
            .CallAsync<AuthContextResponseData>(new SimpleRequestData(321).ToJson());

        Assert.False(response.HasAuth);
        Assert.Null(response.Uid);
        Assert.Null(response.TokenEmail);
        Assert.Equal(321, response.InputValue);
    }


    [Fact]
    public async Task exposes_authenticated_callable_context()
    {
        var auth = CrossFirebaseAuth.Current;
        var email = IntegrationTestData.UniqueEmail("functions-auth-context");

        await using var user = await AuthTestUserScope.SignInWithEmailAndPasswordAsync(auth, email);
        var response = await CrossFirebaseFunctions.Current
            .GetHttpsCallable("echoAuthContext")
            .CallAsync<AuthContextResponseData>(new SimpleRequestData(654).ToJson());

        Assert.True(response.HasAuth);
        Assert.Equal(user.User.Uid, response.Uid);
        Assert.Equal(email, response.TokenEmail);
        Assert.Equal(654, response.InputValue);
    }

}