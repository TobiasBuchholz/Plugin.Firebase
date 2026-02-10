namespace Plugin.Firebase.Firestore;

/// <summary>
/// Extension methods for Firestore operations.
/// </summary>
public static class FirestoreExtensions
{
    /// <summary>
    /// Deletes all documents in a collection in batches.
    /// </summary>
    /// <typeparam name="T">The type of the document data.</typeparam>
    /// <param name="this">The Firestore instance.</param>
    /// <param name="collectionPath">The path to the collection to delete.</param>
    /// <param name="batchSize">The maximum number of documents to delete per batch.</param>
    /// <returns>A task that completes when all documents have been deleted.</returns>
    public static async Task DeleteCollectionAsync<T>(
        this IFirebaseFirestore @this,
        string collectionPath,
        int batchSize
    )
    {
        var snapshot = await @this
            .GetCollection(collectionPath)
            .LimitedTo(batchSize)
            .GetDocumentsAsync<T>();
        if(snapshot.Documents.Any()) {
            var batch = @this.CreateBatch();

            foreach(var document in snapshot.Documents) {
                batch.DeleteDocument(document.Reference);
            }
            await batch.CommitAsync();
            await @this.DeleteCollectionAsync<T>(collectionPath, batchSize);
        }
    }
}