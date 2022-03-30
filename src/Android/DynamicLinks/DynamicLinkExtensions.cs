using Android.Net;
using Firebase.DynamicLinks;
using Plugin.Firebase.DynamicLinks.Parameters;

namespace Plugin.Firebase.Android.Extensions
{
    public static class DynamicLinkExtensions
    {
        public static DynamicLink.SocialMetaTagParameters ToNative(this SocialMetaTagParameters @this)
        {
            return new DynamicLink.SocialMetaTagParameters.Builder()
                .SetTitle(@this.Title)
                .SetDescription(@this.DescriptionText)
                .SetImageUrl(@this.ImageUrl == null ? null : Uri.Parse(@this.ImageUrl))
                .Build();
        }

        public static DynamicLink.IosParameters ToNative(this iOSParameters @this)
        {
            return new DynamicLink.IosParameters.Builder(@this.BundleId)
                .SetAppStoreId(@this.AppStoreId)
                .SetFallbackUrl(@this.FallbackUrl == null ? null : Uri.Parse(@this.FallbackUrl))
                .SetCustomScheme(@this.CustomScheme)
                .SetIpadFallbackUrl(@this.iPadFallbackUrl == null ? null : Uri.Parse(@this.iPadFallbackUrl))
                .SetIpadBundleId(@this.iPadBundleId)
                .SetMinimumVersion(@this.MinimumAppVersion)
                .Build();
        }

        public static DynamicLink.AndroidParameters ToNative(this AndroidParameters @this)
        {
            return new DynamicLink.AndroidParameters.Builder(@this.PackageName)
                .SetFallbackUrl(@this.FallbackUrl == null ? null : Uri.Parse(@this.FallbackUrl))
                .SetMinimumVersion(@this.MinimumVersion)
                .Build();
        }

        public static DynamicLink.GoogleAnalyticsParameters ToNative(this GoogleAnalyticsParameters @this)
        {
            return new DynamicLink.GoogleAnalyticsParameters.Builder()
                .SetSource(@this.Source)
                .SetMedium(@this.Medium)
                .SetCampaign(@this.Campaign)
                .SetTerm(@this.Term)
                .SetContent(@this.Content)
                .Build();
        }

        public static DynamicLink.NavigationInfoParameters ToNative(this NavigationInfoParameters @this)
        {
            return new DynamicLink.NavigationInfoParameters.Builder()
                .SetForcedRedirectEnabled(@this.ForcedRedirectEnabled)
                .Build();
        }

        public static DynamicLink.ItunesConnectAnalyticsParameters ToNative(this iTunesConnectAnalyticsParameters @this)
        {
            return new DynamicLink.ItunesConnectAnalyticsParameters.Builder()
                .SetProviderToken(@this.ProviderToken)
                .SetAffiliateToken(@this.AffiliateToken)
                .SetCampaignToken(@this.CampaignToken)
                .Build();
        }
    }
}