using Plugin.Firebase.Auth;
using Plugin.Firebase.Core.Exceptions;

namespace Plugin.Firebase.IntegrationTests.Auth
{
    [Collection("Sequential")]
    [TestLogging]
    [Preserve(AllMembers = true)]
    public sealed class AuthFixture : IAsyncLifetime
    {
        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public async Task creates_user_with_email_and_password()
        {
            var sut = CrossFirebaseAuth.Current;
            await sut.CreateUserAsync("created-user@test.com", "123456");

            Assert.NotNull(sut.CurrentUser);

            await sut.CurrentUser.DeleteAsync();
            Assert.Null(sut.CurrentUser);
        }

        [Fact]
        public async Task signs_in_user_via_email_and_password()
        {
            var sut = CrossFirebaseAuth.Current;
            var user = await sut.SignInWithEmailAndPasswordAsync("sign-in-with-pw@test.com", "123456");
            Assert.Equal("sign-in-with-pw@test.com", user.Email);
            Assert.Equal("sign-in-with-pw@test.com", sut.CurrentUser.Email);
        }

        [Fact]
        public async Task sign_in_with_email_and_password_creates_user_automatically()
        {
            var sut = CrossFirebaseAuth.Current;
            var email = CreateUniqueEmail("auto-create-sign-in");

            var user = await sut.SignInWithEmailAndPasswordAsync(email, "123456");

            Assert.NotNull(sut.CurrentUser);
            Assert.Equal(email, user.Email);
            Assert.Equal(email, sut.CurrentUser.Email);
            Assert.Equal(user.Uid, sut.CurrentUser.Uid);
        }

        [Fact]
        public async Task throws_error_if_credentials_are_invalid_when_signing_in_user_via_email_and_password()
        {
            var sut = CrossFirebaseAuth.Current;
            var ex = await Assert.ThrowsAnyAsync<CrossPlatformFirebaseAuthException>(
                () => sut.SignInWithEmailAndPasswordAsync("sign-in-with-pw@test.com", "000000", createsUserAutomatically: false)
            );

            AssertNativeAuthExceptionCaptured(ex);
        }

        [Fact]
        public async Task throws_error_if_user_does_not_exist_and_should_not_be_created_automatically_due_sign_in_via_email_and_password()
        {
            var sut = CrossFirebaseAuth.Current;
            var ex = await Assert.ThrowsAnyAsync<CrossPlatformFirebaseAuthException>(
                () => sut.SignInWithEmailAndPasswordAsync("does-not-exist@test.com", "123456", createsUserAutomatically: false)
            );

            AssertNativeAuthExceptionCaptured(ex);
        }

        [Fact]
        public async Task signs_in_user_anonymously()
        {
            var sut = CrossFirebaseAuth.Current;
            Assert.Null(sut.CurrentUser);

            var user = await sut.SignInAnonymouslyAsync();
            Assert.NotNull(user);
            Assert.NotNull(sut.CurrentUser);
            Assert.True(user.IsAnonymous);
        }

        [Fact]
        public async Task links_anonymous_user_with_email_and_password()
        {
            var sut = CrossFirebaseAuth.Current;
            var anonymousUser = await sut.SignInAnonymouslyAsync();
            var email = CreateUniqueEmail("link-anonymous");

            var linkedUser = await sut.LinkWithEmailAndPasswordAsync(email, "123456");

            Assert.Equal(anonymousUser.Uid, linkedUser.Uid);
            Assert.Equal(anonymousUser.Uid, sut.CurrentUser.Uid);
            Assert.False(linkedUser.IsAnonymous);
            Assert.False(sut.CurrentUser.IsAnonymous);
            Assert.Equal(email, linkedUser.Email);
            Assert.Equal(email, sut.CurrentUser.Email);
        }

        [Fact]
        public async Task signs_out_user()
        {
            var sut = CrossFirebaseAuth.Current;
            await sut.SignInWithEmailAndPasswordAsync("sign-out@test.com", "123456");
            Assert.NotNull(sut.CurrentUser);

            await sut.SignOutAsync();
            Assert.Null(sut.CurrentUser);
        }

        // Firebase now requires verify-before-update for newer projects on iOS and Android,
        // so this direct email update path only works with deprecated project configuration.
#if IOS || ANDROID
        [Fact(Skip = "Firebase direct email updates on iOS and Android rely on deprecated project configuration.")]
#else
        [Fact]
#endif
        public async Task updates_user_email()
        {
            var sut = CrossFirebaseAuth.Current;
            await sut.SignInWithEmailAndPasswordAsync("to-update-email@test.com", "123456");
            Assert.NotNull(sut.CurrentUser);

            await sut.CurrentUser.UpdateEmailAsync("updated@test.com");
            Assert.Equal("updated@test.com", sut.CurrentUser.Email);
        }

