using Firebase.Auth;
using Foundation;
using Plugin.Firebase.Auth;
using ActionCodeSettings = Plugin.Firebase.Auth.ActionCodeSettings;
using NativeActionCodeSettings = Firebase.Auth.ActionCodeSettings;

namespace Plugin.Firebase.iOS.Auth
{
    public static class FirebaseAuthExtensions
    {
        public static FirebaseUserWrapper ToAbstract(this User @this)
        {
            return new FirebaseUserWrapper(@this);
        }
        
        public static ProviderInfo ToAbstract(this IUserInfo @this)
        {
            return new ProviderInfo
                (@this.Uid,
                @this.ProviderId,
                @this.DisplayName,
                @this.Email,
                @this.PhoneNumber,
                @this.PhotoUrl?.AbsoluteString);
        }
        
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