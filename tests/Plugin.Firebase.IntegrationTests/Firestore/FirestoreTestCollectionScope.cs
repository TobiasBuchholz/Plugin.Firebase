using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore;

internal sealed class FirestoreTestCollectionScope : IAsyncDisposable
{
    private readonly TimeSpan _cleanupTimeout;
    private bool _isDisposed;

    private FirestoreTestCollectionScope(string path, TimeSpan cleanupTimeout)
    {
        Path = path;
        _cleanupTimeout = cleanupTimeout;
    }

    public string Path { get; }

    public static FirestoreTestCollectionScope Create(
        string prefix,
        TimeSpan? cleanupTimeout = null)
    {
        return new FirestoreTestCollectionScope(
            IntegrationTestData.UniqueId(prefix).Replace('-', '_'),
            cleanupTimeout ?? IntegrationTestTimeouts.Cleanup);
    }

    public string DocumentPath(string documentId)
    {
        return $"{Path}/{documentId}";
    }

    public IDocumentReference GetDocument(IFirebaseFirestore firestore, string documentId)
    {
        return firestore.GetDocument(DocumentPath(documentId));
    }

    public ICollectionReference GetCollection(IFirebaseFirestore firestore)
    {
        return firestore.GetCollection(Path);
    }

    public async ValueTask DisposeAsync()
    {
        if(_isDisposed) {
            return;
        }

        _isDisposed = true;
        TestLog.Write($"[FIRESTORE CLEANUP START] {Path}");

        try {
            await CrossFirebaseFirestore.Current
                .DeleteCollectionAsync<Dictionary<string, object?>>(Path, batchSize: 10)
                .WaitForTestAsync(_cleanupTimeout, "Firestore collection cleanup");
            TestLog.Write($"[FIRESTORE CLEANUP END] {Path}");
        } catch(TimeoutException) {
            TestLog.Write($"[FIRESTORE CLEANUP TIMEOUT] {Path}");
        } catch(Exception e) {
            TestLog.Write($"[FIRESTORE CLEANUP ERROR] {Path}: {e}");
        }
    }
}