using System;

namespace Plugin.Firebase.Common
{
    public class FirebaseException : Exception
    {
        public FirebaseException(string message)
            : base(message)
        {
        }
    }
}