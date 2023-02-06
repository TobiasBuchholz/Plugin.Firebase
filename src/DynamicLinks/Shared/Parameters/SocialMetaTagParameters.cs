namespace Plugin.Firebase.DynamicLinks.Parameters
{
    public sealed class SocialMetaTagParameters
    {
        public SocialMetaTagParameters(
            string title = null,
            string descriptionText = null,
            string imageUrl = null)
        {
            Title = title;
            DescriptionText = descriptionText;
            ImageUrl = imageUrl;
        }

        public string Title { get; }
        public string DescriptionText { get; }
        public string ImageUrl { get; }
    }
}