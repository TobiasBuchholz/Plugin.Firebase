using Plugin.Firebase.Analytics;
using Plugin.Firebase.AppCheck;
using Plugin.Firebase.Auth;
using Plugin.Firebase.CloudMessaging;
using Plugin.Firebase.Crashlytics;
using Plugin.Firebase.Firestore;
using Plugin.Firebase.Functions;
using Plugin.Firebase.Installations;
using Plugin.Firebase.PerformanceMonitoring;
using Plugin.Firebase.RemoteConfig;
using Plugin.Firebase.Storage;

namespace Plugin.Firebase.IntegrationTests.Bundled
{
    [Collection("Sequential")]
    [TestLogging]
    [IntegrationTestFixture(IntegrationTestPackage.Bundled)]
    [Preserve(AllMembers = true)]
    public sealed class BundledInitializerFixture
    {
        [Fact]
        public void active_services_are_available_after_bundled_initialization()
        {
            Assert.NotNull(CrossFirebaseAnalytics.Current);
            Assert.NotNull(CrossFirebaseAppCheck.Current);
            Assert.NotNull(CrossFirebaseAuth.Current);
            Assert.NotNull(CrossFirebaseCloudMessaging.Current);
            Assert.NotNull(CrossFirebaseCrashlytics.Current);
            Assert.NotNull(CrossFirebaseFirestore.Current);
            Assert.NotNull(CrossFirebaseFunctions.Current);
            Assert.NotNull(CrossFirebaseInstallations.Current);
            Assert.NotNull(CrossFirebasePerformanceMonitoring.Current);
            Assert.NotNull(CrossFirebaseRemoteConfig.Current);
            Assert.NotNull(CrossFirebaseStorage.Current);
        }

        [Fact]
        public void active_services_dispose_and_reacquire()
        {
            AssertRecreates(() => CrossFirebaseAnalytics.Current, CrossFirebaseAnalytics.Dispose);
            AssertRecreates(() => CrossFirebaseAppCheck.Current, CrossFirebaseAppCheck.Dispose);
            AssertRecreates(() => CrossFirebaseAuth.Current, CrossFirebaseAuth.Dispose);
            AssertRecreates(() => CrossFirebaseCloudMessaging.Current, CrossFirebaseCloudMessaging.Dispose);
            AssertRecreates(() => CrossFirebaseCrashlytics.Current, CrossFirebaseCrashlytics.Dispose);
            AssertRecreates(() => CrossFirebaseFirestore.Current, CrossFirebaseFirestore.Dispose);
            AssertRecreates(() => CrossFirebaseFunctions.Current, CrossFirebaseFunctions.Dispose);
            AssertRecreates(() => CrossFirebaseInstallations.Current, CrossFirebaseInstallations.Dispose);
            AssertRecreates(() => CrossFirebasePerformanceMonitoring.Current, CrossFirebasePerformanceMonitoring.Dispose);
            AssertRecreates(() => CrossFirebaseRemoteConfig.Current, CrossFirebaseRemoteConfig.Dispose);
            AssertRecreates(() => CrossFirebaseStorage.Current, CrossFirebaseStorage.Dispose);
        }

        private static void AssertRecreates<T>(
            Func<T> getCurrent,
            Action dispose)
            where T : class
        {
            var first = getCurrent();

            dispose();

            var second = getCurrent();

            Assert.NotNull(second);
            Assert.NotSame(first, second);
        }
    }
}