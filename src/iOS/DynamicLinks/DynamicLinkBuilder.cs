using System;
using System.Threading.Tasks;
using Firebase.DynamicLinks;
using Foundation;
using Plugin.Firebase.DynamicLinks;
using Plugin.Firebase.DynamicLinks.Parameters;

namespace Plugin.Firebase.iOS.DynamicLinks
{
    public sealed class DynamicLinkBuilder : IDynamicLinkBuilder
    {
        private string _link;
        private string _domainUriPrefix;
        private AndroidParameters _androidParameters;
        private iOSParameters _iOSParameters;
        private SocialMetaTagParameters _socialMetaTagParameters;
        private GoogleAnalyticsParameters _googleAnalyticsParameters;
        private NavigationInfoParameters _navigationInfoParameters;
        private iTunesConnectAnalyticsParameters _itunesConnectAnalyticsParameters;

        public IDynamicLinkBuilder SetLink(string link)
        {
            _link = link;
            return this;
        }

        public IDynamicLinkBuilder SetLink(Uri link)
        {
            _link = link.AbsoluteUri;
            return this;
        }

        public IDynamicLinkBuilder SetDomainUriPrefix(string prefix)
        {
            _domainUriPrefix = prefix;
            return this;
        }

        public IDynamicLinkBuilder SetAndroidParameters(AndroidParameters parameters)
        {
            _androidParameters = parameters;
            return this;
        }

        public IDynamicLinkBuilder SetiOSParameters(iOSParameters parameters)
        {
            _iOSParameters = parameters;
            return this;
        }

        public IDynamicLinkBuilder SetSocialMetaTagParameters(SocialMetaTagParameters parameters)
        {
            _socialMetaTagParameters = parameters;
            return this;
        }

        public IDynamicLinkBuilder SetGoogleAnalyticsParameters(GoogleAnalyticsParameters parameters)
        {
            _googleAnalyticsParameters = parameters;
            return this;
        }

        public IDynamicLinkBuilder SetNavigationInfoParameters(NavigationInfoParameters parameters)
        {
            _navigationInfoParameters = parameters;
            return this;
        }

        public IDynamicLinkBuilder SetiTunesConnectAnalyticsParameters(iTunesConnectAnalyticsParameters parameters)
        {
            _itunesConnectAnalyticsParameters = parameters;
            return this;
        }

        public Uri BuildDynamicLink()
        {
            return CreateDynamicLinkComponents().Url;
        }

        private DynamicLinkComponents CreateDynamicLinkComponents()
        {
            var link = NSUrl.FromString(_link);
            var components = new DynamicLinkComponents(link, _domainUriPrefix);
            components.AndroidParameters = _androidParameters?.ToNative();
            components.IOSParameters = _iOSParameters?.ToNative();
            components.SocialMetaTagParameters = _socialMetaTagParameters?.ToNative();
            components.AnalyticsParameters = _googleAnalyticsParameters?.ToNative();
            components.NavigationInfoParameters = _navigationInfoParameters?.ToNative();
            components.ITunesConnectParameters = _itunesConnectAnalyticsParameters?.ToNative();
            return components;
        }

        public async Task<ShortDynamicLink> BuildShortDynamicLinkAsync()
        {
            var result = await CreateDynamicLinkComponents().GetShortenUrlAsync();
            return new ShortDynamicLink(new Uri(result.ShortUrl.AbsoluteString), result.Warnings);
        }
    }
}