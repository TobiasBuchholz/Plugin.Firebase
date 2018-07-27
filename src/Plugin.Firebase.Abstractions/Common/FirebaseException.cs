using System;

namespace Plugin.Firebase.Abstractions.Common
{
    public sealed class FirebaseException : Exception
    {
        public FirebaseException(string message) 
            : base(message)
        {
        }
    }
}