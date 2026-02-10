namespace Plugin.Firebase.Firestore;

/// <summary>
/// Represents a path to a field in a Firestore document.
/// </summary>
public sealed class FieldPath
{
    private FieldPath(string[] fields = null, bool isDocumentId = false)
    {
        Fields = fields;
        IsDocumentId = isDocumentId;
    }

    /// <summary>
    /// Creates a <c>FieldPath</c> from the provided field names.
    /// </summary>
    /// <param name="fields">An array of field names representing the path.</param>
    /// <returns>A new <c>FieldPath</c> instance.</returns>
    public static FieldPath Of(string[] fields)
    {
        return new FieldPath(fields);
    }

    /// <summary>
    /// Returns a special sentinel <c>FieldPath</c> to refer to the document ID.
    /// </summary>
    /// <returns>A <c>FieldPath</c> representing the document ID.</returns>
    public static FieldPath DocumentId()
    {
        return new FieldPath(isDocumentId: true);
    }

    /// <summary>
    /// Gets the array of field names that make up this path.
    /// </summary>
    public string[] Fields { get; }

    /// <summary>
    /// Gets a value indicating whether this path refers to the document ID.
    /// </summary>
    public bool IsDocumentId { get; }
}