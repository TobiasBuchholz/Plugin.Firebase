namespace Plugin.Firebase.Auth;

/// <summary>
/// Represents profile data from a third-party identity provider associated with a Firebase user.
/// </summary>
public class ProviderInfo
{
    /// <summary>
    /// Creates a new <c>ProviderInfo</c> instance.
    /// </summary>
    /// <param name="uid">The user's unique ID within the provider.</param>
    /// <param name="providerId">The ID of the identity provider.</param>
    /// <param name="displayName">The user's display name.</param>
    /// <param name="email">The user's email address.</param>
    /// <param name="phoneNumber">The user's phone number.</param>
    /// <param name="photoUrl">The URL of the user's profile photo.</param>
    public ProviderInfo(
        string uid,
        string providerId,
        string? displayName,
        string? email,
        string? phoneNumber,
        string? photoUrl
    )
    {
        Uid = uid;
        ProviderId = providerId;
        DisplayName = displayName;
        Email = email;
        PhoneNumber = phoneNumber;
        PhotoUrl = photoUrl;
    }

    /// <summary>
    /// Gets the user's unique ID within the provider.
    /// </summary>
    public string Uid { get; }

    /// <summary>
    /// Gets the ID of the identity provider (e.g., "google.com", "facebook.com").
    /// </summary>
    public string ProviderId { get; }

    /// <summary>
    /// Gets the user's display name from the provider.
    /// </summary>
    public string? DisplayName { get; }

    /// <summary>
    /// Gets the user's email address from the provider.
    /// </summary>
    public string? Email { get; }

    /// <summary>
    /// Gets the user's phone number from the provider.
    /// </summary>
    public string? PhoneNumber { get; }

    /// <summary>
    /// Gets the URL of the user's profile photo from the provider.
    /// </summary>
    public string? PhotoUrl { get; }
}