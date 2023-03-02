using Firebase.Auth;
using Foundation;
using Plugin.Firebase.Core.Platforms.iOS.Extensions;
using NativeActionCodeSettings = Firebase.Auth.ActionCodeSettings;
using NativeUserMetadata = Firebase.Auth.UserMetadata;

namespace Plugin.Firebase.Auth.Platforms.iOS.Extensions;

public static class FirebaseAuthExtensions
{
    public static FirebaseUserWrapper ToAbstract(this User @this, AdditionalUserInfo additionalUserInfo = null)
    {
        return new FirebaseUserWrapper(@this);
    }

    public static ProviderInfo ToAbstract(this IUserInfo @this, AdditionalUserInfo additionalUserInfo = null)
    {
        return new ProviderInfo
            (@this.Uid,
            @this.ProviderId,
            @this.DisplayName,
            @this.Email ?? GetEmailFromAdditionalUserInfo(additionalUserInfo),
            @this.PhoneNumber,
            @this.PhotoUrl?.AbsoluteString);
    }

    private static string GetEmailFromAdditionalUserInfo(AdditionalUserInfo additionalUserInfo)
    {
        var profile = additionalUserInfo?.Profile;
        if(profile != null && profile.ContainsKey(new NSString("email"))) {
            return profile["email"].ToString();
        }
        return null;
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

    public static UserMetadata ToAbstract(this NativeUserMetadata @this)
    {
        return new UserMetadata(
            @this.CreationDate.ToDateTimeOffset(),
            @this.LastSignInDate.ToDateTimeOffset());
    }

    public static IAuthTokenResult ToAbstract(this AuthTokenResult @this)
    {
        return new AuthTokenResultWrapper(@this);
    }
}