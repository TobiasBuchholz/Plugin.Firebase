namespace Plugin.Firebase.Common;

public class FirebaseException : Exception
{
    public FirebaseException()
    {
    }

    public FirebaseException(string message)
        : base(message)
    {
    }

    public FirebaseException(string message, Exception inner)
        : base(message, inner)
    {
    }
}