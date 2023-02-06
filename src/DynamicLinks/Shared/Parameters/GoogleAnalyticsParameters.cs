namespace Plugin.Firebase.DynamicLinks.Parameters
{
    public sealed class GoogleAnalyticsParameters
    {
        public GoogleAnalyticsParameters(
            string source = null,
            string medium = null,
            string campaign = null,
            string term = null,
            string content = null)
        {
            Source = source;
            Medium = medium;
            Campaign = campaign;
            Term = term;
            Content = content;
        }

        public string Source { get; }
        public string Medium { get; }
        public string Campaign { get; }
        public string Term { get; }
        public string Content { get; }
    }
}