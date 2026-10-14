using System.Text.Json;
using Plugin.Firebase.Auth;
using Plugin.Firebase.Core.Exceptions;
using Plugin.Firebase.Functions;
using Plugin.Firebase.IntegrationTests.Functions;

namespace Plugin.Firebase.IntegrationTests.Auth
{
    public sealed partial class AuthFixture
    {
        [EmulatorBackendFact]
        public async Task signs_in_user_via_email_link_on_auth_emulator()
        {
            var sut = CrossFirebaseAuth.Current;
            var email = IntegrationTestData.UniqueEmail("email-link");
            var actionCodeSettings = CreateActionCodeSettings();

            await sut.SendSignInLink(email, actionCodeSettings);
            var emailLink = await GetLatestAuthEmulatorEmailLinkAsync(email, "EMAIL_SIGNIN");

            Assert.True(sut.IsSignInWithEmailLink(emailLink));

            var user = await sut.SignInWithEmailLinkAsync(email, emailLink);
            await using var testUser = AuthTestUserScope.TrackCurrentUser(sut);

            Assert.Equal(email, user.Email);
            Assert.Equal(email, sut.CurrentUser!.Email);
        }


        [OptInFact(
            IntegrationTestOptions.RunPhoneAuthTestsEnvironmentVariableName,
            IntegrationTestOptions.RunPhoneAuthTestsAndroidSystemPropertyName)]
        public async Task signs_in_user_via_phone_number_when_enabled()
        {
            var sut = CrossFirebaseAuth.Current;
            var phoneNumber = IntegrationTestData.GetRequiredConfigurationValue(
                IntegrationTestOptions.PhoneAuthNumberEnvironmentVariableName,
                IntegrationTestOptions.PhoneAuthNumberAndroidSystemPropertyName);
            var verificationCode = IntegrationTestData.GetRequiredConfigurationValue(
                IntegrationTestOptions.PhoneAuthCodeEnvironmentVariableName,
                IntegrationTestOptions.PhoneAuthCodeAndroidSystemPropertyName);

            await sut.SignOutAsync();
            await sut.VerifyPhoneNumberAsync(phoneNumber).WaitForTestAsync(
                IntegrationTestTimeouts.PhoneVerification,
                "phone number verification before sign-in");

            var user = await sut.SignInWithPhoneNumberVerificationCodeAsync(verificationCode);
            await using var testUser = AuthTestUserScope.TrackCurrentUser(sut, deleteOnDispose: false);

            Assert.NotNull(user);
            Assert.Equal(user.Uid, sut.CurrentUser!.Uid);
        }


        [OptInFact(
            IntegrationTestOptions.RunPhoneAuthTestsEnvironmentVariableName,
            IntegrationTestOptions.RunPhoneAuthTestsAndroidSystemPropertyName)]
        public async Task verify_phone_number_faults_for_invalid_number_when_enabled()
        {
            // OnVerificationFailed must fault the returned task, not leave it completed
            var sut = CrossFirebaseAuth.Current;
            var ex = await Assert.ThrowsAnyAsync<CrossPlatformFirebaseAuthException>(
                () => sut.VerifyPhoneNumberAsync("not-a-phone-number").WaitForTestAsync(
                    IntegrationTestTimeouts.PhoneVerification,
                    "invalid phone number verification failure")
            );

            AuthAssertions.NativeAuthExceptionCaptured(ex);
        }


        [OptInFact(
            IntegrationTestOptions.RunPhoneAuthTestsEnvironmentVariableName,
            IntegrationTestOptions.RunPhoneAuthTestsAndroidSystemPropertyName)]
        public async Task concurrent_phone_number_verifications_for_same_number_complete_when_enabled()
        {
            // Android reuses the in-flight native session for the same number and drops the second request's callbacks
            var sut = CrossFirebaseAuth.Current;
            var phoneNumber = IntegrationTestData.GetRequiredConfigurationValue(
                IntegrationTestOptions.PhoneAuthNumberEnvironmentVariableName,
                IntegrationTestOptions.PhoneAuthNumberAndroidSystemPropertyName);

            var first = sut.VerifyPhoneNumberAsync(phoneNumber);
            var second = sut.VerifyPhoneNumberAsync(phoneNumber);

            await Task.WhenAll(first, second).WaitForTestAsync(
                IntegrationTestTimeouts.PhoneVerification,
                "concurrent phone number verifications for the same number");
        }


