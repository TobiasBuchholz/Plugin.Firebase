using Foundation;
using NativeActionCodeSettings = Firebase.Auth.ActionCodeSettings;

namespace Plugin.Firebase.Auth
{
    public static class ActionCodeSettingsExtensions
    {
        public static NativeActionCodeSettings ToNative(this ActionCodeSettings @this)
        {
            var settings = new NativeActionCodeSettings();
            settings.Url = new NSUrl(@this.Url);
            settings.HandleCodeInApp = @this.HandleCodeInApp;
            settings.IOSBundleId = @this.IOSBundleId;
            settings.SetAndroidPackageName(@this.AndroidPackageName, @this.AndroidInstallIfNotAvailable, @this.AndroidMinimumVersion);
            return settings;
        }
    }
}