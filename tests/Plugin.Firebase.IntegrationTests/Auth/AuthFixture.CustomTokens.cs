using System.Text.Json;
using Plugin.Firebase.Auth;
using Plugin.Firebase.Core.Exceptions;
using Plugin.Firebase.Functions;
using Plugin.Firebase.IntegrationTests.Functions;

namespace Plugin.Firebase.IntegrationTests.Auth
{
    public sealed partial class AuthFixture
    {
        [Fact]
        public async Task signs_in_user_via_custom_token()
        {
            var auth = CrossFirebaseAuth.Current;
            var uid = IntegrationTestData.UniqueId("custom-token");
            var claims = AuthTestPayloads.CreateNestedCustomClaims();
            var requestJson = JsonSerializer.Serialize(new {
                uid,
                claims,
            });
            var response = await CrossFirebaseFunctions.Current
                .GetHttpsCallable("createCustomToken")
                .CallAsync<CustomTokenResponseData>(requestJson);

            await auth.SignOutAsync();
            var user = await auth.SignInWithCustomTokenAsync(response.Token);
            await using var testUser = AuthTestUserScope.TrackCurrentUser(auth);
            var idTokenResult = await user.GetIdTokenResultAsync(forceRefresh: true);

            Assert.Equal(uid, response.Uid);
            Assert.Equal(uid, user.Uid);
            Assert.Equal(uid, auth.CurrentUser!.Uid);
            AuthAssertions.NestedCustomClaims(idTokenResult);
        }


        [Fact]
        public async Task retrieves_custom_claims()
        {
            var sut = CrossFirebaseAuth.Current;
            await using var user = await AuthTestUserScope.SignInWithEmailAndPasswordAsync(
                sut,
                IntegrationTestUsers.CustomClaimsEmail,
                deleteOnDispose: false);
            var idTokenResult = await user.User.GetIdTokenResultAsync();

            AuthAssertions.NestedCustomClaims(idTokenResult);
        }


        [Fact]
        public async Task exposes_id_token_metadata()
        {
            var sut = CrossFirebaseAuth.Current;
            var email = IntegrationTestData.UniqueEmail("token-metadata");
            await using var user = await AuthTestUserScope.SignInWithEmailAndPasswordAsync(sut, email);

            var idTokenResult = await user.User.GetIdTokenResultAsync();
            var refreshedIdTokenResult = await user.User.GetIdTokenResultAsync(forceRefresh: true);

            Assert.False(string.IsNullOrWhiteSpace(idTokenResult.Token));
            Assert.NotNull(idTokenResult.Claims);
            Assert.NotEmpty(idTokenResult.Claims);
            Assert.NotEqual(default, idTokenResult.AuthDate);
            Assert.NotEqual(default, idTokenResult.IssuedAtDate);
            Assert.NotEqual(default, idTokenResult.ExpirationDate);
            Assert.False(string.IsNullOrWhiteSpace(refreshedIdTokenResult.Token));
            Assert.False(string.IsNullOrWhiteSpace(idTokenResult.SignInProvider));
        }

    }
}