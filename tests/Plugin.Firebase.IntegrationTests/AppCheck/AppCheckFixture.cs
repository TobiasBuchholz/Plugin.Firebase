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
            // On iOS the native provider factory can only be installed before
            // FirebaseApp.configure() runs; configuring afterwards cannot take effect
            // and must fail loudly instead of silently dropping the provider.
            Assert.Throws<InvalidOperationException>(() => CrossFirebaseAppCheck.Configure(AppCheckOptions.Debug));
            Assert.Throws<InvalidOperationException>(() => CrossFirebaseAppCheck.Configure(AppCheckOptions.Disabled));
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
            try {
                Assert.Throws<NotSupportedException>(() => CrossFirebaseAppCheck.Configure(AppCheckOptions.DeviceCheck));
                Assert.Throws<NotSupportedException>(() => CrossFirebaseAppCheck.Configure(AppCheckOptions.AppAttest));
            }
            finally {
                CrossFirebaseAppCheck.Configure(AppCheckOptions.Disabled);
            }
        }

        [EmulatorBackendIosFact]
        public async Task auth_and_functions_work_when_app_check_is_not_installed_on_ios()
        {
            // On iOS, Configure() after initialization is rejected, so App Check simply
            // remains uninstalled here; Auth and Functions must still work.
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
            // Post-initialization Configure() is only supported on Android; on iOS this
            // test relies on the provider already being installed at launch time.
            if(OperatingSystem.IsAndroid()) {
                CrossFirebaseAppCheck.Configure(AppCheckOptions.Debug);
            }

            var cachedToken = await CrossFirebaseAppCheck.GetTokenAsync();
            var forcedToken = await CrossFirebaseAppCheck.GetTokenAsync(forceRefresh: true);

            Assert.False(string.IsNullOrWhiteSpace(cachedToken));
            Assert.False(string.IsNullOrWhiteSpace(forcedToken));
        }
    }
}