        [Fact]
        public async Task updates_user_password()
        {
            const string email = "to-update-pw@test.com";
            var sut = CrossFirebaseAuth.Current;
            await sut.SignInWithEmailAndPasswordAsync(email, "123456");
            Assert.NotNull(sut.CurrentUser);

            await sut.CurrentUser.UpdatePasswordAsync("abcdefgh");
            await sut.SignOutAsync();
            Assert.Null(sut.CurrentUser);

            await Assert.ThrowsAnyAsync<Exception>(() => sut.SignInWithEmailAndPasswordAsync(email, "123456"));
            await sut.SignInWithEmailAndPasswordAsync(email, "abcdefgh");
            Assert.NotNull(sut.CurrentUser);
        }

        [Fact]
        public async Task updates_user_profile()
        {
            const string displayName = "Bruce Wayne";
            const string photoUrl = "https://url.to/image.jpg";
            var sut = CrossFirebaseAuth.Current;
            await sut.SignInWithEmailAndPasswordAsync("to-update-profile@test.com", "123456");
            Assert.NotNull(sut.CurrentUser);
            Assert.Null(sut.CurrentUser.DisplayName);
            Assert.Null(sut.CurrentUser.PhotoUrl);

            await sut.CurrentUser.UpdateProfileAsync(displayName, photoUrl);
            Assert.Equal(displayName, sut.CurrentUser.DisplayName);
            Assert.Equal(photoUrl, sut.CurrentUser.PhotoUrl);

            await sut.CurrentUser.UpdateProfileAsync(displayName: null);
            Assert.Null(sut.CurrentUser.DisplayName);
            Assert.Equal(photoUrl, sut.CurrentUser.PhotoUrl);

            await sut.CurrentUser.UpdateProfileAsync(displayName);
            Assert.Equal(displayName, sut.CurrentUser.DisplayName);
            Assert.Equal(photoUrl, sut.CurrentUser.PhotoUrl);

            await sut.CurrentUser.UpdateProfileAsync(photoUrl: null);
            Assert.Equal(displayName, sut.CurrentUser.DisplayName);
            Assert.Null(sut.CurrentUser.PhotoUrl);

            await sut.CurrentUser.UpdateProfileAsync(photoUrl: photoUrl);
            Assert.Equal(displayName, sut.CurrentUser.DisplayName);
            Assert.Equal(photoUrl, sut.CurrentUser.PhotoUrl);

            await sut.CurrentUser.UpdateProfileAsync(displayName, photoUrl);
            Assert.Equal(displayName, sut.CurrentUser.DisplayName);
            Assert.Equal(photoUrl, sut.CurrentUser.PhotoUrl);
        }

        [Fact]
        public async Task sends_verification_email()
        {
            var sut = CrossFirebaseAuth.Current;
            await sut.SignInWithEmailAndPasswordAsync("verification-email@test.com", "123456");
            Assert.NotNull(sut.CurrentUser);

            await sut.CurrentUser.SendEmailVerificationAsync();
        }

        [Fact]
        public async Task sends_password_reset_email_for_current_user()
        {
            var sut = CrossFirebaseAuth.Current;
            var email = CreateUniqueEmail("pw-reset-current");
            await sut.SignInWithEmailAndPasswordAsync(email, "123456");

            await sut.SendPasswordResetEmailAsync();
        }

        [Fact]
        public async Task sends_password_reset_email_for_explicit_email()
        {
            var sut = CrossFirebaseAuth.Current;
            var email = CreateUniqueEmail("pw-reset-explicit");
            await sut.SignInWithEmailAndPasswordAsync(email, "123456");

            await sut.SendPasswordResetEmailAsync(email);
        }

        [Fact]
        public async Task reloads_current_user()
        {
            var sut = CrossFirebaseAuth.Current;
            await sut.SignInWithEmailAndPasswordAsync("reload-current-user@test.com", "123456");
            Assert.NotNull(sut.CurrentUser);

            var uid = sut.CurrentUser.Uid;
            await sut.ReloadCurrentUserAsync();

            Assert.NotNull(sut.CurrentUser);
            Assert.Equal(uid, sut.CurrentUser.Uid);
        }

