using NativeActionCodeSettings = Firebase.Auth.ActionCodeSettings;

namespace Plugin.Firebase.Auth
{
    public static class ActionCodeSettingsExtensions
    {
        public static NativeActionCodeSettings ToNative(this ActionCodeSettings @this)
        {
            return NativeActionCodeSettings
                .NewBuilder()
                .SetUrl(@this.Url)
                .SetHandleCodeInApp(@this.HandleCodeInApp)
                .SetIOSBundleId(@this.IOSBundleId)
                .SetAndroidPackageName(@this.AndroidPackageName, @this.AndroidInstallIfNotAvailable, @this.AndroidMinimumVersion)
                .Build();
        }
    }
}