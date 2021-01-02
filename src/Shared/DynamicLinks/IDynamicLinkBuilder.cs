using System;
using System.Threading.Tasks;
using Plugin.Firebase.DynamicLinks.Parameters;

namespace Plugin.Firebase.DynamicLinks
{
    /// <summary>
    /// Builder for creating Dynamic Links.
    /// </summary>
    public interface IDynamicLinkBuilder
    {
        /// <summary>
        /// Set the deep link.
        /// </summary>
        /// <param name="link">
        /// The link your app will open. You can specify any URL your app can handle, such as a link to your app's content, or a URL
        /// that initiates some app-specific logic such as crediting the user with a coupon, or displaying a specific welcome screen.
        /// This link must be a well-formatted URL, be properly URL-encoded, and use the HTTP or HTTPS scheme.
        /// </param>
        /// <returns>The same builder object.</returns>
        IDynamicLinkBuilder SetLink(string link);
        
        /// <summary>
        /// Set the deep link.
        /// </summary>
        /// <param name="link">
        /// The link your app will open. You can specify any URL your app can handle, such as a link to your app's content, or a URL
        /// that initiates some app-specific logic such as crediting the user with a coupon, or displaying a specific welcome screen.
        /// This link must be a well-formatted URL, be properly URL-encoded, and use the HTTP or HTTPS scheme.
        /// </param>
        /// <returns>The same builder object.</returns>
        IDynamicLinkBuilder SetLink(Uri link);
        
        /// <summary>
        /// Sets the domain uri prefix (of the form "//xyz.app.goo.gl", "//custom.com/xyz") to use for this Dynamic Link.
        /// </summary>
        /// <param name="prefix">
        /// The target project's Domain Uri Prefix. You can find this value in the Dynamic Links section of the Firebase console.
        /// </param>
        /// <returns>The same builder object.</returns>
        IDynamicLinkBuilder SetDomainUriPrefix(string prefix);
        
        /// <summary>
        /// Sets the Android parameters.
        /// </summary>
        /// <param name="parameters">The Android parameters.</param>
        /// <returns>The same builder object.</returns>
        IDynamicLinkBuilder SetAndroidParameters(AndroidParameters parameters);
        
        /// <summary>
        /// Sets the iOS parameters.
        /// </summary>
        /// <param name="parameters">The iOS parameters.</param>
        /// <returns>The same builder object.</returns>
        IDynamicLinkBuilder SetiOSParameters(iOSParameters parameters);
        
        /// <summary>
        /// Sets the social meta-tag parameters.
        /// </summary>
        /// <param name="parameters">The SocialMetaTagParameters.</param>
        /// <returns>The same builder object.</returns>
        IDynamicLinkBuilder SetSocialMetaTagParameters(SocialMetaTagParameters parameters);
        
        /// <summary>
        /// Sets the Google Analytics parameters.
        /// </summary>
        /// <param name="parameters">The GoogleAnalyticsParameters</param>
        /// <returns>The same builder object.</returns>
        IDynamicLinkBuilder SetGoogleAnalyticsParameters(GoogleAnalyticsParameters parameters);
        
        /// <summary>
        /// Sets the navigation info parameters.
        /// </summary>
        /// <param name="parameters">The NavigationInfoParameters.</param>
        /// <returns>The same builder object.</returns>
        IDynamicLinkBuilder SetNavigationInfoParameters(NavigationInfoParameters parameters);
        
        /// <summary>
        /// Sets the iTunes Connect App Analytics parameters.
        /// </summary>
        /// <param name="parameters">The ItunesConnectAnalyticsParameters.</param>
        /// <returns>The same builder object.</returns>
        IDynamicLinkBuilder SetiTunesConnectAnalyticsParameters(iTunesConnectAnalyticsParameters parameters);
        
        /// <summary>
        /// Creates a Dynamic Link from the parameters.
        /// </summary>
        Uri BuildDynamicLink();
        
        /// <summary>
        /// Creates a shortened Dynamic Link from the parameters.
        /// </summary>
        Task<ShortDynamicLink> BuildShortDynamicLinkAsync();
    }
}