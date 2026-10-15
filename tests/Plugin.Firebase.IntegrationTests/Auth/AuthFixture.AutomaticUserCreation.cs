using Plugin.Firebase.Auth;
using Plugin.Firebase.Core.Exceptions;

namespace Plugin.Firebase.IntegrationTests.Auth
{
    public sealed partial class AuthFixture
    {
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


        // Emulator-only: the emulator enforces the 6-character default minimum, while a real
        // project's password policy may accept or reject other passwords.
        [EmulatorBackendFact]
        public async Task surfaces_creation_failure_for_new_account_with_weak_password()
        {
            // auto-creation for a genuinely new account must surface the actionable creation
            // error instead of the initial user-not-found sign-in error
            var sut = CrossFirebaseAuth.Current;
            var email = IntegrationTestData.UniqueEmail("weak-password");
            var ex = await Assert.ThrowsAnyAsync<CrossPlatformFirebaseAuthException>(
                () => sut.SignInWithEmailAndPasswordAsync(email, "1")
            );

            AuthAssertions.NativeAuthExceptionCaptured(ex);
#if ANDROID
            Assert.Equal("ERROR_WEAK_PASSWORD", ex.NativeErrorCode);
#elif IOS
            Assert.Equal("WeakPassword", ex.NativeErrorCode);
#endif
        }


        [EmulatorBackendFact]
        public async Task rethrows_sign_in_error_when_creation_reports_existing_account()
        {
            // With email enumeration protection, a wrong password for an existing account is reported
            // as an invalid credential, like a missing account, so the auto-create fallback runs and
            // collides with the account. The sign-in error must win over the collision. The emulator
            // has protection off by default, so the test turns it on and restores it afterwards. A
            // dedicated user keeps this independent of test order and shared accounts.
            var sut = CrossFirebaseAuth.Current;
            var email = IntegrationTestData.UniqueEmail("creation-collision");
            await using var createdUser =
                await AuthTestUserScope.CreateWithEmailAndPasswordAsync(sut, email);
            await sut.SignOutAsync();

            var improvedEmailPrivacy = await GetAuthEmulatorImprovedEmailPrivacyAsync();
            CrossPlatformFirebaseAuthException ex;
            try {
                await SetAuthEmulatorImprovedEmailPrivacyAsync(true);
                ex = await Assert.ThrowsAnyAsync<CrossPlatformFirebaseAuthException>(
                    () => sut.SignInWithEmailAndPasswordAsync(email, "000000")
                );
            }
            finally {
                await SetAuthEmulatorImprovedEmailPrivacyAsync(improvedEmailPrivacy);
            }

            AuthAssertions.NativeAuthExceptionCaptured(ex);
#if ANDROID
            Assert.Equal("ERROR_INVALID_CREDENTIAL", ex.NativeErrorCode);
            Assert.Equal(
                FirebaseAuthAndroidExceptionTypeNames.InvalidCredentialsException,
                ex.NativeExceptionTypeName);
#elif IOS
            Assert.Equal("InvalidCredential", ex.NativeErrorCode);
#endif
        }
    }
}