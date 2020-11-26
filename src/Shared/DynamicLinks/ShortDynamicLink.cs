using System;

namespace Plugin.Firebase.DynamicLinks
{
    public sealed class ShortDynamicLink
    {
        public ShortDynamicLink(Uri shortLink, string[] warnings)
        {
            ShortLink = shortLink;
            Warnings = warnings;
        }

        public override string ToString()
        {
            return ShortLink.ToString();
        }

        public Uri ShortLink { get; }
        public string[] Warnings { get; }
    }
}