using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plugin.Firebase.Auth
{
    public interface IFirebaseUser
    {
        Task UpdateEmailAsync(string email);
        Task UpdatePasswordAsync(string password);
        Task UpdatePhoneNumberAsync(string verificationId, string smsCode);
        Task UpdateProfileAsync(string displayName = null, string photoUrl = null);
        Task SendEmailVerificationAsync(ActionCodeSettings actionCodeSettings = null);
        Task DeleteAsync();
        
        string Uid { get; }
        string DisplayName { get; }
        string Email { get; }
        string PhotoUrl { get; }
        bool IsEmailVerified { get; }
        bool IsAnonymous { get; }
        IEnumerable<ProviderInfo> ProviderInfos { get; }
    }
}