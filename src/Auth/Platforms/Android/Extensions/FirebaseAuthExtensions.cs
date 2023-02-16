using Firebase.Auth;
using Plugin.Firebase.Android.Auth;
using NativeActionCodeSettings = Firebase.Auth.ActionCodeSettings;

namespace Plugin.Firebase.Auth.Android.Extensions;

public static class FirebaseAuthExtensions
{
    public static FirebaseUserWrapper ToAbstract(this FirebaseUser @this, IAdditionalUserInfo additionalUserInfo = null)
    {
        return new FirebaseUserWrapper(@this);
    }

    public static ProviderInfo ToAbstract(this IUserInfo @this, IAdditionalUserInfo additionalUserInfo = null)
    {
        return new ProviderInfo(
            @this.Uid,
            @this.ProviderId,
            @this.DisplayName,
            @this.Email ?? GetEmailFromAdditionalUserInfo(additionalUserInfo),
            @this.PhoneNumber,
            @this.PhotoUrl?.ToString());
    }

    private static string GetEmailFromAdditionalUserInfo(IAdditionalUserInfo additionalUserInfo)
    {
        var profile = additionalUserInfo?.Profile;
        if(profile != null && profile.ContainsKey("email")) {
            return profile["email"].ToString();
        }
        return null;
    }

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

    public static UserMetadata ToAbstract(this IFirebaseUserMetadata @this)
    {
        return new UserMetadata(
            DateTimeOffset.FromUnixTimeMilliseconds(@this.CreationTimestamp),
            DateTimeOffset.FromUnixTimeMilliseconds(@this.LastSignInTimestamp));
    }

    public static IAuthTokenResult ToAbstract(this GetTokenResult @this)
    {
        return new AuthTokenResultWrapper(@this);
    }
}