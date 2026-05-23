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

        [Fact]
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

        [Fact]
        public void covers_platform_specific_unsupported_provider_behavior()
        {
            try {
                if(OperatingSystem.IsAndroid()) {
                    Assert.Throws<NotSupportedException>(() => CrossFirebaseAppCheck.Configure(AppCheckOptions.DeviceCheck));
                    Assert.Throws<NotSupportedException>(() => CrossFirebaseAppCheck.Configure(AppCheckOptions.AppAttest));
                }

                if(OperatingSystem.IsIOS()) {
                    CrossFirebaseAppCheck.Configure(AppCheckOptions.PlayIntegrity);
                }
            }
            finally {
                CrossFirebaseAppCheck.Configure(AppCheckOptions.Disabled);
            }
        }

        [EmulatorBackendFact]
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