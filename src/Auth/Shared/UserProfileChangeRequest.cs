namespace Plugin.Firebase.Auth;

/// <summary>
/// A request object for changing Firebase user profile fields.
/// </summary>
public readonly struct UserProfileChangeRequest
{
    private UserProfileChangeRequest(
        string? displayName,
        bool updatesDisplayName,
        string? photoUrl,
        bool updatesPhotoUrl
    )
    {
        DisplayName = displayName;
        UpdatesDisplayName = updatesDisplayName;
        PhotoUrl = photoUrl;
        UpdatesPhotoUrl = updatesPhotoUrl;
    }

    /// <summary>
    /// Gets the display name to set when <see cref="UpdatesDisplayName"/> is <c>true</c>.
    /// </summary>
    public string? DisplayName { get; }

    /// <summary>
    /// Gets whether the display name should be changed.
    /// </summary>
    public bool UpdatesDisplayName { get; }

    /// <summary>
    /// Gets the photo URL to set when <see cref="UpdatesPhotoUrl"/> is <c>true</c>.
    /// </summary>
    public string? PhotoUrl { get; }

    /// <summary>
    /// Gets whether the photo URL should be changed.
    /// </summary>
    public bool UpdatesPhotoUrl { get; }

    /// <summary>
    /// Builds <see cref="UserProfileChangeRequest"/> instances.
    /// </summary>
    public sealed class Builder
    {
        private string? _displayName;
        private string? _photoUrl;
        private bool _updatesDisplayName;
        private bool _updatesPhotoUrl;

        /// <summary>
        /// Sets the user's display name. Pass <c>null</c> to clear it.
        /// </summary>
        /// <param name="displayName">The display name to set.</param>
        public Builder SetDisplayName(string? displayName)
        {
            _displayName = displayName;
            _updatesDisplayName = true;
            return this;
        }

        /// <summary>
        /// Sets the user's photo URL. Pass <c>null</c> to clear it.
        /// </summary>
        /// <param name="photoUrl">The photo URL to set.</param>
        public Builder SetPhotoUrl(string? photoUrl)
        {
            _photoUrl = photoUrl;
            _updatesPhotoUrl = true;
            return this;
        }

        /// <summary>
        /// Builds a request with the values set on this builder.
        /// </summary>
        public UserProfileChangeRequest Build()
        {
            return new UserProfileChangeRequest(
                _displayName,
                _updatesDisplayName,
                _photoUrl,
                _updatesPhotoUrl
            );
        }
    }
}