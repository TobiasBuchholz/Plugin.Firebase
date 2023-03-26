namespace Plugin.Firebase.Firestore;

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

[AttributeUsage(AttributeTargets.Property)]
public class FirestoreServerTimestampAttribute : Attribute
{
    public FirestoreServerTimestampAttribute(string propertyName)
    {
        PropertyName = propertyName;
    }

    public string PropertyName { get; }
}