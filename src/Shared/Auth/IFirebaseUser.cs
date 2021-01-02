using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plugin.Firebase.Auth
{
    /// <summary>
    /// Represents a user. Firebase Auth does not attempt to validate users when loading them from the keychain. Invalidated users
    /// (such as those whose passwords have been changed on another client) are automatically logged out when an auth-dependent operation
    /// is attempted or when the ID token is automatically refreshed.
    /// </summary>
    public interface IFirebaseUser
    {
        /// <summary>
        /// Updates the email address for the user. On success, the cached user profile data is updated. May fail if there is already
        /// an account with this email address that was created using email and password authentication.
        /// </summary>
        /// <param name="email">The email address for the user.</param>
        Task UpdateEmailAsync(string email);
        
        /// <summary>
        /// Updates the password for the user. On success, the cached user profile data is updated.
        /// </summary>
        /// <param name="password">The new password for the user.</param>
        Task UpdatePasswordAsync(string password);
        
        /// <summary>
        /// Updates the phone number for the user. On success, the cached user profile data is updated.
        /// </summary>
        /// <param name="verificationId">A valid verificationId retrieved by calling VerifyPhoneNumber().</param>
        /// <param name="smsCode">The 6 digit SMS-code sent to the user.</param>
        Task UpdatePhoneNumberAsync(string verificationId, string smsCode);
        
        /// <summary>
        /// Change the user’s profile data.
        /// </summary>
        /// <param name="displayName">The user’s display name.</param>
        /// <param name="photoUrl">The user’s photo URL.</param>
        Task UpdateProfileAsync(string displayName = null, string photoUrl = null);
        
        /// <summary>
        /// Initiates email verification for the user.
        /// </summary>
        /// <param name="actionCodeSettings">An <c>ActionCodeSettings</c> object containing settings related to handling action codes.</param>
        Task SendEmailVerificationAsync(ActionCodeSettings actionCodeSettings = null);
        
        /// <summary>
        /// Disassociates a user account from a third-party identity provider with this user.
        /// </summary>
        /// <param name="providerId">The provider ID of the provider to unlink.</param>
        /// <returns></returns>
        Task UnlinkAsync(string providerId);
        
        /// <summary>
        /// Deletes the user account (also signs out the user, if this was the current user).
        /// </summary>
        /// <returns></returns>
        Task DeleteAsync();
        
        /// <summary>
        /// Returns a string used to uniquely identify your user in your Firebase project's user database. 
        /// </summary>
        string Uid { get; }
        
        /// <summary>
        /// Returns the main display name of this user from the Firebase project's user database. 
        /// </summary>
        string DisplayName { get; }
        
        /// <summary>
        /// Returns the main email address of the user, as stored in the Firebase project's user database.
        /// </summary>
        string Email { get; }
        
        /// <summary>
        /// Returns the URL of this user's main profile picture, as stored in the Firebase project's user database. 
        /// </summary>
        string PhotoUrl { get; }
        
        /// <summary>
        /// Always returns <c>FirebaseAuthProvider.PROVIDER_ID</c>.
        /// </summary>
        string ProviderId { get; }
        
        /// <summary>
        /// Indicates the email address associated with this user has been verified.
        /// </summary>
        bool IsEmailVerified { get; }
        
        /// <summary>
        /// Indicates the user represents an anonymous user.
        /// </summary>
        bool IsAnonymous { get; }
        
        /// <summary>
        /// Profile data for each identity provider, if any. This data is cached on sign-in and updated when linking or unlinking.
        /// </summary>
        IEnumerable<ProviderInfo> ProviderInfos { get; }
        
        /// <summary>
        /// Metadata associated with the Firebase user in question.
        /// </summary>
        UserMetadata Metadata { get; }
    }
}