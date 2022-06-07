using System;
using System.Threading.Tasks;

namespace Plugin.Firebase.Auth
{
    /// <summary>
    /// Manages authentication for Firebase apps.
    /// </summary>
    public interface IFirebaseAuth : IDisposable
    {
        /// <summary>
        /// Starts the phone number authentication flow by sending a verification code to the specified phone number.
        /// </summary>
        /// <param name="phoneNumber">The phone number to be verified.</param>
        Task VerifyPhoneNumberAsync(string phoneNumber);

        /// <summary>
        /// Tries to create a new user account with the given email address and password. If successful, it also signs the user in into the app.
        /// </summary>
        /// <param name="email">The user’s email address.</param>
        /// <param name="password">The user’s password.</param>
        Task CreateUserAsync(string email, string password);

        /// <summary>
        /// Asynchronously signs in to Firebase with the given Auth token.
        /// </summary>
        /// <param name="token">A self-signed custom auth token.</param>
        /// <returns>The signed in <c>IFirebaseUser</c> object.</returns>
        Task<IFirebaseUser> SignInWithCustomTokenAsync(string token);


        /// <summary>
        /// Signs in using the verification code that was send to the given phone number after calling <c>VerifyPhoneNumberAsync(phoneNumber)</c>.
        /// </summary>
        /// <param name="verificationCode">The code that was send to the given phone number after calling <c>VerifyPhoneNumberAsync(phoneNumber)</c>.</param>
        /// <returns>The signed in <c>IFirebaseUser</c> object.</returns>
        Task<IFirebaseUser> SignInWithPhoneNumberVerificationCodeAsync(string verificationCode);

        /// <summary>
        /// Signs in using an email address and password.
        /// </summary>
        /// <param name="email">The user’s email address.</param>
        /// <param name="password">The user’s password.</param>
        /// <param name="createsUserAutomatically">If the user doesn't exist, it will be created automatically and signed in afterwards if the value is true.</param>
        /// <returns>The signed in <c>IFirebaseUser</c> object.</returns>
        Task<IFirebaseUser> SignInWithEmailAndPasswordAsync(string email, string password, bool createsUserAutomatically = true);

        /// <summary>
        /// Signs in using an email address and email sign-in link.
        /// </summary>
        /// <param name="email">The user’s email address.</param>
        /// <param name="link">The email sign-in link.</param>
        /// <returns>The signed in <c>IFirebaseUser</c> object.</returns>
        Task<IFirebaseUser> SignInWithEmailLinkAsync(string email, string link);

        /// <summary>
        /// Signs in using a google account.
        /// </summary>
        /// <returns>The signed in <c>IFirebaseUser</c> object.</returns>
        Task<IFirebaseUser> SignInWithGoogleAsync();

        /// <summary>
        /// Signs in using a facebook account.
        /// </summary>
        /// <returns>The signed in <c>IFirebaseUser</c> object.</returns>
        Task<IFirebaseUser> SignInWithFacebookAsync();

        /// <summary>
        /// Signs in using a Apple account.
        /// </summary>
        /// <returns>The signed in <c>IFirebaseUser</c> object.</returns>
        Task<IFirebaseUser> SignInWithAppleAsync();

        /// <summary>
        /// Asynchronously creates and becomes an anonymous user. If there is already an anonymous user signed in, that user will
        /// be returned instead. If there is any other existing user signed in, that user will be signed out.
        /// </summary>
        /// <returns>The signed in <c>IFirebaseUser</c> object.</returns>
        Task<IFirebaseUser> SignInAnonymouslyAsync();

        /// <summary>
        /// Link the signed in user with a phone number.
        /// </summary>
        /// <param name="verificationCode">The code that was send to the given phone number after calling <c>VerifyPhoneNumberAsync(phoneNumber)</c>.</param>
        /// <returns>The signed in <c>IFirebaseUser</c> object.</returns>
        Task<IFirebaseUser> LinkWithPhoneNumberVerificationCodeAsync(string verificationCode);

        /// <summary>
        /// Link the signed in user with the given email address and password.
        /// </summary>
        /// <param name="email">The user’s email address.</param>
        /// <param name="password">The user’s password.</param>
        /// <returns>The signed in <c>IFirebaseUser</c> object.</returns>
        Task<IFirebaseUser> LinkWithEmailAndPasswordAsync(string email, string password);

        /// <summary>
        /// Link the signed in user with a google account.
        /// </summary>
        /// <returns>The signed in <c>IFirebaseUser</c> object.</returns>
        Task<IFirebaseUser> LinkWithGoogleAsync();

        /// <summary>
        /// Link the signed in user with a facebook account.
        /// </summary>
        /// <returns>The signed in <c>IFirebaseUser</c> object.</returns>
        Task<IFirebaseUser> LinkWithFacebookAsync();

        /// <summary>
        /// Fetches the list of all sign-in methods previously used for the provided email address.
        /// </summary>
        /// <param name="email">The email address for which to obtain a list of sign-in methods.</param>
        Task<string[]> FetchSignInMethodsAsync(string email);

        /// <summary>
        /// Sends a sign in with email link to provided email address.
        /// </summary>
        /// <param name="toEmail">The email address of the user.</param>
        /// <param name="actionCodeSettings">An <c>ActionCodeSettings</c> object containing settings related to handling action codes.</param>
        Task SendSignInLink(string toEmail, ActionCodeSettings actionCodeSettings);

        /// <summary>
        /// Signs out the current user.
        /// </summary>
        Task SignOutAsync();

        /// <summary>
        /// Checks if link is an email sign-in link.
        /// </summary>
        /// <param name="link">The email sign-in link.</param>
        bool IsSignInWithEmailLink(string link);

        /// <summary>
        /// Sends authenticated user an email with a link to reset his password.
        /// </summary>
        Task SendPasswordResetEmailAsync();

        /// <summary>
        /// Sends registered user an email with a link to reset his password.
        /// </summary>
        /// <param name="email">The registered user email.</param>
        Task SendPasswordResetEmailAsync(string email);

        /// <summary>
        /// Modify this FirebaseAuth instance to communicate with the Firebase Authentication emulator.
        /// Note: this must be called before this instance has been used to do any operations.
        /// </summary>
        /// <param name="host">The emulator host (for example, 10.0.2.2 on android and localhost on iOS)</param>
        /// <param name="port">The emulator port (for example, 9099)</param>
        void UseEmulator(string host, int port);

        /// <summary>
        /// The currently signed in <c>IFirebaseUser</c> object or <c>null</c> if the user is signed out.
        /// </summary>
        IFirebaseUser CurrentUser { get; }
    }
}