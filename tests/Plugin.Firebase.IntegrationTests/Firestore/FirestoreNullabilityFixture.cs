using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore;

[Collection("Sequential")]
[TestLogging]
[IntegrationTestFixture(IntegrationTestPackage.Firestore)]
[Preserve(AllMembers = true)]
public sealed partial class FirestoreNullabilityFixture : IAsyncLifetime
{
    private readonly FirestoreTestCollectionScope _testingCollection =
        FirestoreTestCollectionScope.Create("nullability_testing");

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _testingCollection.DisposeAsync();
    }

    private string TestingDocumentPath(string documentId)
    {
        return _testingCollection.DocumentPath(documentId);
    }

    private IDocumentReference GetTestingDocument(IFirebaseFirestore firestore, string documentId)
    {
        return _testingCollection.GetDocument(firestore, documentId);
    }

    private ICollectionReference GetTestingCollection(IFirebaseFirestore firestore)
    {
        return _testingCollection.GetCollection(firestore);
    }

    private static void AssertRejects(Action action)
    {
        var exception = Record.Exception(action);
        Assert.NotNull(exception);
    }

    private static async Task AssertRejectsAsync(Func<Task> action)
    {
        var exception = await Record.ExceptionAsync(action);
        Assert.NotNull(exception);
    }

#nullable disable
    private static T RequiredNull<T>() where T : class
    {
        return null;
    }
}