        [OptInFact(
            IntegrationTestOptions.RunPhoneAuthTestsEnvironmentVariableName,
            IntegrationTestOptions.RunPhoneAuthTestsAndroidSystemPropertyName)]
        public async Task links_signed_in_user_with_phone_number_when_enabled()
        {
            var sut = CrossFirebaseAuth.Current;
            var phoneNumber = IntegrationTestData.GetRequiredConfigurationValue(
                IntegrationTestOptions.PhoneAuthNumberEnvironmentVariableName,
                IntegrationTestOptions.PhoneAuthNumberAndroidSystemPropertyName);
            var verificationCode = IntegrationTestData.GetRequiredConfigurationValue(
                IntegrationTestOptions.PhoneAuthCodeEnvironmentVariableName,
                IntegrationTestOptions.PhoneAuthCodeAndroidSystemPropertyName);

            await using var user = await AuthTestUserScope.SignInAnonymouslyAsync(sut);
            await sut.VerifyPhoneNumberAsync(phoneNumber).WaitForTestAsync(
                IntegrationTestTimeouts.PhoneVerification,
                "phone number verification before linking");

            var linkedUser = await sut.LinkWithPhoneNumberVerificationCodeAsync(verificationCode);

            Assert.Equal(sut.CurrentUser!.Uid, linkedUser.Uid);
            Assert.Contains(sut.CurrentUser!.ProviderInfos ?? Array.Empty<ProviderInfo>(), x => x.ProviderId == "phone");
        }


        [OptInFact(
            IntegrationTestOptions.RunPhoneAuthTestsEnvironmentVariableName,
            IntegrationTestOptions.RunPhoneAuthTestsAndroidSystemPropertyName)]
        public async Task updates_user_phone_number_when_enabled()
        {
            var sut = CrossFirebaseAuth.Current;
            var verificationId = IntegrationTestData.GetRequiredConfigurationValue(
                IntegrationTestOptions.PhoneAuthVerificationIdEnvironmentVariableName,
                IntegrationTestOptions.PhoneAuthVerificationIdAndroidSystemPropertyName);
            var verificationCode = IntegrationTestData.GetRequiredConfigurationValue(
                IntegrationTestOptions.PhoneAuthCodeEnvironmentVariableName,
                IntegrationTestOptions.PhoneAuthCodeAndroidSystemPropertyName);

            await using var user = await AuthTestUserScope.SignInWithUniqueEmailAndPasswordAsync(
                sut,
                "update-phone");
            Assert.NotNull(sut.CurrentUser);
            await sut.CurrentUser!.UpdatePhoneNumberAsync(verificationId, verificationCode);

            Assert.Contains(sut.CurrentUser!.ProviderInfos ?? Array.Empty<ProviderInfo>(), x => x.ProviderId == "phone");
        }


        [Fact]
        public async Task sends_verification_email()
        {
            var sut = CrossFirebaseAuth.Current;
            await using var user = await AuthTestUserScope.SignInWithEmailAndPasswordAsync(
                sut,
                IntegrationTestUsers.VerificationEmail,
                deleteOnDispose: false);
            Assert.NotNull(sut.CurrentUser);

            await sut.CurrentUser!.SendEmailVerificationAsync();
        }


        [EmulatorBackendFact]
        public async Task sends_verification_email_with_action_code_settings()
        {
            var sut = CrossFirebaseAuth.Current;
            await using var user = await AuthTestUserScope.SignInWithUniqueEmailAndPasswordAsync(
                sut,
                "verification-settings");
            Assert.NotNull(sut.CurrentUser);

            await sut.CurrentUser!.SendEmailVerificationAsync(CreateActionCodeSettings());
        }


        [Fact]
        public async Task sends_password_reset_email_for_current_user()
        {
            var sut = CrossFirebaseAuth.Current;
            await using var user = await AuthTestUserScope.SignInWithUniqueEmailAndPasswordAsync(
                sut,
                "pw-reset-current");

            await sut.SendPasswordResetEmailAsync();
        }


        [Fact]
        public async Task sends_password_reset_email_for_explicit_email()
        {
            var sut = CrossFirebaseAuth.Current;
            await using var user = await AuthTestUserScope.SignInWithUniqueEmailAndPasswordAsync(
                sut,
                "pw-reset-explicit");
            var email = user.Email ?? throw new InvalidOperationException("Expected scoped user email.");

            await sut.SendPasswordResetEmailAsync(email);
        }

    }
}