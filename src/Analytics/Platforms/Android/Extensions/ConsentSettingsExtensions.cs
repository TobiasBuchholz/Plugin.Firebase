using NativeConsentStatus = Firebase.Analytics.FirebaseAnalytics.ConsentStatus;
using NativeConsentType = Firebase.Analytics.FirebaseAnalytics.ConsentType;

namespace Plugin.Firebase.Analytics.Platforms.Android.Extensions;

internal static class ConsentSettingsExtensions
{
    internal static IDictionary<NativeConsentType, NativeConsentStatus> ToNativeConsentSettings(
        this IDictionary<ConsentType, ConsentStatus> consentSettings
    )
    {
        ArgumentNullException.ThrowIfNull(consentSettings);

        return consentSettings.ToDictionary(
            x => x.Key.ToNative(),
            x => x.Value.ToNative()
        );
    }

    private static NativeConsentType ToNative(this ConsentType consentType)
    {
        return consentType switch {
            ConsentType.AdStorage => NativeConsentType.AdStorage,
            ConsentType.AnalyticsStorage => NativeConsentType.AnalyticsStorage,
            ConsentType.AdUserData => NativeConsentType.AdUserData,
            ConsentType.AdPersonalization => NativeConsentType.AdPersonalization,
            _ => throw new ArgumentOutOfRangeException(nameof(consentType), consentType, null)
        };
    }

    private static NativeConsentStatus ToNative(this ConsentStatus consentStatus)
    {
        return consentStatus switch {
            ConsentStatus.Granted => NativeConsentStatus.Granted,
            ConsentStatus.Denied => NativeConsentStatus.Denied,
            _ => throw new ArgumentOutOfRangeException(nameof(consentStatus), consentStatus, null)
        };
    }
}