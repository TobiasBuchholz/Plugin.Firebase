using System;

namespace Plugin.Firebase.Firestore
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
    
    [AttributeUsage(AttributeTargets.Property)]
    public class FirestoreDocumentIdAttribute : Attribute
    {
        public FirestoreDocumentIdAttribute()
        {
        }
    }
}