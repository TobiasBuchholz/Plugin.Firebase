using Firebase.DynamicLinks;
using Foundation;
using Plugin.Firebase.DynamicLinks.Parameters;

namespace Plugin.Firebase.iOS.DynamicLinks
{
    public static class DynamicLinksExtensions
    {
        public static DynamicLinkSocialMetaTagParameters ToNative(this SocialMetaTagParameters @this)
        {
            var parameters = new DynamicLinkSocialMetaTagParameters();
            parameters.Title = @this.Title;
            parameters.DescriptionText = @this.DescriptionText;
            parameters.ImageUrl = @this.ImageUrl == null ? null : NSUrl.FromString(@this.ImageUrl);
            return parameters;
        }

        public static DynamicLinkiOSParameters ToNative(this iOSParameters @this)
        {
            var parameters = new DynamicLinkiOSParameters(@this.BundleId);
            parameters.AppStoreId = @this.AppStoreId;
            parameters.FallbackUrl = @this.FallbackUrl == null ? null : NSUrl.FromString(@this.FallbackUrl);
            parameters.CustomScheme = @this.CustomScheme;
            parameters.IPadFallbackUrl = @this.iPadFallbackUrl == null ? null : NSUrl.FromString(@this.iPadFallbackUrl);
            parameters.IPadBundleId = @this.iPadBundleId;
            parameters.MinimumAppVersion = @this.MinimumAppVersion;
            return parameters;
        }

        public static DynamicLinkAndroidParameters ToNative(this AndroidParameters @this)
        {
            var parameters = new DynamicLinkAndroidParameters(@this.PackageName);
            parameters.FallbackUrl = @this.FallbackUrl == null ? null : NSUrl.FromString(@this.FallbackUrl);
            parameters.MinimumVersion = @this.MinimumVersion;
            return parameters;
        }

        public static DynamicLinkGoogleAnalyticsParameters ToNative(this GoogleAnalyticsParameters @this)
        {
            var parameters = new DynamicLinkGoogleAnalyticsParameters();
            parameters.Source = @this.Source;
            parameters.Medium = @this.Medium;
            parameters.Campaign = @this.Campaign;
            parameters.Term = @this.Term;
            parameters.Content = @this.Content;
            return parameters;
        }

        public static DynamicLinkNavigationInfoParameters ToNative(this NavigationInfoParameters @this)
        {
            var parameters = new DynamicLinkNavigationInfoParameters();
            parameters.ForcedRedirectEnabled = @this.ForcedRedirectEnabled;
            return parameters;
        }

        public static DynamicLinkiTunesConnectAnalyticsParameters ToNative(this iTunesConnectAnalyticsParameters @this)
        {
            var parameters = new DynamicLinkiTunesConnectAnalyticsParameters();
            parameters.ProviderToken = @this.ProviderToken;
            parameters.AffiliateToken = @this.AffiliateToken;
            parameters.CampaignToken = @this.CampaignToken;
            return parameters;
        }
    }
}