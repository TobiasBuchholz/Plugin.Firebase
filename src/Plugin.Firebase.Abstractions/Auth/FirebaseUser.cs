namespace Plugin.Firebase.Abstractions.Auth
{
    public sealed class FirebaseUser
    {
        public FirebaseUser(string uid, string displayName, string email, string photoUrl, bool isEmailVerified, bool isAnonymous)
        {
            Uid = uid;
            DisplayName = displayName;
            Email = email;
            PhotoUrl = photoUrl;
            IsEmailVerified = isEmailVerified;
            IsAnonymous = isAnonymous;
        }

        public string Uid { get; }
        public string DisplayName { get; }
        public string Email { get; }
        public string PhotoUrl { get; }
        public bool IsEmailVerified { get; }
        public bool IsAnonymous { get; }
    }
}