namespace Plugin.Firebase.Core.Exceptions;

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