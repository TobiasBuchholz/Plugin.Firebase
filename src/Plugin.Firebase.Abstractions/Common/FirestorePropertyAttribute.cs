using System;

namespace Plugin.Firebase.Abstractions.Common
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