using System;
using Plugin.Firebase.Analytics;
using Plugin.Firebase.Auth;
using Plugin.Firebase.CloudMessaging;
using Plugin.Firebase.DynamicLinks;
using Plugin.Firebase.Firestore;
using Plugin.Firebase.Functions;
using Plugin.Firebase.RemoteConfig;
using Plugin.Firebase.Storage;
using Xunit;

namespace Plugin.Firebase.IntegrationTests
{
    public class MustPassFixture
    {
        [Fact]
        public void must_pass()
        {
            // Assert.True(CrossFirebaseAnalytics.IsSupported); -> disabled on Android because of build issues
            Assert.True(CrossFirebaseAuth.IsSupported);
            Assert.True(CrossFirebaseCloudMessaging.IsSupported);
            Assert.True(CrossFirebaseDynamicLinks.IsSupported);
            Assert.True(CrossFirebaseFirestore.IsSupported);
            Assert.True(CrossFirebaseFunctions.IsSupported);
            Assert.True(CrossFirebaseStorage.IsSupported);
            Assert.True(CrossFirebaseRemoteConfig.IsSupported);
        }
    }
}
