using System.Threading.Tasks;
using Plugin.Firebase.DynamicLinks;
using Plugin.Firebase.DynamicLinks.Parameters;
using Xamarin.Essentials;
using Xunit;

namespace Plugin.Firebase.IntegrationTests
{
    public sealed class DynamicLinksFixture
    {
        [Fact]
        public void builds_long_dynamic_link()
        {
            var sut = CrossFirebaseDynamicLinks.Current;
            
            var iOSParameters = new iOSParameters(
                bundleId:"com.example.ios",
                appStoreId:"123456789",
                minimumAppVersion:"1.2.3");
            
            var androidParameters = new AndroidParameters(
                packageName:"com.example.android",
                minimumVersion:123);
            
            var googleAnalyticsParameters = new GoogleAnalyticsParameters(
                source:"orkut",
                medium:"social",
                campaign:"example-promo");
            
            var itunesConnectAnalyticsParameters = new iTunesConnectAnalyticsParameters(
                providerToken:"123456",
                campaignToken:"example-promo");
            
            var socialMetaTagParameters = new SocialMetaTagParameters(
                title:"Example of dynamic link",
                descriptionText:"This link works whether the app is installed or not!",
                imageUrl:"https://www.example.com/my-image.jpg");
            
            var dynamicLink = sut
                .CreateDynamicLink()
                .SetLink("https://www.example.com/my-page")
                .SetDomainUriPrefix("https://example.com/link")
                .SetiOSParameters(iOSParameters)
                .SetAndroidParameters(androidParameters)
                .SetGoogleAnalyticsParameters(googleAnalyticsParameters)
                .SetiTunesConnectAnalyticsParameters(itunesConnectAnalyticsParameters)
                .SetSocialMetaTagParameters(socialMetaTagParameters)
                .BuildDynamicLink();

            Assert.Equal(
                DeviceInfo.Platform == DevicePlatform.iOS
                    ? "https://example.com/link/?ibi=com.example.ios&utm_source=orkut&st=Example%20of%20dynamic%20link&amv=123&ct=example-promo&utm_medium=social&link=https:%2F%2Fwww.example.com%2Fmy-page&imv=1.2.3&utm_campaign=example-promo&isi=123456789&pt=123456&sd=This%20link%20works%20whether%20the%20app%20is%20installed%20or%20not!&si=https:%2F%2Fwww.example.com%2Fmy-image.jpg&apn=com.example.android"
                    : "https://example.com/link?utm_campaign=example-promo&ct=example-promo&pt=123456&sd=This%20link%20works%20whether%20the%20app%20is%20installed%20or%20not!&si=https%3A%2F%2Fwww.example.com%2Fmy-image.jpg&st=Example%20of%20dynamic%20link&amv=123&apn=com.example.android&ibi=com.example.ios&imv=1.2.3&isi=123456789&link=https%3A%2F%2Fwww.example.com%2Fmy-page&utm_medium=social&utm_source=orkut",
                dynamicLink.AbsoluteUri);
        }

        [Fact]
        public async Task builds_short_dynamic_link()
        {
            var sut = CrossFirebaseDynamicLinks.Current;
            
            var dynamicLink = await sut
                .CreateDynamicLink()
                .SetLink("https://pluginfirebase-integrationtest.web.app")
                .SetDomainUriPrefix("https://integrationtests.page.link")
                .SetiOSParameters(new iOSParameters(bundleId:"plugin.firebase.integrationtests"))
                .SetAndroidParameters(new AndroidParameters("plugin.firebase.integrationtests"))
                .BuildShortDynamicLinkAsync();
            
            Assert.StartsWith("https://integrationtests.page.link/", dynamicLink.ShortLink.AbsoluteUri);
        }
    }
}