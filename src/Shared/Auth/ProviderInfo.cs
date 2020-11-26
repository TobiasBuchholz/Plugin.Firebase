namespace Plugin.Firebase.Auth
{
    public class ProviderInfo
    {
        public ProviderInfo(string uid, string providerId, string displayName, string email, string phoneNumber, string photoUrl)
        {
            Uid = uid;
            ProviderId = providerId;
            DisplayName = displayName;
            Email = email;
            PhoneNumber = phoneNumber;
            PhotoUrl = photoUrl;
        }

        public string Uid { get; }
        public string ProviderId { get; }
        public string DisplayName { get; }
        public string Email { get; }
        public string PhoneNumber { get; }
        public string PhotoUrl { get; }
    }
}