        [Fact]
        public async Task invokes_auth_state_listener_on_sign_in_and_sign_out()
        {
            var sut = CrossFirebaseAuth.Current;
            var email = CreateUniqueEmail("auth-state");
            await sut.CreateUserAsync(email, "123456");
            await sut.SignOutAsync();
            var sawSignedIn = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var sawSignedOutAfterSignIn = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            using var listener = sut.AddAuthStateListener(auth => {
                if(auth.CurrentUser != null) {
                    sawSignedIn.TrySetResult(true);
                } else if(sawSignedIn.Task.IsCompleted) {
                    sawSignedOutAfterSignIn.TrySetResult(true);
                }
            });

            await sut.SignInWithEmailAndPasswordAsync(email, "123456", createsUserAutomatically: false);
            await sawSignedIn.Task.WaitAsync(TimeSpan.FromSeconds(10));

            await sut.SignOutAsync();
            await sawSignedOutAfterSignIn.Task.WaitAsync(TimeSpan.FromSeconds(10));

            await sut.SignInWithEmailAndPasswordAsync(email, "123456", createsUserAutomatically: false);
            await sut.CurrentUser.DeleteAsync();
        }

        [Fact]
        public async Task sets_language_code()
        {
            var sut = CrossFirebaseAuth.Current;
            await sut.SignInWithEmailAndPasswordAsync("set-language-code@test.com", "123456");

            var ex = Record.Exception(() => {
                sut.LanguageCode = "fr";
                sut.UseAppLanguage();
            });
            Assert.Null(ex);
        }

        [Fact]
        public async Task exposes_user_metadata_and_provider_infos_after_sign_in()
        {
            var sut = CrossFirebaseAuth.Current;
            var email = CreateUniqueEmail("user-metadata");
            var user = await sut.SignInWithEmailAndPasswordAsync(email, "123456");

            Assert.False(string.IsNullOrWhiteSpace(user.ProviderId));
            Assert.NotNull(user.ProviderInfos);
            Assert.Contains(user.ProviderInfos, x => x.Email == email);
            Assert.NotNull(user.Metadata);
            Assert.NotEqual(default, user.Metadata.CreationDate);
            Assert.NotEqual(default, user.Metadata.LastSignInDate);
        }

        [Fact]
        public async Task deletes_user()
        {
            var sut = CrossFirebaseAuth.Current;
            var user = await sut.SignInWithEmailAndPasswordAsync("to-delete@test.com", "123456");
            Assert.NotNull(sut.CurrentUser);

            await user.DeleteAsync();
            Assert.Null(sut.CurrentUser);
        }

        [Fact]
        public async Task retrieves_custom_claims()
        {
            var sut = CrossFirebaseAuth.Current;
            var user = await sut.SignInWithEmailAndPasswordAsync("custom-claims@test.com", "123456");
            var idTokenResult = await user.GetIdTokenResultAsync();
            await sut.SignOutAsync(); // sign out so the user won't get deleted

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

        [Fact]
        public async Task exposes_id_token_metadata()
        {
            var sut = CrossFirebaseAuth.Current;
            var email = CreateUniqueEmail("token-metadata");
            var user = await sut.SignInWithEmailAndPasswordAsync(email, "123456");

            var idTokenResult = await user.GetIdTokenResultAsync();

            Assert.False(string.IsNullOrWhiteSpace(idTokenResult.Token));
            Assert.NotNull(idTokenResult.Claims);
            Assert.NotEmpty(idTokenResult.Claims);
            Assert.NotEqual(default, idTokenResult.AuthDate);
            Assert.NotEqual(default, idTokenResult.IssuedAtDate);
            Assert.NotEqual(default, idTokenResult.ExpirationDate);
        }

        public async Task DisposeAsync()
        {
            var sut = CrossFirebaseAuth.Current;
            if(sut.CurrentUser != null) {
                await sut.CurrentUser.DeleteAsync();
            }
            await sut.SignOutAsync();
        }

        private static void AssertNativeAuthExceptionCaptured(CrossPlatformFirebaseAuthException exception)
        {
            Assert.NotNull(exception.InnerException);
            Assert.False(string.IsNullOrWhiteSpace(exception.NativeExceptionTypeName));
            Assert.False(string.IsNullOrWhiteSpace(exception.NativeErrorMessage));
        }

        private static string CreateUniqueEmail(string prefix)
        {
            return $"{prefix}-{Guid.NewGuid():N}@test.com";
        }
    }
}