using Plugin.Firebase.AppCheck;
using Plugin.Firebase.Auth;
using Plugin.Firebase.Functions;
using Plugin.Firebase.IntegrationTests.Functions;

namespace Plugin.Firebase.IntegrationTests.AppCheck
{
    [Collection("Sequential")]
    [TestLogging]
    [IntegrationTestFixture(IntegrationTestPackage.AppCheck)]
    [Preserve(AllMembers = true)]
    public sealed class AppCheckFixture
    {
        [Fact]
        public void rejects_null_options()
        {
            Assert.Throws<ArgumentNullException>(() => CrossFirebaseAppCheck.Configure(null!));
        }

        [IosFact]
        public void transitions_between_disabled_and_debug_providers()
        {
            try {
                CrossFirebaseAppCheck.Configure(AppCheckOptions.Disabled);
                CrossFirebaseAppCheck.Configure(AppCheckOptions.Debug);
                CrossFirebaseAppCheck.Configure(AppCheckOptions.Disabled);
            }
            finally {
                CrossFirebaseAppCheck.Configure(AppCheckOptions.Disabled);
            }
        }

        [AndroidFact]
        public void installs_each_provider_change_after_initialization_on_android()
        {
            try {
                CrossFirebaseAppCheck.Configure(AppCheckOptions.Debug);
                AppCheckAssertions.InstalledAndroidProviderFactoryIs(AppCheckProviderType.Debug);

                CrossFirebaseAppCheck.Configure(AppCheckOptions.PlayIntegrity);
                AppCheckAssertions.InstalledAndroidProviderFactoryIs(AppCheckProviderType.PlayIntegrity);
            }
            finally {
                // Android cannot remove a provider factory, so leave Debug installed for the rest of the run.
                CrossFirebaseAppCheck.Configure(AppCheckOptions.Debug);
            }

            AppCheckAssertions.InstalledAndroidProviderFactoryIs(AppCheckProviderType.Debug);
        }

        [AndroidFact]
        public void rejects_disabling_after_a_provider_factory_is_installed_on_android()
        {
            CrossFirebaseAppCheck.Configure(AppCheckOptions.Debug);

            Assert.Throws<InvalidOperationException>(() => CrossFirebaseAppCheck.Configure(AppCheckOptions.Disabled));
            AppCheckAssertions.InstalledAndroidProviderFactoryIs(AppCheckProviderType.Debug);
        }

        [AndroidFact]
        public void keeps_the_installed_provider_after_dispose_on_android()
        {
            CrossFirebaseAppCheck.Configure(AppCheckOptions.Debug);

            CrossFirebaseAppCheck.Dispose();

            AppCheckAssertions.InstalledAndroidProviderFactoryIs(AppCheckProviderType.Debug);
            Assert.Throws<InvalidOperationException>(() => CrossFirebaseAppCheck.Configure(AppCheckOptions.Disabled));
        }

        [Fact]
        public void covers_platform_specific_unsupported_provider_behavior()
        {
            if(OperatingSystem.IsAndroid()) {
                Assert.Throws<NotSupportedException>(() => CrossFirebaseAppCheck.Configure(AppCheckOptions.DeviceCheck));
                Assert.Throws<NotSupportedException>(() => CrossFirebaseAppCheck.Configure(AppCheckOptions.AppAttest));
            }

            if(OperatingSystem.IsIOS()) {
                try {
                    CrossFirebaseAppCheck.Configure(AppCheckOptions.PlayIntegrity);
                }
                finally {
                    CrossFirebaseAppCheck.Configure(AppCheckOptions.Disabled);
                }
            }
        }

        // iOS only: Android cannot return to Disabled once another test in the run has installed a provider.
        [EmulatorBackendIosFact]
        public async Task disabled_app_check_does_not_break_auth_or_functions_on_emulator()
        {
            var auth = CrossFirebaseAuth.Current;
            var email = IntegrationTestData.UniqueEmail("app-check-disabled");

            try {
                CrossFirebaseAppCheck.Configure(AppCheckOptions.Disabled);
                await using var user = await AuthTestUserScope.SignInWithEmailAndPasswordAsync(auth, email);

                var response = await CrossFirebaseFunctions.Current
                    .GetHttpsCallable("convertToLeet")
                    .CallAsync<SimpleResponseData>("{\"input_value\":777}");

                Assert.Equal(777, response.InputValue);
                Assert.Equal(1337, response.OutputValue);
            }
            finally {
                CrossFirebaseAppCheck.Configure(AppCheckOptions.Disabled);
            }
        }

        [RealFirebaseOptInFact(IntegrationTestEnvironment.RunAppCheckTokenTestsEnvironmentVariableName)]
        public async Task fetches_cached_and_forced_debug_tokens_when_enabled()
        {
            CrossFirebaseAppCheck.Configure(AppCheckOptions.Debug);

            var cachedToken = await CrossFirebaseAppCheck.GetTokenAsync();
            var forcedToken = await CrossFirebaseAppCheck.GetTokenAsync(forceRefresh: true);

            Assert.False(string.IsNullOrWhiteSpace(cachedToken));
            Assert.False(string.IsNullOrWhiteSpace(forcedToken));
        }
    }
}