namespace Plugin.Firebase.Firestore;

/// <summary>
/// Specifies the type of change to a document.
/// </summary>
public enum DocumentChangeType
{
    /// <summary>
    /// Indicates a new document was added to the result set.
    /// </summary>
    Added,

    /// <summary>
    /// Indicates an existing document was modified.
    /// </summary>
    Modified,

    /// <summary>
    /// Indicates a document was removed from the result set.
    /// </summary>
    Removed,
}