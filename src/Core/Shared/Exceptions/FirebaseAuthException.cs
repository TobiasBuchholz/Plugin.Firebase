namespace Plugin.Firebase.Core.Exceptions;

public enum FIRAuthError
{
    /// <summary>
    /// Unknown error reason.
    /// </summary>
    Undefined = 0,
    /// <summary>
    /// Indicates the email address is malformed.
    /// </summary>
    InvalidEmail = 1,
    /// <summary>
    /// Indicates the user attempted sign in with a wrong password.
    /// </summary>
    WrongPassword = 2,
    /// <summary>
    /// Indicates an attempt to set a password that is considered too weak.
    /// </summary>
    WeakPassword = 3,
    /// <summary>
    /// Indicates the email used to attempt sign up already exists.
    /// </summary>
    EmailAlreadyInUse = 4,
    /// <summary>
    /// Indicates the user account was not found.
    /// </summary>
    UserNotFound = 5,
    /// <summary>
    /// Indicates the current user’s token has expired, for example, the user may have changed account password on another device.
    /// You must prompt the user to sign in again on this device.
    /// </summary>
    UserTokenExpired = 6,
    /// <summary>
    /// Indicates the supplied credential is invalid. This could happen if it has expired or it is malformed.
    /// </summary>
    InvalidCredential = 7,
    /// <summary>
    /// Indicates the user's account is disabled.
    /// </summary>
    UserDisabled = 8,
    /// <summary>
    /// Indicates the user already signed in once with a trusted provider and hence cannot sign in with another provider.
    /// List of trusted and untrusted providers: https://firebase.google.com/docs/auth/users#verified_email_addresses
    /// </summary>
    AccountExistsWithDifferentCredential = 9,
    /// <summary>
    /// Indicates a validation error with the custom token.
    /// </summary>
    InvalidCustomToken = 10,
    /// <summary>
    /// Indicates the service account and the API key belong to different projects.
    /// </summary>
    CustomTokenMismatch = 11,
    /// <summary>
    /// Indicates the saved auth credential is invalid. The user needs to sign in again.
    /// </summary>
    InvalidUserToken = 12,
    /// <summary>
    /// Indicates that the operation is not allowed (provider disabled or email/password not enabled).
    /// </summary>
    OperationNotAllowed = 13,
    /// <summary>
    /// Indicates that the user must reauthenticate due to a recent-login requirement.
    /// </summary>
    RequiresRecentLogin = 14,
    /// <summary>
    /// Indicates that a different user than the current user was used for reauthentication.
    /// </summary>
    UserMismatch = 15,
    /// <summary>
    /// Indicates that the provider has already been linked to the user.
    /// </summary>
    ProviderAlreadyLinked = 16,
    /// <summary>
    /// Indicates the provider is not linked to the user.
    /// </summary>
    NoSuchProvider = 17,
    /// <summary>
    /// Indicates the email asserted by a credential is already in use by another account.
    /// </summary>
    CredentialAlreadyInUse = 18,
    /// <summary>
    /// Indicates that the phone auth credential was created with an empty verification ID.
    /// </summary>
    MissingVerificationId = 19,
    /// <summary>
    /// Indicates that the phone auth credential was created with an empty verification code.
    /// </summary>
    MissingVerificationCode = 20,
    /// <summary>
    /// Indicates that the phone auth credential was created with an invalid verification code.
    /// </summary>
    InvalidVerificationCode = 21,
    /// <summary>
    /// Indicates that the phone auth credential was created with an invalid verification ID.
    /// </summary>
    InvalidVerificationId = 22,
    /// <summary>
    /// Indicates that the SMS code has expired.
    /// </summary>
    SessionExpired = 23,
    /// <summary>
    /// Indicates that the quota of SMS messages for a given project has been exceeded.
    /// </summary>
    QuotaExceeded = 24,
    /// <summary>
    /// Indicates that the APNs device token was not obtained or forwarded.
    /// </summary>
    MissingAppToken = 25,
    /// <summary>
    /// Indicates that the APNs device token was missing when required for phone auth.
    /// </summary>
    MissingAppCredential = 26,
    /// <summary>
    /// Indicates that an invalid APNs device token was used.
    /// </summary>
    InvalidAppCredential = 27,
    /// <summary>
    /// Indicates that a notification was not forwarded to Firebase Auth when required.
    /// </summary>
    NotificationNotForwarded = 28,
    /// <summary>
    /// Indicates an invalid recipient email was sent in the request.
    /// </summary>
    InvalidRecipientEmail = 29,
    /// <summary>
    /// Indicates an invalid sender email is set in the console for this action.
    /// </summary>
    InvalidSender = 30,
    /// <summary>
    /// Indicates an invalid email template for sending update email.
    /// </summary>
    InvalidMessagePayload = 31,
    /// <summary>
    /// Indicates that the iOS bundle ID is missing when required.
    /// </summary>
    MissingIosBundleId = 32,
    /// <summary>
    /// Indicates that the Android package name is missing when required.
    /// </summary>
    MissingAndroidPackageName = 33,
    /// <summary>
    /// Indicates that the domain specified in the continue URL is not allowlisted.
    /// </summary>
    UnauthorizedDomain = 34,
    /// <summary>
    /// Indicates that the domain specified in the continue URL is not valid.
    /// </summary>
    InvalidContinueUri = 35,
    /// <summary>
    /// Indicates an invalid API key was supplied in the request.
    /// </summary>
    InvalidApiKey = 36,
    /// <summary>
    /// Indicates the app is not authorized to use Firebase Authentication with the provided API key.
    /// </summary>
    AppNotAuthorized = 37,
    /// <summary>
    /// Indicates an error occurred when accessing the keychain.
    /// </summary>
    KeychainError = 38,
    /// <summary>
    /// Indicates an internal error occurred.
    /// </summary>
    InternalError = 39,
    /// <summary>
    /// Indicates a network error occurred during the operation.
    /// </summary>
    NetworkError = 40,
    /// <summary>
    /// Indicates the request has been blocked after an abnormal number of requests.
    /// </summary>
    TooManyRequests = 41,
    /// <summary>
    /// Indicates a web-based sign-in flow failed due to a web context network error.
    /// </summary>
    WebNetworkRequestFailed = 42
}

