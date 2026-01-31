namespace Plugin.Firebase.Core.Exceptions;

/// <summary>
/// Base exception type for Plugin.Firebase.
/// </summary>
public class FirebaseException : Exception
{
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public FirebaseException() { }

    /// <summary>
    /// Creates a new instance with a message.
    /// </summary>
    /// <param name="message">Error message.</param>
    public FirebaseException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Creates a new instance with a message and an inner exception.
    /// </summary>
    /// <param name="message">Error message.</param>
    /// <param name="inner">Inner exception.</param>
    public FirebaseException(string message, Exception inner)
        : base(message, inner)
    {
    }
}