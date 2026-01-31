using Firebase.Auth;
using Plugin.Firebase.Core.Platforms.iOS.Extensions;
using NativeActionCodeSettings = Firebase.Auth.ActionCodeSettings;
using NativeUserMetadata = Firebase.Auth.UserMetadata;

namespace Plugin.Firebase.Auth.Platforms.iOS.Extensions;

/// <summary>
/// Provides extension methods for converting between native iOS Firebase Auth types and cross-platform types.
/// </summary>
public static class FirebaseAuthExtensions
{
    /// <summary>
    /// Converts a native iOS Firebase User to a cross-platform FirebaseUserWrapper.
    /// </summary>
    /// <param name="this">The native iOS Firebase User to convert.</param>
    /// <param name="additionalUserInfo">Optional additional user info from the authentication result.</param>
    /// <returns>A cross-platform FirebaseUserWrapper instance.</returns>
    public static FirebaseUserWrapper ToAbstract(
        this User @this,
        AdditionalUserInfo additionalUserInfo = null
    )
    {
        return new FirebaseUserWrapper(@this);
    }

    /// <summary>
    /// Converts a native iOS Firebase IUserInfo to a cross-platform ProviderInfo.
    /// </summary>
    /// <param name="this">The native iOS Firebase user info to convert.</param>
    /// <param name="additionalUserInfo">Optional additional user info that may contain the email.</param>
    /// <returns>A cross-platform ProviderInfo instance.</returns>
    public static ProviderInfo ToAbstract(
        this IUserInfo @this,
        AdditionalUserInfo additionalUserInfo = null
    )
    {
        return new ProviderInfo(
            @this.Uid,
            @this.ProviderId,
            @this.DisplayName,
            @this.Email ?? GetEmailFromAdditionalUserInfo(additionalUserInfo),
            @this.PhoneNumber,
            @this.PhotoUrl?.AbsoluteString
        );
    }

    private static string GetEmailFromAdditionalUserInfo(AdditionalUserInfo additionalUserInfo)
    {
        var profile = additionalUserInfo?.Profile;
        if(profile != null && profile.ContainsKey(new NSString("email"))) {
            return profile["email"].ToString();
        }
        return null;
    }

    /// <summary>
    /// Converts cross-platform ActionCodeSettings to native iOS ActionCodeSettings.
    /// </summary>
    /// <param name="this">The cross-platform ActionCodeSettings to convert.</param>
    /// <returns>A native iOS ActionCodeSettings instance.</returns>
    public static NativeActionCodeSettings ToNative(this ActionCodeSettings @this)
    {
        var settings = new NativeActionCodeSettings();
        if(@this.Url is not null) {
            settings.Url = new NSUrl(@this.Url);
        }
        settings.HandleCodeInApp = @this.HandleCodeInApp;
        settings.IOSBundleId = @this.IOSBundleId;
        if(@this.AndroidPackageName is not null) {
            settings.SetAndroidPackageName(
                @this.AndroidPackageName,
                @this.AndroidInstallIfNotAvailable,
                @this.AndroidMinimumVersion
            );
        }
        return settings;
    }

    /// <summary>
    /// Converts native iOS Firebase UserMetadata to cross-platform UserMetadata.
    /// </summary>
    /// <param name="this">The native iOS Firebase user metadata to convert.</param>
    /// <returns>A cross-platform UserMetadata instance.</returns>
    public static UserMetadata ToAbstract(this NativeUserMetadata @this)
    {
        return new UserMetadata(
            @this.CreationDate.ToDateTimeOffset(),
            @this.LastSignInDate.ToDateTimeOffset()
        );
    }

    /// <summary>
    /// Converts native iOS Firebase AuthTokenResult to a cross-platform IAuthTokenResult.
    /// </summary>
    /// <param name="this">The native iOS Firebase auth token result to convert.</param>
    /// <returns>A cross-platform IAuthTokenResult wrapper.</returns>
    public static IAuthTokenResult ToAbstract(this AuthTokenResult @this)
    {
        return new AuthTokenResultWrapper(@this);
    }
}