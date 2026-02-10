namespace Plugin.Firebase.Auth;

/// <summary>
/// Represents metadata about a Firebase user.
/// </summary>
public sealed class UserMetadata
{
    /// <summary>
    /// Creates a new <c>UserMetadata</c> instance.
    /// </summary>
    /// <param name="creationDate">The date and time the user account was created.</param>
    /// <param name="lastSignInDate">The date and time of the user's last sign-in.</param>
    public UserMetadata(DateTimeOffset creationDate, DateTimeOffset lastSignInDate)
    {
        CreationDate = creationDate;
        LastSignInDate = lastSignInDate;
    }

    /// <summary>
    /// Gets the date and time the user account was created.
    /// </summary>
    public DateTimeOffset CreationDate { get; }

    /// <summary>
    /// Gets the date and time of the user's last sign-in.
    /// </summary>
    public DateTimeOffset LastSignInDate { get; }
}