namespace Plugin.Firebase.Auth;

public sealed class UserMetadata
{
    public UserMetadata(DateTimeOffset creationDate, DateTimeOffset lastSignInDate)
    {
        CreationDate = creationDate;
        LastSignInDate = lastSignInDate;
    }

    public DateTimeOffset CreationDate { get; }
    public DateTimeOffset LastSignInDate { get; }
}