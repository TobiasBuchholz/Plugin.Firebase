using System.Collections.Generic;

namespace Plugin.Firebase.Auth
{
    public sealed class FirebaseUser
    {
        public static FirebaseUser Empty() =>
            new FirebaseUser("", "", "", "", false, false, null);
        
        public FirebaseUser(
            string uid,
            string displayName,
            string email,
            string photoUrl,
            bool isEmailVerified,
            bool isAnonymous,
            IEnumerable<ProviderInfo> providerInfos)
        {
            Uid = uid;
            DisplayName = displayName;
            Email = email;
            PhotoUrl = photoUrl;
            IsEmailVerified = isEmailVerified;
            IsAnonymous = isAnonymous;
            ProviderInfos = providerInfos;
        }

        public override string ToString()
        {
            return $"[{nameof(FirebaseUser)}: {nameof(Uid)}={Uid}, {nameof(Email)}={Email}]";
        }

        public string Uid { get; }
        public string DisplayName { get; }
        public string Email { get; }
        public string PhotoUrl { get; }
        public bool IsEmailVerified { get; }
        public bool IsAnonymous { get; }
        public IEnumerable<ProviderInfo> ProviderInfos { get; }
    }
}