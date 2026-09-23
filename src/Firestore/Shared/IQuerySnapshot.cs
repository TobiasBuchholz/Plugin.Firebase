namespace Plugin.Firebase.Firestore;

/// <summary>
/// A <c>IQuerySnapshot</c> contains zero or more <c>IDocumentSnapshot</c> objects.
/// </summary>
/// <typeparam name="T">The type of the document item.</typeparam>
public interface IQuerySnapshot<T>
{
    /// <summary>
    /// Returns an enumerable of the documents that changed since the last snapshot. If this is the first snapshot, all documents will be
    /// in the list as Added changes.
    /// </summary>
    /// <remarks>
    /// Calls with the same <paramref name="includeMetadataChanges"/> value return the same <c>DocumentChange</c> instances.
    /// Passing <c>false</c> returns the same instances as <see cref="DocumentChanges"/>. Added and modified documents use
    /// the same <c>IDocumentSnapshot</c> instances as <see cref="Documents"/>; removed documents have their own.
    /// </remarks>
    /// <param name="includeMetadataChanges">
    /// Whether metadata-only changes (i.e. only <c>IDocumentSnapshot.Metadata</c> changed) should be included.
    /// </param>
    IEnumerable<DocumentChange<T>> GetDocumentChanges(bool includeMetadataChanges);

    /// <summary>
    /// An enumerable of the <c>IDocumentSnapshots</c> that make up this document set.
    /// </summary>
    /// <remarks>
    /// Every read returns the same <c>IDocumentSnapshot</c> instances. This query snapshot keeps them, and any data already
    /// read from them, in memory for as long as it is referenced.
    /// </remarks>
    IEnumerable<IDocumentSnapshot<T>> Documents { get; }

    /// <summary>
    /// Metadata about this snapshot, concerning its source and if it has local modifications.
    /// </summary>
    ISnapshotMetadata Metadata { get; }

    /// <summary>
    /// An enumerable of the documents that changed since the last snapshot. If this is the first snapshot, all documents will be in the
    /// list as Added changes.
    /// </summary>
    /// <remarks>
    /// Every read returns the same <c>DocumentChange</c> instances as <c>GetDocumentChanges(false)</c>. Added and modified
    /// documents use the same <c>IDocumentSnapshot</c> instances as <see cref="Documents"/>.
    /// </remarks>
    IEnumerable<DocumentChange<T>> DocumentChanges { get; }

    /// <summary>
    /// The query on which you called in order to get this <c>IQuerySnapshot</c>.
    /// </summary>
    IQuery Query { get; }

    /// <summary>
    /// Indicates whether this <c>IQuerySnapshot</c> is empty (contains no documents).
    /// </summary>
    bool IsEmpty { get; }

    /// <summary>
    /// The count of documents in this <c>IQuerySnapshot</c>.
    /// </summary>
    int Count { get; }
}