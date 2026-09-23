namespace Plugin.Firebase.Firestore;

/// <summary>
/// A <c>IDocumentSnapshot</c> object contains data read from a document in your Firestore database. The data can be extracted with the data
/// property. For a <c>IDocumentSnapshot</c> object that points to a non-existing document, any data access will return null. You can use
/// the exists property to explicitly verify a documents existence.
/// </summary>
/// <typeparam name="T">The type of the document item.</typeparam>
public interface IDocumentSnapshot<out T> : IDocumentSnapshot
{
    /// <summary>
    /// Retrieves all fields in the document as the given generic type. Returns null if the document doesn't exist.
    /// </summary>
    /// <remarks>
    /// The document is converted to <typeparamref name="T"/> the first time this property is read. Later reads of the same
    /// snapshot return that instance, so changes made to it stay visible through this snapshot. They are not written to
    /// Firestore. If the conversion fails, every read throws the same exception.
    /// </remarks>
    T? Data { get; }

    /// <summary>
    /// Metadata about this snapshot concerning its source and if it has local modifications.
    /// </summary>
    ISnapshotMetadata Metadata { get; }

    /// <summary>
    /// A <c>IDocumentReference</c> object to the document location.
    /// </summary>
    IDocumentReference Reference { get; }
}

/// <summary>
/// Base interface for document snapshots.
/// </summary>
public interface IDocumentSnapshot { }