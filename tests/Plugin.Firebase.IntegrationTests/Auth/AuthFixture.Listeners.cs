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
        public async Task invokes_auth_state_listener_on_sign_in_and_sign_out()
        {
            var sut = CrossFirebaseAuth.Current;
            var email = IntegrationTestData.UniqueEmail("auth-state");
            await using var user = await AuthTestUserScope.CreateWithEmailAndPasswordAsync(sut, email);
            await sut.SignOutAsync();
            var sawSignedIn = new CallbackProbe<bool>();
            var sawSignedOutAfterSignIn = new CallbackProbe<bool>();

            using var listener = sut.AddAuthStateListener(auth => {
                if(auth.CurrentUser != null) {
                    sawSignedIn.TrySetResult(true);
                } else if(sawSignedIn.IsCompleted) {
                    sawSignedOutAfterSignIn.TrySetResult(true);
                }
            });

            await sut.SignInWithEmailAndPasswordAsync(
                email,
                IntegrationTestUsers.DefaultPassword,
                createsUserAutomatically: false);
            await sawSignedIn.WaitAsync(
                IntegrationTestTimeouts.Callback,
                "auth state listener sign-in");

            await sut.SignOutAsync();
            await sawSignedOutAfterSignIn.WaitAsync(
                IntegrationTestTimeouts.Callback,
                "auth state listener sign-out");

            await sut.SignInWithEmailAndPasswordAsync(
                email,
                IntegrationTestUsers.DefaultPassword,
                createsUserAutomatically: false);
            Assert.NotNull(sut.CurrentUser);
        }

    }
}