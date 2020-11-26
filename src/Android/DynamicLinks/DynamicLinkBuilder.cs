using System;
using System.Linq;
using System.Threading.Tasks;
using Android.Gms.Extensions;
using Android.Runtime;
using Firebase.DynamicLinks;
using Plugin.Firebase.DynamicLinks;
using Plugin.Firebase.DynamicLinks.Parameters;
using Plugin.Firebase.Android.Extensions;
using AndroidUri = Android.Net.Uri;

namespace Plugin.Firebase.Android.DynamicLinks
{
    public sealed class DynamicLinkBuilder : IDynamicLinkBuilder
    {
        private readonly DynamicLink.Builder _builder;

        public DynamicLinkBuilder()
        {
            _builder = FirebaseDynamicLinks.Instance.CreateDynamicLink();
        }

        public IDynamicLinkBuilder SetLink(string link)
        {
            _builder.SetLink(AndroidUri.Parse(link));
            return this;
        }

        public IDynamicLinkBuilder SetLink(Uri link)
        {
            _builder.SetLink(AndroidUri.Parse(link.AbsoluteUri));
            return this;
        }

        public IDynamicLinkBuilder SetDomainUriPrefix(string prefix)
        {
            _builder.SetDomainUriPrefix(prefix);
            return this;
        }

        public IDynamicLinkBuilder SetAndroidParameters(AndroidParameters parameters)
        {
            _builder.SetAndroidParameters(parameters.ToNative());
            return this;
        }

        public IDynamicLinkBuilder SetiOSParameters(iOSParameters parameters)
        {
            _builder.SetIosParameters(parameters.ToNative());
            return this;
        }

        public IDynamicLinkBuilder SetSocialMetaTagParameters(SocialMetaTagParameters parameters)
        {
            _builder.SetSocialMetaTagParameters(parameters.ToNative());
            return this;
        }

        public IDynamicLinkBuilder SetGoogleAnalyticsParameters(GoogleAnalyticsParameters parameters)
        {
            _builder.SetGoogleAnalyticsParameters(parameters.ToNative());
            return this;
        }

        public IDynamicLinkBuilder SetNavigationInfoParameters(NavigationInfoParameters parameters)
        {
            _builder.SetNavigationInfoParameters(parameters.ToNative());
            return this;
        }

        public IDynamicLinkBuilder SetiTunesConnectAnalyticsParameters(iTunesConnectAnalyticsParameters parameters)
        {
            _builder.SetItunesConnectAnalyticsParameters(parameters.ToNative());
            return this;
        }

        public Uri BuildDynamicLink()
        {
            return new Uri(_builder.BuildDynamicLink().Uri.ToString());
        }

        public async Task<ShortDynamicLink> BuildShortDynamicLinkAsync()
        {
            // instead of building the short link directly a workaround is needed to prevent an ApiException: 8
            // -> see https://stackoverflow.com/questions/52152116/short-dynamic-link-error-com-google-android-gms-common-api-apiexception-8
            var link = _builder.BuildDynamicLink(); 
            var result = await _builder.SetLongLink(link.Uri).BuildShortDynamicLink();
            var shortLink = result.JavaCast<IShortDynamicLink>();
            return new ShortDynamicLink(new Uri(shortLink.ShortLink.ToString()), shortLink.Warnings.Select(x => $"{x.Code}: {x.Message}").ToArray());
        }
    }
}