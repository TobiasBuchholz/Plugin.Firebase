namespace Plugin.Firebase.Firestore;

/// <summary>
/// Specifies the type of a <see cref="FieldValue"/> sentinel operation.
/// </summary>
public enum FieldValueType
{
    /// <summary>
    /// Union elements into an array field.
    /// </summary>
    ArrayUnion,

    /// <summary>
    /// Remove elements from an array field.
    /// </summary>
    ArrayRemove,

    /// <summary>
    /// Increment a numeric field by an integer value.
    /// </summary>
    IntegerIncrement,

    /// <summary>
    /// Increment a numeric field by a double value.
    /// </summary>
    DoubleIncrement,

    /// <summary>
    /// Delete a field from a document.
    /// </summary>
    Delete,

    /// <summary>
    /// Set a field to the server-generated timestamp.
    /// </summary>
    ServerTimestamp,
}