using System;
using System.Threading.Tasks;
using Plugin.Firebase.DynamicLinks.Parameters;

namespace Plugin.Firebase.DynamicLinks
{
    public interface IDynamicLinkBuilder
    {
        IDynamicLinkBuilder SetLink(string link);
        IDynamicLinkBuilder SetLink(Uri link);
        IDynamicLinkBuilder SetDomainUriPrefix(string prefix);
        IDynamicLinkBuilder SetAndroidParameters(AndroidParameters parameters);
        IDynamicLinkBuilder SetiOSParameters(iOSParameters parameters);
        IDynamicLinkBuilder SetSocialMetaTagParameters(SocialMetaTagParameters parameters);
        IDynamicLinkBuilder SetGoogleAnalyticsParameters(GoogleAnalyticsParameters parameters);
        IDynamicLinkBuilder SetNavigationInfoParameters(NavigationInfoParameters parameters);
        IDynamicLinkBuilder SetiTunesConnectAnalyticsParameters(iTunesConnectAnalyticsParameters parameters);
        Uri BuildDynamicLink();
        Task<ShortDynamicLink> BuildShortDynamicLinkAsync();
    }
}