using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore
{
    [Collection("Sequential")]
    [TestLogging]
    [IntegrationTestFixture(IntegrationTestPackage.Firestore)]
    [Preserve(AllMembers = true)]
    public sealed partial class FirestoreFixture : IAsyncLifetime
    {
        private static readonly SemaphoreSlim SeedLock = new(1, 1);
        private static bool _basePokemonsSeeded;
        private readonly FirestoreTestCollectionScope _testingCollection = FirestoreTestCollectionScope.Create("testing");

        public async Task InitializeAsync()
        {
            await EnsureBasePokemonsSeededAsync();
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

        private static async Task EnsureBasePokemonsSeededAsync()
        {
            if(_basePokemonsSeeded) {
                return;
            }

            await SeedLock.WaitAsync();
            try {
                if(_basePokemonsSeeded) {
                    return;
                }

                TestLog.Write("[FIRESTORE SEED START] pokemons");
                await PokemonFactory.CreateBasePokemonsAtFirestoreAsync();
                _basePokemonsSeeded = true;
                TestLog.Write("[FIRESTORE SEED END] pokemons");
            }
            finally {
                SeedLock.Release();
            }
        }
    }
}