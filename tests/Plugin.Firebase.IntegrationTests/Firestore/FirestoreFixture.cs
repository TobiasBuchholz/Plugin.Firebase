using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore
{
    [Collection("Sequential")]
    [TestLogging]
    [Preserve(AllMembers = true)]
    public sealed class FirestoreFixture : IAsyncLifetime
    {
        private static readonly SemaphoreSlim SeedLock = new(1, 1);
        private static bool _basePokemonsSeeded;
        private readonly string _testingCollectionPath = $"testing_{Guid.NewGuid():N}";

        public async Task InitializeAsync()
        {
            await EnsureBasePokemonsSeededAsync();
        }

        [Fact]
        public async Task adds_document_to_collection()
        {
            var sut = CrossFirebaseFirestore.Current;
            var pokemon = PokemonFactory.CreateBulbasur();
            var path = TestingDocumentPath(pokemon.Id);
            var document = GetTestingDocument(sut, pokemon.Id);

            await document.SetDataAsync(pokemon);

            var snapshot = await document.GetDocumentSnapshotAsync<Pokemon>();
            Assert.False(snapshot.Metadata.HasPendingWrites);
            Assert.Equal(pokemon.Id, snapshot.Reference.Id);
            Assert.Equal(path, snapshot.Reference.Path);
            Assert.Equal(pokemon, snapshot.Data);
        }

        [Fact]
        public async Task sets_server_timestamp_via_property_attribute()
        {
            var sut = CrossFirebaseFirestore.Current;
            var pokemon = PokemonFactory.CreateBulbasur();
            var path = TestingDocumentPath(pokemon.Id);

            var document = GetTestingDocument(sut, pokemon.Id);
            await document.SetDataAsync(pokemon);

            var snapshot = await GetTestingDocument(sut, pokemon.Id)
                .GetDocumentSnapshotAsync<Pokemon>(Source.Server);
            Assert.NotEqual(snapshot.Data.ServerTimestamp, DateTimeOffset.MinValue);
            Assert.NotEqual(snapshot.Data.ServerTimestamp, DateTimeOffset.Now);
        }

        [Fact]
        public async Task updates_existing_document()
        {
            var sut = CrossFirebaseFirestore.Current;
            var pokemon = PokemonFactory.CreateSquirtle();
            var path = TestingDocumentPath(pokemon.Id);
            var document = GetTestingDocument(sut, pokemon.Id);

            await document.SetDataAsync(pokemon);
            Assert.Equal(pokemon, (await document.GetDocumentSnapshotAsync<Pokemon>()).Data);

            var update = new Dictionary<object, object> {
                { "name", "Cool Squirtle" },
                { "moves", FieldValue.ArrayUnion("Bubble-Blast") },
                { "first_sighting_location.latitude", 13.37 },
                { "original_reference", document }
            };

            await document.UpdateDataAsync(update);
            var snapshot = await document.GetDocumentSnapshotAsync<Pokemon>();
            Assert.Equal("Cool Squirtle", snapshot.Data.Name);
            Assert.True(snapshot.Data.Moves.Contains("Bubble-Blast"));
            Assert.Equal(13.37, snapshot.Data.FirstSightingLocation.Latitude);
        }

        [Fact]
        public async Task runs_transaction()
        {
            var sut = CrossFirebaseFirestore.Current;
            var bulbasur = PokemonFactory.CreateBulbasur();
            var charmander = PokemonFactory.CreateCharmander();
            var squirtle = PokemonFactory.CreateSquirtle();
            var documentBulbasur = GetTestingDocument(sut, "1");
            var documentCharmander = GetTestingDocument(sut, "4");
            var documentSquirtle = GetTestingDocument(sut, "7");
            var otherMoves = new[] { "other_move", "another_move" };
            await documentBulbasur.SetDataAsync(bulbasur);
            await documentCharmander.SetDataAsync(charmander);

            var charmanderSightingCount = await sut.RunTransactionAsync(transaction => {
                var snapshotCharmander = transaction.GetDocument<Pokemon>(documentCharmander);
                var newSightingCount = snapshotCharmander.Data.SightingCount + 1;
                transaction.SetData(documentSquirtle, squirtle);
                transaction.UpdateData(documentCharmander, ("sighting_count", newSightingCount));
                transaction.UpdateData(documentCharmander, ("moves", otherMoves));
                transaction.UpdateData(documentCharmander, ("items", FieldValue.Delete()));
                transaction.DeleteDocument(documentBulbasur);
                return newSightingCount;
            });

            var charmanderSnapshot = await documentCharmander.GetDocumentSnapshotAsync<Pokemon>();
            Assert.Equal(squirtle, (await documentSquirtle.GetDocumentSnapshotAsync<Pokemon>()).Data);
            Assert.Equal(charmander.SightingCount + 1, charmanderSightingCount);
            Assert.Equal(otherMoves, charmanderSnapshot.Data.Moves);
            Assert.Null(charmanderSnapshot.Data.Items);
            Assert.Null((await documentBulbasur.GetDocumentSnapshotAsync<Pokemon>()).Data);
        }

        [Fact]
        public async Task writes_data_as_batch()
        {
            var sut = CrossFirebaseFirestore.Current;
            var bulbasur = PokemonFactory.CreateBulbasur();
            var charmander = PokemonFactory.CreateCharmander();
            var squirtle = PokemonFactory.CreateSquirtle();
            var documentBulbasur = GetTestingDocument(sut, "1");
            var documentCharmander = GetTestingDocument(sut, "4");
            var documentSquirtle = GetTestingDocument(sut, "7");
            await documentBulbasur.SetDataAsync(bulbasur);
            await documentCharmander.SetDataAsync(charmander);

            var batch = sut.CreateBatch();
            batch.SetData(documentSquirtle, squirtle);
            batch.UpdateData(documentCharmander, ("sighting_count", 1337));
            batch.DeleteDocument(documentBulbasur);
            await batch.CommitAsync();

            Assert.Equal(squirtle, (await documentSquirtle.GetDocumentSnapshotAsync<Pokemon>()).Data);
            Assert.Equal(1337, (await documentCharmander.GetDocumentSnapshotAsync<Pokemon>()).Data.SightingCount);
            Assert.Null((await documentBulbasur.GetDocumentSnapshotAsync<Pokemon>()).Data);
        }

        [Fact]
        public async Task gets_data_with_simple_queries()
        {
            var sut = CrossFirebaseFirestore.Current;
            var collection = sut.GetCollection("pokemons");

            var firePokemons = await collection
                .WhereEqualsTo("poke_type", PokeType.Fire)
                .GetDocumentsAsync<Pokemon>();

            var smallPokemons = await collection
                .WhereLessThanOrEqualsTo("height_in_cm", 100)
                .GetDocumentsAsync<Pokemon>();

            Assert.Equal(3, firePokemons.Documents.Count());
            Assert.Equal(5, smallPokemons.Documents.Count());
        }

        [Fact]
        public async Task gets_data_with_compound_query()
        {
            var sut = CrossFirebaseFirestore.Current;
            var collection = sut.GetCollection("pokemons");

            var smallWaterPokemons = await collection
                .WhereEqualsTo("poke_type", PokeType.Water)
                .WhereGreaterThanOrEqualsTo("height_in_cm", 50)
                .WhereLessThan("height_in_cm", 100)
                .GetDocumentsAsync<Pokemon>();

            Assert.Single(smallWaterPokemons.Documents);
        }

        [Fact]
        public async Task gets_data_with_array_contains_queries()
        {
            var sut = CrossFirebaseFirestore.Current;

            var pokemonsByContains = await sut
                .GetCollection("pokemons")
                .WhereArrayContains("moves", "Razor-Wind")
                .GetDocumentsAsync<Pokemon>();

            var pokemonsByContainsAny = await sut
                .GetCollection("pokemons")
                .WhereArrayContainsAny("moves", new object[] { "Razor-Wind", "Fire-Punch" })
                .GetDocumentsAsync<Pokemon>();

            Assert.Equal(new[] { "1", "2", "3" }, pokemonsByContains.Documents.Select(x => x.Data.Id));
            Assert.Equal(new[] { "1", "2", "3", "4", "5", "6" }, pokemonsByContainsAny.Documents.Select(x => x.Data.Id));
        }

        [Fact]
        public async Task gets_data_using_in_query()
        {
            var sut = CrossFirebaseFirestore.Current;

            var pokemons = await sut
                .GetCollection("pokemons")
                .WhereFieldIn(FieldPath.DocumentId(), new object[] { "1", "2", "3" })
                .GetDocumentsAsync<Pokemon>();

            Assert.Equal(new[] { "1", "2", "3" }, pokemons.Documents.Select(x => x.Data.Id));
        }

        [Fact]
        public async Task orders_and_limits_data()
        {
            var sut = CrossFirebaseFirestore.Current;

            var pokemons = await sut
                .GetCollection("pokemons")
                .OrderBy("name", true)
                .LimitedTo(3)
                .GetDocumentsAsync<Pokemon>();

            Assert.Equal(new[] { "Wartortle", "Venusaur", "Squirtle" }, pokemons.Documents.Select(x => x.Data.Name));
        }

        [Fact]
        public async Task adds_simple_cursor_to_query()
        {
            var sut = CrossFirebaseFirestore.Current;

            var pokemonsByHeight = await sut
                .GetCollection("pokemons")
                .OrderBy("height_in_cm")
                .StartingAt(50)
                .EndingBefore(100)
                .GetDocumentsAsync<Pokemon>();

            var pokemonsByWeight = await sut
                .GetCollection("pokemons")
                .OrderBy("weight_in_kg")
                .StartingAfter(8.5)
                .EndingAt(85.5)
                .GetDocumentsAsync<Pokemon>();

            Assert.Equal(new[] { "7", "4", "1" }, pokemonsByHeight.Documents.Select(x => x.Data.Id));
            Assert.Equal(new[] { "7", "2", "5", "8", "9" }, pokemonsByWeight.Documents.Select(x => x.Data.Id));
        }

        [Fact]
        public async Task uses_document_snapshot_to_define_query_cursor()
        {
            var sut = CrossFirebaseFirestore.Current;

            var snapshot = await sut
                .GetDocument("pokemons/2")
                .GetDocumentSnapshotAsync<Pokemon>();

            var pokemons = await sut
                .GetCollection("pokemons")
                .OrderBy("name")
                .StartingAt(snapshot)
                .GetDocumentsAsync<Pokemon>();

            Assert.Equal(new[] { "Ivysaur", "Squirtle", "Venusaur", "Wartortle" }, pokemons.Documents.Select(x => x.Data.Name));
        }

        [Fact]
        public async Task sets_multiple_cursor_conditions()
        {
            var sut = CrossFirebaseFirestore.Current;

            var pokemons = await sut
                .GetCollection("pokemons")
                .OrderBy("poke_type")
                .OrderBy("name")
                .StartingAt(PokeType.Water, "Squirtle")
                .GetDocumentsAsync<Pokemon>();

            Assert.Equal(new[] { "Squirtle", "Wartortle", "Bulbasaur", "Ivysaur", "Venusaur" }, pokemons.Documents.Select(x => x.Data.Name));
        }

        [Fact]
        public async Task paginates_data()
        {
            var sut = CrossFirebaseFirestore.Current;
            var collection = sut.GetCollection("pokemons");

            var firstPageSnapshot = await collection
                .LimitedTo(5)
                .GetDocumentsAsync<Pokemon>();

            var nextPageSnapshot = await collection
                .LimitedTo(5)
                .StartingAfter(firstPageSnapshot.Documents.Last())
                .GetDocumentsAsync<Pokemon>();

            Assert.Equal(new[] { "1", "2", "3", "4", "5" }, firstPageSnapshot.Documents.Select(x => x.Data.Id));
            Assert.Equal(new[] { "6", "7", "8", "9" }, nextPageSnapshot.Documents.Select(x => x.Data.Id));
        }

        [Fact]
        public async Task gets_real_time_updates_on_single_document()
        {
            var sut = CrossFirebaseFirestore.Current;
            var document = GetTestingDocument(sut, "1");
            await document.SetDataAsync(PokemonFactory.CreateBulbasur());

            var sightingCounts = new List<long>();
            var disposable = document.AddSnapshotListener<Pokemon>(x => {
                if(x.Data != null) {
                    sightingCounts.Add(x.Data.SightingCount);
                }
            });

            for(var i = 0; i < 3; i++) {
                await document.UpdateDataAsync(("sighting_count", i));
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }

            Assert.Equal(new[] { 0L, 1L, 2L }, sightingCounts.Distinct());
            disposable.Dispose();
        }

        [Fact]
        public async Task set_and_get_a_map()
        {
            var sut = CrossFirebaseFirestore.Current;
            var pokemon = PokemonFactory.CreateCharmeleon();
            var path = TestingDocumentPath(pokemon.Id);
            var document = GetTestingDocument(sut, pokemon.Id);

            await document.SetDataAsync(pokemon);

            var snapshot = await document.GetDocumentSnapshotAsync<Pokemon>();
            Assert.False(snapshot.Metadata.HasPendingWrites);
            Assert.Equal(pokemon.Id, snapshot.Reference.Id);
            Assert.Equal(path, snapshot.Reference.Path);
            Assert.Equal(pokemon, snapshot.Data);

            Assert.Equal(4, snapshot.Data.OtherProperties["legs"]);
            Assert.Equal(3, snapshot.Data.OtherProperties["colors"]);

            var updates = new Dictionary<object, object> {
                { "other_properties.colors", FieldValue.IntegerIncrement(1) }
            };

            await document.UpdateDataAsync(updates);

            snapshot = await document.GetDocumentSnapshotAsync<Pokemon>();
            Assert.Equal(4, snapshot.Data.OtherProperties["colors"]);
        }

        [Fact]
        public async Task updates_nested_map_and_datetime_values()
        {
            var sut = CrossFirebaseFirestore.Current;
            var pokemon = PokemonFactory.CreateSquirtle();
            var path = TestingDocumentPath(pokemon.Id);
            var document = GetTestingDocument(sut, pokemon.Id);
            var expectedCreationDate = new DateTime(2024, 1, 2, 3, 4, 5, 678, DateTimeKind.Utc);
            var expectedLocation = new SightingLocation(13.37, 42.24);

            await document.SetDataAsync(pokemon);
            await document.UpdateDataAsync(
                ("creation_date", expectedCreationDate),
                ("first_sighting_location", new Dictionary<object, object> {
                    { "latitude", expectedLocation.Latitude },
                    { "longitude", expectedLocation.Longitude }
                }),
                ("other_properties", new Dictionary<object, object> {
                    { "legs", 4L },
                    { "colors", 3L }
                })
            );

            var snapshot = await document.GetDocumentSnapshotAsync<Pokemon>();
            Assert.InRange(Math.Abs(snapshot.Data.CreationDate.Ticks - expectedCreationDate.Ticks), 0, 10);
            Assert.Equal(expectedLocation, snapshot.Data.FirstSightingLocation);
            Assert.Equal(4L, snapshot.Data.OtherProperties["legs"]);
            Assert.Equal(3L, snapshot.Data.OtherProperties["colors"]);
        }

        [Fact]
        public async Task gets_real_time_updates_on_multiple_documents()
        {
            var sut = CrossFirebaseFirestore.Current;
            var collection = GetTestingCollection(sut);

            var changes = new List<IEnumerable<(DocumentChangeType, string)>>();
            var disposable = collection
                .WhereEqualsTo("poke_type", PokeType.Fire)
                .AddSnapshotListener<Pokemon>(x => {
                    changes.Add(x.DocumentChanges.Select(y => (y.ChangeType, y.DocumentSnapshot.Data.Name)));
                });

            await collection.GetDocument("4").SetDataAsync(PokemonFactory.CreateCharmander());
            await Task.Delay(TimeSpan.FromMilliseconds(500));

            await collection.GetDocument("5").SetDataAsync(PokemonFactory.CreateCharmeleon());
            await Task.Delay(TimeSpan.FromMilliseconds(500));

            await collection.GetDocument("6").SetDataAsync(PokemonFactory.CreateCharizard());
            await Task.Delay(TimeSpan.FromMilliseconds(500));

            await collection.GetDocument("4").UpdateDataAsync(("sighting_count", 1337));
            await Task.Delay(TimeSpan.FromMilliseconds(500));

            await collection.GetDocument("5").DeleteDocumentAsync();
            await Task.Delay(TimeSpan.FromMilliseconds(500));

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

            Assert.Equal(expectedChanges, changes.SelectMany(x => x));
            disposable.Dispose();
        }

        [Fact]
        public async Task deletes_document()
        {
            var sut = CrossFirebaseFirestore.Current;
            var pokemon = PokemonFactory.CreateCharmander();
            var path = TestingDocumentPath(pokemon.Id);
            var document = GetTestingDocument(sut, pokemon.Id);

            await document.SetDataAsync(pokemon);
            Assert.NotNull((await GetTestingDocument(sut, pokemon.Id).GetDocumentSnapshotAsync<Pokemon>()).Data);

            await document.DeleteDocumentAsync();
            Assert.Null((await GetTestingDocument(sut, pokemon.Id).GetDocumentSnapshotAsync<Pokemon>()).Data);
        }

        [Fact]
        public async Task deletes_fields_of_document()
        {
            var sut = CrossFirebaseFirestore.Current;
            var pokemon = PokemonFactory.CreateCharmander();
            var path = TestingDocumentPath(pokemon.Id);
            var document = GetTestingDocument(sut, pokemon.Id);
            await document.SetDataAsync(pokemon);

            await document.UpdateDataAsync(
                ("moves", FieldValue.Delete()),
                ("items", FieldValue.Delete()),
                ("first_sighting_location", FieldValue.Delete()),
                ("poke_type", FieldValue.Delete()));

            var snapshot = await document.GetDocumentSnapshotAsync<Pokemon>();
            Assert.Null(snapshot.Data.Moves);
            Assert.Null(snapshot.Data.FirstSightingLocation);
            Assert.Null(snapshot.Data.Items);
            Assert.Equal(PokeType.Undefined, snapshot.Data.PokeType);
        }

        [Fact]
        public async Task copies_document_id_in_firestore_document_id_attributed_property()
        {
            var sut = CrossFirebaseFirestore.Current;
            var item = new SimpleItem(title: "test");
            var path = TestingDocumentPath("1337");
            var document = GetTestingDocument(sut, "1337");

            await document.SetDataAsync(item);

            var snapshot = await document.GetDocumentSnapshotAsync<SimpleItem>();
            Assert.Equal("1337", snapshot.Data.Id);
            Assert.Equal("1337", snapshot.Reference.Id);
        }

        [Fact]
        public async Task clones_pokemon_with_original_reference()
        {
            var sut = CrossFirebaseFirestore.Current;
            var bulbasurReference = sut.GetDocument($"pokemons/1");
            var bulbasur = (await bulbasurReference.GetDocumentSnapshotAsync<Pokemon>()).Data;
            var copy = bulbasur.Clone(bulbasurReference);
            var copyPath = TestingDocumentPath(copy.Id);
            var copyDocument = GetTestingDocument(sut, copy.Id);
            await copyDocument.SetDataAsync(copy);

            var copySnapshot = await copyDocument.GetDocumentSnapshotAsync<Pokemon>();
            Assert.False(copySnapshot.Metadata.HasPendingWrites);
            Assert.Equal($"{bulbasur.Id}_copied", copySnapshot.Reference.Id);
            Assert.Equal(copyPath, copySnapshot.Reference.Path);
            Assert.Equal(copy, copySnapshot.Data);
        }

        [Fact]
        public async Task retrieves_subs_collection()
        {
            var sut = CrossFirebaseFirestore.Current;
            var pokemon = PokemonFactory.CreateBulbasur();
            var path = TestingDocumentPath(pokemon.Id);
            var subCollectionName = "sub_items";
            var subCollectionPath = $"{path}/{subCollectionName}";
            var document = GetTestingDocument(sut, pokemon.Id);
            var subDocument = sut.GetDocument($"{subCollectionPath}/123");

            await document.SetDataAsync(pokemon);
            await subDocument.SetDataAsync(new Dictionary<object, object>() { { "foo", "bar" } });

            var subCollectionRef1 = sut.GetCollection(subCollectionPath);
            var subCollectionRef2 = document.GetCollection(subCollectionName);
            var snapshot1 = await subCollectionRef1.GetDocumentsAsync<object>();
            var snapshot2 = await subCollectionRef2.GetDocumentsAsync<object>();
            Assert.Single(snapshot1.Documents);
            Assert.Single(snapshot2.Documents);
        }

        [Fact]
        public async Task writes_geopoint_values_inside_firestore_lists()
        {
            var sut = CrossFirebaseFirestore.Current;
            var document = GetTestingDocument(sut, "geopoint-list-values");
            var expected = new[] {
                new GeoPoint(10.5, 20.25),
                new GeoPoint(-33.875, 151.2)
            };

            await document.SetDataAsync(new GeoPointListDocument(expected));

            var result = (await document.GetDocumentSnapshotAsync<GeoPointListDocument>()).Data;
            Assert.Equal(expected[0].Latitude, result.Locations[0].Latitude);
            Assert.Equal(expected[0].Longitude, result.Locations[0].Longitude);
            Assert.Equal(expected[1].Latitude, result.Locations[1].Latitude);
            Assert.Equal(expected[1].Longitude, result.Locations[1].Longitude);
        }

        public async Task DisposeAsync()
        {
            TestLog.Write($"[FIRESTORE CLEANUP START] {_testingCollectionPath}");

            try {
                await CrossFirebaseFirestore.Current
                    .DeleteCollectionAsync<Pokemon>(_testingCollectionPath, batchSize: 10)
                    .WaitAsync(TimeSpan.FromSeconds(15));
                TestLog.Write($"[FIRESTORE CLEANUP END] {_testingCollectionPath}");
            } catch(TimeoutException) {
                TestLog.Write($"[FIRESTORE CLEANUP TIMEOUT] {_testingCollectionPath}");
            } catch(Exception e) {
                TestLog.Write($"[FIRESTORE CLEANUP ERROR] {_testingCollectionPath}: {e}");
            }
        }

        private string TestingDocumentPath(string documentId)
        {
            return $"{_testingCollectionPath}/{documentId}";
        }

        private IDocumentReference GetTestingDocument(IFirebaseFirestore firestore, string documentId)
        {
            return firestore.GetDocument(TestingDocumentPath(documentId));
        }

        private ICollectionReference GetTestingCollection(IFirebaseFirestore firestore)
        {
            return firestore.GetCollection(_testingCollectionPath);
        }

        [Preserve(AllMembers = true)]
        private sealed class GeoPointListDocument : IFirestoreObject
        {
            public GeoPointListDocument()
            {
                // needed for firestore
            }

            public GeoPointListDocument(IList<GeoPoint> locations)
            {
                Locations = locations;
            }

            [FirestoreProperty("locations")]
            public IList<GeoPoint> Locations { get; private set; }
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
            } finally {
                SeedLock.Release();
            }
        }
    }
}
