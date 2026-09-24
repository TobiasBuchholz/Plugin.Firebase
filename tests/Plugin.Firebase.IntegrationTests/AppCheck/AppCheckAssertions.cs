#if ANDROID
using Firebase.AppCheck.Debug;
using Firebase.AppCheck.Internal;
using Firebase.AppCheck.PlayIntegrity;
#endif
using Plugin.Firebase.AppCheck;

namespace Plugin.Firebase.IntegrationTests.AppCheck
{
    internal static class AppCheckAssertions
    {
        // Reads the factory held by the native SDK, so provider changes are checked against native
        // state rather than the options the plugin last accepted.
        public static void InstalledAndroidProviderFactoryIs(AppCheckProviderType provider)
        {
#if ANDROID
            Java.Lang.Object expected = provider switch {
                AppCheckProviderType.Debug => DebugAppCheckProviderFactory.Instance,
                AppCheckProviderType.PlayIntegrity => PlayIntegrityAppCheckProviderFactory.Instance,
                _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, "Android has no factory for this provider.")
            };
            var appCheck = Assert.IsAssignableFrom<DefaultFirebaseAppCheck>(
                global::Firebase.AppCheck.FirebaseAppCheck.GetInstance(global::Firebase.FirebaseApp.Instance));
            var installed = Assert.IsAssignableFrom<Java.Lang.Object>(appCheck.InstalledAppCheckProviderFactory);

            Assert.Equal(expected.Class.Name, installed.Class.Name);
#else
            throw new PlatformNotSupportedException("Installed App Check provider factories are only inspectable on Android.");
#endif
        }
    }
}