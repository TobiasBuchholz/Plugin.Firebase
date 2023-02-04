namespace Plugin.Firebase.Firestore
{
    public static class FirestoreExtensions
    {
        public static async Task DeleteCollectionAsync<T>(this IFirebaseFirestore @this, string collectionPath, int batchSize)
        {
            var snapshot = await @this.GetCollection(collectionPath).LimitedTo(batchSize).GetDocumentsAsync<T>();
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
}