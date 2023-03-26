using Plugin.Firebase.Core.Exceptions;

namespace Plugin.Firebase.CloudMessaging
{
    public sealed class FCMException : FirebaseException
    {
        public FCMException(string message)
            : base(message)
        {
        }
    }
}