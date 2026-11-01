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
        public async Task creates_user_with_email_and_password()
        {
            var sut = CrossFirebaseAuth.Current;
            await using var user = await AuthTestUserScope.CreateWithEmailAndPasswordAsync(
                sut,
                IntegrationTestUsers.CreatedUserEmail);

            Assert.NotNull(sut.CurrentUser);
            Assert.Equal(user.User.Uid, sut.CurrentUser!.Uid);
        }


        [Fact]
        public async Task signs_in_user_via_email_and_password()
        {
            var sut = CrossFirebaseAuth.Current;
            await using var user = await AuthTestUserScope.SignInWithEmailAndPasswordAsync(
                sut,
                IntegrationTestUsers.SignInWithPasswordEmail,
                deleteOnDispose: false);
            Assert.Equal(IntegrationTestUsers.SignInWithPasswordEmail, user.User.Email);
            Assert.Equal(IntegrationTestUsers.SignInWithPasswordEmail, sut.CurrentUser!.Email);
        }


        [Fact]
        public async Task signs_in_user_with_native_credential()
        {
            var sut = CrossFirebaseAuth.Current;
            var email = IntegrationTestData.UniqueEmail("native-sign-in");
            await using var createdUser = await AuthTestUserScope.CreateWithEmailAndPasswordAsync(sut, email);
            await sut.SignOutAsync();
            var credential = CreateNativeEmailCredential(email, IntegrationTestUsers.DefaultPassword);

            var user = await sut.SignInWithCredentialAsync(credential);

            Assert.Equal(email, user.Email);
            Assert.NotNull(sut.CurrentUser);
            Assert.Equal(email, sut.CurrentUser!.Email);
            Assert.Equal(user.Uid, sut.CurrentUser!.Uid);
        }


        [Fact]
        public async Task sign_in_with_email_and_password_creates_user_automatically()
        {
            var sut = CrossFirebaseAuth.Current;
            var email = IntegrationTestData.UniqueEmail("auto-create-sign-in");

            await using var user = await AuthTestUserScope.SignInWithEmailAndPasswordAsync(sut, email);

            Assert.NotNull(sut.CurrentUser);
            Assert.Equal(email, user.User.Email);
            Assert.Equal(email, sut.CurrentUser!.Email);
            Assert.Equal(user.User.Uid, sut.CurrentUser!.Uid);
        }


        [Fact]
        public async Task throws_error_if_credentials_are_invalid_when_signing_in_user_via_email_and_password()
        {
            var sut = CrossFirebaseAuth.Current;
            var ex = await Assert.ThrowsAnyAsync<CrossPlatformFirebaseAuthException>(
                () => sut.SignInWithEmailAndPasswordAsync(
                    IntegrationTestUsers.SignInWithPasswordEmail,
                    "000000",
                    createsUserAutomatically: false)
            );

            AuthAssertions.NativeAuthExceptionCaptured(ex);
        }


        [Fact]
        public async Task throws_error_if_user_does_not_exist_and_should_not_be_created_automatically_due_sign_in_via_email_and_password()
        {
            var sut = CrossFirebaseAuth.Current;
            var ex = await Assert.ThrowsAnyAsync<CrossPlatformFirebaseAuthException>(
                () => sut.SignInWithEmailAndPasswordAsync(
                    IntegrationTestUsers.MissingUserEmail,
                    IntegrationTestUsers.DefaultPassword,
                    createsUserAutomatically: false)
            );

            AuthAssertions.NativeAuthExceptionCaptured(ex);
        }


        [Fact]
        public async Task throws_cross_platform_exception_for_invalid_native_credential()
        {
            var sut = CrossFirebaseAuth.Current;
            var credential = CreateNativeEmailCredential(
                IntegrationTestData.UniqueEmail("invalid-native-sign-in"),
                "000000");

            var ex = await Assert.ThrowsAnyAsync<CrossPlatformFirebaseAuthException>(
                () => sut.SignInWithCredentialAsync(credential)
            );

            AuthAssertions.NativeAuthExceptionCaptured(ex);
        }


        [Fact]
        public async Task signs_in_user_anonymously()
        {
            var sut = CrossFirebaseAuth.Current;
            Assert.Null(sut.CurrentUser);

            await using var user = await AuthTestUserScope.SignInAnonymouslyAsync(sut);
            Assert.NotNull(user.User);
            Assert.NotNull(sut.CurrentUser);
            Assert.True(user.User.IsAnonymous);
        }


        [Fact]
        public async Task links_anonymous_user_with_email_and_password()
        {
            var sut = CrossFirebaseAuth.Current;
            await using var anonymousUser = await AuthTestUserScope.SignInAnonymouslyAsync(sut);
            var email = IntegrationTestData.UniqueEmail("link-anonymous");

            var linkedUser = await sut.LinkWithEmailAndPasswordAsync(email, IntegrationTestUsers.DefaultPassword);

            Assert.Equal(anonymousUser.User.Uid, linkedUser.Uid);
            Assert.Equal(anonymousUser.User.Uid, sut.CurrentUser!.Uid);
            Assert.False(linkedUser.IsAnonymous);
            Assert.False(sut.CurrentUser!.IsAnonymous);
            Assert.Equal(email, linkedUser.Email);
            Assert.Equal(email, sut.CurrentUser!.Email);
        }


        [Fact]
        public async Task links_anonymous_user_with_native_credential()
        {
            var sut = CrossFirebaseAuth.Current;
            await using var anonymousUser = await AuthTestUserScope.SignInAnonymouslyAsync(sut);
            var email = IntegrationTestData.UniqueEmail("native-link");
            var credential = CreateNativeEmailCredential(email, IntegrationTestUsers.DefaultPassword);

            var linkedUser = await sut.LinkWithCredentialAsync(credential);

            Assert.Equal(anonymousUser.User.Uid, linkedUser.Uid);
            Assert.NotNull(sut.CurrentUser);
            Assert.Equal(anonymousUser.User.Uid, sut.CurrentUser!.Uid);
            Assert.False(linkedUser.IsAnonymous);
            Assert.False(sut.CurrentUser!.IsAnonymous);
            Assert.Equal(email, linkedUser.Email);
            Assert.Equal(email, sut.CurrentUser!.Email);
        }


        [Fact]
        public async Task unlinks_email_password_provider_from_linked_user()
        {
            var sut = CrossFirebaseAuth.Current;
            await using var user = await AuthTestUserScope.SignInAnonymouslyAsync(sut);
            var linkedUser = await sut.LinkWithEmailAndPasswordAsync(
                IntegrationTestData.UniqueEmail("unlink-provider"),
                IntegrationTestUsers.DefaultPassword);

            Assert.NotNull(linkedUser.ProviderInfos);
            Assert.Contains(linkedUser.ProviderInfos, x => x.ProviderId == "password");

            Assert.NotNull(sut.CurrentUser);
            await sut.CurrentUser!.UnlinkAsync("password");
            await sut.CurrentUser!.ReloadAsync();

            Assert.DoesNotContain(sut.CurrentUser!.ProviderInfos ?? Array.Empty<ProviderInfo>(), x => x.ProviderId == "password");
        }

        [Fact]
        public async Task reauthenticates_user_with_email_and_password()
        {
            var sut = CrossFirebaseAuth.Current;
            var email = IntegrationTestData.UniqueEmail("reauth-email-password");
            await using var user = await AuthTestUserScope.SignInWithEmailAndPasswordAsync(sut, email);
            var uid = user.User.Uid;

            await sut.CurrentUser!.ReauthenticateWithEmailAndPasswordAsync(
                email,
                IntegrationTestUsers.DefaultPassword);
            await sut.CurrentUser!.UpdatePasswordAsync(IntegrationTestUsers.UpdatedPassword);

            Assert.NotNull(sut.CurrentUser);
            Assert.Equal(uid, sut.CurrentUser!.Uid);

            await sut.SignOutAsync();
            await Assert.ThrowsAnyAsync<CrossPlatformFirebaseAuthException>(
                () => sut.SignInWithEmailAndPasswordAsync(
                    email,
                    IntegrationTestUsers.DefaultPassword,
                    createsUserAutomatically: false)
            );

            var updatedUser = await sut.SignInWithEmailAndPasswordAsync(
                email,
                IntegrationTestUsers.UpdatedPassword,
                createsUserAutomatically: false);
            Assert.Equal(uid, updatedUser.Uid);
        }


        [Fact]
        public async Task throws_error_if_reauthenticating_with_invalid_email_password()
        {
            var sut = CrossFirebaseAuth.Current;
            var email = IntegrationTestData.UniqueEmail("reauth-invalid");
            await using var user = await AuthTestUserScope.SignInWithEmailAndPasswordAsync(sut, email);

            var ex = await Assert.ThrowsAnyAsync<CrossPlatformFirebaseAuthException>(
                () => sut.CurrentUser!.ReauthenticateWithEmailAndPasswordAsync(email, "000000")
            );

            AuthAssertions.NativeAuthExceptionCaptured(ex);
        }

        private static global::Firebase.Auth.AuthCredential CreateNativeEmailCredential(
            string email,
            string password
        )
        {
#if ANDROID
            return global::Firebase.Auth.EmailAuthProvider.GetCredential(email, password);
#elif IOS
            return global::Firebase.Auth.EmailAuthProvider.GetCredentialFromPassword(email, password);
#else
            throw new PlatformNotSupportedException();
#endif
        }

    }
}