public class FirebaseAuthException : FirebaseException
{
    public FIRAuthError Reason { get; }
    public string? ErrorCode { get; }
    public string? NativeErrorDomain { get; }
    public long? NativeErrorCode { get; }
    public string? Email { get; }

    public FirebaseAuthException(FIRAuthError reason)
        : this(reason, string.Empty)
    {
    }

    public FirebaseAuthException(FIRAuthError reason, string message)
        : this(reason, message, null, null, null, null, null)
    {
    }

    public FirebaseAuthException(FIRAuthError reason, string message, Exception inner)
        : this(reason, message, inner, null, null, null, null)
    {
    }

    public FirebaseAuthException(
        FIRAuthError reason,
        string message,
        Exception? inner,
        string? errorCode,
        string? nativeErrorDomain,
        long? nativeErrorCode,
        string? email)
        : base(message, inner)
    {
        Reason = reason;
        ErrorCode = errorCode;
        NativeErrorDomain = nativeErrorDomain;
        NativeErrorCode = nativeErrorCode;
        Email = email;
    }

    public static FirebaseAuthException FromErrorCode(
        string? errorCode,
        string? message,
        Exception? inner = null,
        string? nativeErrorDomain = null,
        long? nativeErrorCode = null,
        string? email = null)
    {
        var reason = MapReason(errorCode);
        return new FirebaseAuthException(
            reason,
            message ?? string.Empty,
            inner,
            errorCode,
            nativeErrorDomain,
            nativeErrorCode,
            email);
    }

    private static FIRAuthError MapReason(string? errorCode)
    {
        if(string.IsNullOrWhiteSpace(errorCode)) {
            return FIRAuthError.Undefined;
        }

        var normalized = NormalizeErrorCode(errorCode);
        if(string.Equals(normalized, "NetworkRequestFailed", StringComparison.OrdinalIgnoreCase)) {
            return FIRAuthError.NetworkError;
        }

        if(Enum.TryParse(normalized, ignoreCase: true, out FIRAuthError reason)) {
            return reason;
        }

        return FIRAuthError.Undefined;
    }

    private static string NormalizeErrorCode(string errorCode)
    {
        var code = errorCode.Trim();

        if(code.StartsWith("ERROR_", StringComparison.OrdinalIgnoreCase)) {
            code = code.Substring("ERROR_".Length);
        }

        if(code.StartsWith("FIRAuthErrorCode", StringComparison.OrdinalIgnoreCase)) {
            code = code.Substring("FIRAuthErrorCode".Length);
        }

        if(code.StartsWith("AuthErrorCode", StringComparison.OrdinalIgnoreCase)) {
            code = code.Substring("AuthErrorCode".Length);
        }

        if(code.Contains('_')) {
            return ToPascalCase(code);
        }

        return code;
    }

    private static string ToPascalCase(string code)
    {
        var parts = code.Split('_', StringSplitOptions.RemoveEmptyEntries);
        if(parts.Length == 0) {
            return code;
        }

        var buffer = new System.Text.StringBuilder(code.Length);
        foreach(var part in parts) {
            if(part.Length == 0) {
                continue;
            }

            var lower = part.ToLowerInvariant();
            buffer.Append(char.ToUpperInvariant(lower[0]));
            if(lower.Length > 1) {
                buffer.Append(lower.Substring(1));
            }
        }

        return buffer.ToString();
    }
}