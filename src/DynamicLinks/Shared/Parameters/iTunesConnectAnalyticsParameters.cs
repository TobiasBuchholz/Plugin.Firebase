namespace Plugin.Firebase.DynamicLinks.Parameters
{
    public sealed class iTunesConnectAnalyticsParameters
    {
        public iTunesConnectAnalyticsParameters(
            string providerToken = null,
            string affiliateToken = null,
            string campaignToken = null)
        {
            ProviderToken = providerToken;
            AffiliateToken = affiliateToken;
            CampaignToken = campaignToken;
        }

        public string ProviderToken { get; }
        public string AffiliateToken { get; }
        public string CampaignToken { get; }
    }
}