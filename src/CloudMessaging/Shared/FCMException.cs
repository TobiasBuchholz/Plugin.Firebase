using Plugin.Firebase.Core.Exceptions;

namespace Plugin.Firebase.CloudMessaging
{
    /// <summary>
    /// Exception thrown when a Firebase Cloud Messaging operation fails.
    /// </summary>
    public sealed class FCMException : FirebaseException
    {
        /// <summary>
        /// Creates a new <c>FCMException</c> with the specified message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public FCMException(string message)
            : base(message) { }
    }
}