using Plugin.Firebase.Analytics;
using Plugin.Firebase.AppCheck;
using Plugin.Firebase.Auth;
using Plugin.Firebase.CloudMessaging;
using Plugin.Firebase.Firestore;
using Plugin.Firebase.Functions;
using Plugin.Firebase.RemoteConfig;
using Plugin.Firebase.Storage;

namespace Plugin.Firebase.IntegrationTests
{
    [Preserve(AllMembers = true)]
    public class MustPassFixture
    {
        [Fact]
        public void must_pass()
        {
            Assert.True(true);
            Assert.True(CrossFirebaseAnalytics.IsSupported);
            Assert.True(CrossFirebaseAuth.IsSupported);
            Assert.True(CrossFirebaseCloudMessaging.IsSupported);
            Assert.True(CrossFirebaseFirestore.IsSupported);
            Assert.True(CrossFirebaseFunctions.IsSupported);
            Assert.True(CrossFirebaseStorage.IsSupported);
            Assert.True(CrossFirebaseRemoteConfig.IsSupported);
            Assert.True(CrossFirebaseAppCheck.IsSupported);
            CrossFirebaseAppCheck.Configure(AppCheckOptions.Disabled);

            if(OperatingSystem.IsAndroid()) {
                Assert.Throws<NotSupportedException>(() => CrossFirebaseAppCheck.Configure(AppCheckOptions.DeviceCheck));
                Assert.Throws<NotSupportedException>(() => CrossFirebaseAppCheck.Configure(AppCheckOptions.AppAttest));
                CrossFirebaseAppCheck.Configure(AppCheckOptions.Debug);
                CrossFirebaseAppCheck.Configure(AppCheckOptions.PlayIntegrity);
            }

            if(OperatingSystem.IsIOS()) {
                CrossFirebaseAppCheck.Configure(AppCheckOptions.PlayIntegrity);
                CrossFirebaseAppCheck.Configure(AppCheckOptions.Debug);
                CrossFirebaseAppCheck.Configure(AppCheckOptions.DeviceCheck);
                CrossFirebaseAppCheck.Configure(AppCheckOptions.AppAttest);
            }
        }

        [Fact]
        public async Task fetches_app_check_token_when_enabled_via_environment()
        {
            var shouldRunTokenTest = Environment.GetEnvironmentVariable("PLUGIN_FIREBASE_RUN_APPCHECK_TOKEN_TESTS") == "1";
            if(!shouldRunTokenTest) {
                return;
            }

            CrossFirebaseAppCheck.Configure(AppCheckOptions.Debug);
            var token = await CrossFirebaseAppCheck.GetTokenAsync(forceRefresh: true);

            Assert.False(string.IsNullOrWhiteSpace(token));
        }
    }
}