using System;

namespace Plugin.Firebase.Abstractions.Firestore
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FirestorePropertyAttribute : Attribute
    {
        public FirestorePropertyAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }
        
        public string PropertyName { get; }
    }
}