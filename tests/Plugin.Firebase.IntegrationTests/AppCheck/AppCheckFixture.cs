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
        public void throws_when_configured_after_firebase_initialization()
        {
            // The native SDK reads the provider factory only when the default app is configured. The harness never
            // configures DeviceCheck or AppAttest before CrossFirebase.Initialize(), so both would change the provider.
            var exception = Assert.Throws<InvalidOperationException>(
                () => CrossFirebaseAppCheck.Configure(AppCheckOptions.DeviceCheck));
            Assert.Throws<InvalidOperationException>(() => CrossFirebaseAppCheck.Configure(AppCheckOptions.AppAttest));

            // Names the provider the plugin recorded when it configured the default app.
            Assert.Contains(
                $"from the '{FirebaseTestHost.LaunchAppCheckOptions.Provider}' provider",
                exception.Message);
        }

        [IosFact]
        public void accepts_the_provider_in_effect_after_firebase_initialization_on_ios()
        {
            var provider = FirebaseTestHost.LaunchAppCheckOptions.Provider;

            // Only a provider the plugin recorded as installed is a no-op; an unrecorded one throws for every provider.
            CrossFirebaseAppCheck.Configure(new AppCheckOptions(provider));

            // A new instance still sees the provider installed before initialization.
            CrossFirebaseAppCheck.Dispose();
            CrossFirebaseAppCheck.Configure(new AppCheckOptions(provider));
        }

        [IosFact]
        public void rejects_unsupported_providers_on_ios()
        {
            Assert.Throws<NotSupportedException>(() => CrossFirebaseAppCheck.Configure(AppCheckOptions.PlayIntegrity));
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

        [AndroidFact]
        public void rejects_unsupported_providers_on_android()
        {
            Assert.Throws<NotSupportedException>(() => CrossFirebaseAppCheck.Configure(AppCheckOptions.DeviceCheck));
            Assert.Throws<NotSupportedException>(() => CrossFirebaseAppCheck.Configure(AppCheckOptions.AppAttest));
        }

        // The emulator harness configures Disabled before CrossFirebase.Initialize(), so iOS runs without a provider
        // factory. iOS only: on Android, other tests in the run install a provider factory that can't be removed.
        [EmulatorBackendIosFact]
        public async Task disabled_app_check_does_not_break_auth_or_functions_on_emulator()
        {
            var auth = CrossFirebaseAuth.Current;
            var email = IntegrationTestData.UniqueEmail("app-check-disabled");

            await using var user = await AuthTestUserScope.SignInWithEmailAndPasswordAsync(auth, email);

            var response = await CrossFirebaseFunctions.Current
                .GetHttpsCallable("convertToLeet")
                .CallAsync<SimpleResponseData>("{\"input_value\":777}");

            Assert.Equal(777, response.InputValue);
            Assert.Equal(1337, response.OutputValue);
        }

        [RealFirebaseOptInFact(IntegrationTestEnvironment.RunAppCheckTokenTestsEnvironmentVariableName)]
        public async Task fetches_cached_and_forced_debug_tokens_when_enabled()
        {
            // With token tests enabled, the harness configures the Debug provider before CrossFirebase.Initialize().
            var cachedToken = await CrossFirebaseAppCheck.GetTokenAsync();
            var forcedToken = await CrossFirebaseAppCheck.GetTokenAsync(forceRefresh: true);

            Assert.False(string.IsNullOrWhiteSpace(cachedToken));
            Assert.False(string.IsNullOrWhiteSpace(forcedToken));
        }
    }
}