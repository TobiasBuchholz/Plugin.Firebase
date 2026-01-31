namespace Plugin.Firebase.Firestore;

/// <summary>
/// Represents a change to a document in a query result set.
/// </summary>
/// <typeparam name="T">The type of the document data.</typeparam>
public sealed class DocumentChange<T>
{
    /// <summary>
    /// Creates a new <c>DocumentChange</c> instance.
    /// </summary>
    /// <param name="documentSnapshot">The snapshot of the changed document.</param>
    /// <param name="changeType">The type of change that occurred.</param>
    /// <param name="newIndex">The new index of the document in the result set.</param>
    /// <param name="oldIndex">The old index of the document in the result set.</param>
    public DocumentChange(
        IDocumentSnapshot<T> documentSnapshot,
        DocumentChangeType changeType,
        int newIndex,
        int oldIndex
    )
    {
        DocumentSnapshot = documentSnapshot;
        ChangeType = changeType;
        NewIndex = newIndex;
        OldIndex = oldIndex;
    }

    /// <summary>
    /// Gets the snapshot of the changed document.
    /// </summary>
    public IDocumentSnapshot<T> DocumentSnapshot { get; }

    /// <summary>
    /// Gets the type of change that occurred (Added, Modified, or Removed).
    /// </summary>
    public DocumentChangeType ChangeType { get; }

    /// <summary>
    /// Gets the new index of the document in the result set after this change, or a special sentinel value if the document was removed.
    /// </summary>
    public int NewIndex { get; }

    /// <summary>
    /// Gets the old index of the document in the result set before this change, or a special sentinel value if the document was added.
    /// </summary>
    public int OldIndex { get; }
}