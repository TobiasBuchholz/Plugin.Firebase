namespace Plugin.Firebase.Firestore;

/// <summary>
/// Specifies a custom property name to use when serializing or deserializing a property to/from Firestore.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class FirestorePropertyAttribute : Attribute
{
    /// <summary>
    /// Creates a new <c>FirestorePropertyAttribute</c> with the specified property name.
    /// </summary>
    /// <param name="propertyName">The name to use in Firestore for this property.</param>
    public FirestorePropertyAttribute(string propertyName)
    {
        PropertyName = propertyName;
    }

    /// <summary>
    /// Gets the property name to use in Firestore.
    /// </summary>
    public string PropertyName { get; }
}

/// <summary>
/// Indicates that a property should be populated with the document ID when reading from Firestore.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class FirestoreDocumentIdAttribute : Attribute;

/// <summary>
/// Indicates that a property should be set to the server timestamp when writing to Firestore.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class FirestoreServerTimestampAttribute : Attribute
{
    /// <summary>
    /// Creates a new <c>FirestoreServerTimestampAttribute</c> with the specified property name.
    /// </summary>
    /// <param name="propertyName">The name to use in Firestore for this property.</param>
    public FirestoreServerTimestampAttribute(string propertyName)
    {
        PropertyName = propertyName;
    }

    /// <summary>
    /// Gets the property name to use in Firestore.
    /// </summary>
    public string PropertyName { get; }
}