using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore;

[Collection("Sequential")]
[TestLogging]
[IntegrationTestFixture(IntegrationTestPackage.Firestore)]
[Preserve(AllMembers = true)]
public sealed class FirestoreLifecycleFixture : IAsyncLifetime
{
    private readonly List<string> _documentPaths = [];

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task reads_explicit_server_and_cache_sources()
    {
        var document = CreateDocument("sources");

        await document.SetDataAsync(("title", "source-test"));
        await CrossFirebaseFirestore.Current.WaitForPendingWritesAsync();

        var serverSnapshot = await document.GetDocumentSnapshotAsync<Dictionary<string, object>>(Source.Server);
        var cacheSnapshot = await document.GetDocumentSnapshotAsync<Dictionary<string, object>>(Source.Cache);

        Assert.Equal("source-test", serverSnapshot.Data!["title"]);
        Assert.Equal("source-test", cacheSnapshot.Data!["title"]);
        Assert.False(serverSnapshot.Metadata.HasPendingWrites);
        Assert.True(cacheSnapshot.Metadata.IsFromCache);
    }

    [Fact]
    public async Task reports_pending_write_metadata_while_network_is_disabled()
    {
        var firestore = CrossFirebaseFirestore.Current;
        var document = CreateDocument("offline");
        var pendingSnapshot = new CallbackProbe<IDocumentSnapshot<Dictionary<string, object>>>();

        using var listener = document.AddSnapshotListener<Dictionary<string, object>>(
            snapshot => {
                if(snapshot.Metadata.HasPendingWrites) {
                    pendingSnapshot.TrySetResult(snapshot);
                }
            },
            error => pendingSnapshot.TrySetException(error),
            includeMetaDataChanges: true);

        await firestore.DisableNetworkAsync();
        var writeTask = document.SetDataAsync(("title", "offline-write"));

        try {
            var snapshot = await pendingSnapshot.WaitAsync(
                IntegrationTestTimeouts.Callback,
                "pending Firestore write metadata");
            Assert.True(snapshot.Metadata.HasPendingWrites);

            var cacheSnapshot = await document.GetDocumentSnapshotAsync<Dictionary<string, object>>(Source.Cache);
            Assert.True(cacheSnapshot.Metadata.IsFromCache);
        }
        finally {
            await firestore.EnableNetworkAsync();
            await writeTask.WaitForTestAsync(
                IntegrationTestTimeouts.LongCallback,
                "offline Firestore write flush");
            await firestore.WaitForPendingWritesAsync();
        }
    }

    [Fact]
    public async Task terminates_clears_persistence_and_restarts()
    {
        var firestore = CrossFirebaseFirestore.Current;

        await firestore.TerminateAsync();
        try {
            await firestore.ClearPersistenceAsync();
        }
        finally {
            firestore.Restart();
            ConfigureFirestoreEmulatorIfNeeded(firestore);
        }

        var document = CreateDocument("restart");
        await document.SetDataAsync(("title", "after-restart"));

        var snapshot = await document.GetDocumentSnapshotAsync<Dictionary<string, object>>(Source.Server);
        Assert.Equal("after-restart", snapshot.Data!["title"]);
    }

    [Fact]
    public async Task fails_when_updating_missing_document()
    {
        var document = CreateDocument("missing-update");

        await Assert.ThrowsAnyAsync<Exception>(
            () => document.UpdateDataAsync(("title", "missing")));
    }

    public async Task DisposeAsync()
    {
        foreach(var path in _documentPaths) {
            try {
                await CrossFirebaseFirestore.Current.GetDocument(path).DeleteDocumentAsync();
            } catch {
                // Cleanup is best-effort because some tests intentionally terminate/restart Firestore.
            }
        }
    }

    private IDocumentReference CreateDocument(string prefix)
    {
        var path = $"acceptance_lifecycle/{IntegrationTestData.UniqueId(prefix)}";
        _documentPaths.Add(path);
        return CrossFirebaseFirestore.Current.GetDocument(path);
    }

    private static void ConfigureFirestoreEmulatorIfNeeded(IFirebaseFirestore firestore)
    {
        if(!IntegrationTestEnvironment.ShouldUseFirestoreEmulator) {
            return;
        }

        var endpoint = IntegrationTestEnvironment.FirestoreEmulatorEndpoint;
        firestore.UseEmulator(endpoint.Host, endpoint.Port);
    }
}