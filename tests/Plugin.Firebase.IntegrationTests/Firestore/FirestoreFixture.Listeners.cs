using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore
{
    public sealed partial class FirestoreFixture
    {
        [Fact]
        public async Task gets_real_time_updates_on_single_document()
        {
            var sut = CrossFirebaseFirestore.Current;
            var document = GetTestingDocument(sut, "1");
            await document.SetDataAsync(PokemonFactory.CreateBulbasur());

            var expectedSightingCounts = new[] { 0L, 1L, 2L };
            var sightingCounts = new List<long>();
            var sightingCountLock = new object();
            var receivedExpectedCounts = new CallbackProbe<bool>();
            using var disposable = document.AddSnapshotListener<Pokemon>(x => {
                if(x.Data != null) {
                    lock(sightingCountLock) {
                        sightingCounts.Add(x.Data!.SightingCount);
                        if(expectedSightingCounts.All(sightingCounts.Contains)) {
                            receivedExpectedCounts.TrySetResult(true);
                        }
                    }
                }
            });

            for(var i = 0; i < 3; i++) {
                await document.UpdateDataAsync((Pokemon.SightingCountField, i));
            }

            await receivedExpectedCounts.WaitAsync(
                IntegrationTestTimeouts.Callback,
                "single-document Firestore listener updates");
            lock(sightingCountLock) {
                Assert.Equal(expectedSightingCounts, sightingCounts.Distinct());
            }
        }

        [Fact]
        public async Task gets_real_time_updates_on_multiple_documents()
        {
            var sut = CrossFirebaseFirestore.Current;
            var collection = GetTestingCollection(sut);
            var expectedChanges = new[] {
                (DocumentChangeType.Added, "Charmander"),
                (DocumentChangeType.Modified, "Charmander"),
                (DocumentChangeType.Added, "Charmeleon"),
                (DocumentChangeType.Modified, "Charmeleon"),
                (DocumentChangeType.Added, "Charizard"),
                (DocumentChangeType.Modified, "Charizard"),
                (DocumentChangeType.Modified, "Charmander"),
                (DocumentChangeType.Removed, "Charmeleon")
            };

            var changes = new List<IEnumerable<(DocumentChangeType, string)>>();
            var changesLock = new object();
            using var disposable = collection
                .WhereEqualsTo(Pokemon.PokeTypeField, PokeType.Fire)
                .AddSnapshotListener<Pokemon>(x => {
                    lock(changesLock) {
                        changes.Add(x.DocumentChanges.Select(y => (y.ChangeType, y.DocumentSnapshot.Data!.Name)).ToList());
                    }
                });

            await collection.GetDocument("4").SetDataAsync(PokemonFactory.CreateCharmander());
            await WaitForChangeCountAsync(2);

            await collection.GetDocument("5").SetDataAsync(PokemonFactory.CreateCharmeleon());
            await WaitForChangeCountAsync(4);

            await collection.GetDocument("6").SetDataAsync(PokemonFactory.CreateCharizard());
            await WaitForChangeCountAsync(6);

            await collection.GetDocument("4").UpdateDataAsync((Pokemon.SightingCountField, 1337));
            await WaitForChangeCountAsync(7);

            await collection.GetDocument("5").DeleteDocumentAsync();
            await WaitForChangeCountAsync(expectedChanges.Length);

            Assert.Equal(expectedChanges, GetObservedChanges());

            Task WaitForChangeCountAsync(int expectedMinimumCount)
            {
                return IntegrationTestTasks.EventuallyAsync(
                    () => Assert.True(GetObservedChanges().Count >= expectedMinimumCount),
                    IntegrationTestTimeouts.Callback);
            }

            IReadOnlyList<(DocumentChangeType, string)> GetObservedChanges()
            {
                lock(changesLock) {
                    return changes.SelectMany(x => x).ToList();
                }
            }
        }
    }
}