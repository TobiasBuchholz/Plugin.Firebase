using System;

namespace Plugin.Firebase.Common
{
    public sealed class FirebaseException : Exception
    {
        public FirebaseException(string message) 
            : base(message)
        {
        }
    }
}