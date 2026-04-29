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
        public async Task creates_document_with_auto_generated_reference()
        {
            var sut = CrossFirebaseFirestore.Current;
            var collection = GetTestingCollection(sut);
            var document = collection.CreateDocument();
            var item = new SimpleItem("generated-item");

            await document.SetDataAsync(item);

            var snapshot = await document.GetDocumentSnapshotAsync<SimpleItem>();
            Assert.False(string.IsNullOrWhiteSpace(document.Id));
            Assert.False(string.IsNullOrWhiteSpace(document.Path));
            Assert.Equal(document.Id, snapshot.Reference.Id);
            Assert.Equal(document.Id, snapshot.Data.Id);
            Assert.Equal("generated-item", snapshot.Data.Title);
        }

        [Fact]
        public async Task adds_document_with_auto_generated_id()
        {
            var sut = CrossFirebaseFirestore.Current;
            var collection = GetTestingCollection(sut);

            var document = await collection.AddDocumentAsync(new SimpleItem("added-item"));

            var snapshot = await document.GetDocumentSnapshotAsync<SimpleItem>();
            Assert.False(string.IsNullOrWhiteSpace(document.Id));
            Assert.False(string.IsNullOrWhiteSpace(document.Path));
            Assert.Equal(document.Id, snapshot.Reference.Id);
            Assert.Equal(document.Id, snapshot.Data.Id);
            Assert.Equal("added-item", snapshot.Data.Title);
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
        public async Task increments_double_field_values()
        {
            var sut = CrossFirebaseFirestore.Current;
            var pokemon = PokemonFactory.CreateBulbasur();
            var document = GetTestingDocument(sut, "double-increment");

            await document.SetDataAsync(pokemon);
            await document.UpdateDataAsync(("weight_in_kg", FieldValue.DoubleIncrement(0.25)));

            var snapshot = await document.GetDocumentSnapshotAsync<Pokemon>();
            Assert.Equal(pokemon.WeightInKg + 0.25, snapshot.Data.WeightInKg, 6);
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
        public async Task uses_field_path_overloads()
        {
            var sut = CrossFirebaseFirestore.Current;
            var nestedFieldPath = FieldPath.Of(new[] { "first_sighting_location", "latitude" });

            var nestedPathResults = await sut
                .GetCollection("pokemons")
                .WhereEqualsTo(nestedFieldPath, 52.5042112)
                .GetDocumentsAsync<Pokemon>();

            var documentIdResults = await sut
                .GetCollection("pokemons")
                .OrderBy(FieldPath.DocumentId())
                .StartingAt("2")
                .EndingAt("4")
                .GetDocumentsAsync<Pokemon>();

            Assert.Equal(9, nestedPathResults.Count);
            Assert.Equal(new[] { "2", "3", "4" }, documentIdResults.Documents.Select(x => x.Data.Id));
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
        public async Task uses_limited_to_last()
        {
            var sut = CrossFirebaseFirestore.Current;

            var pokemons = await sut
                .GetCollection("pokemons")
                .OrderBy("name")
                .LimitedToLast(3)
                .GetDocumentsAsync<Pokemon>();

            Assert.Equal(new[] { "Squirtle", "Venusaur", "Wartortle" }, pokemons.Documents.Select(x => x.Data.Name));
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
        public async Task uses_snapshot_end_cursors()
        {
            var sut = CrossFirebaseFirestore.Current;
            var snapshot = await sut
                .GetDocument("pokemons/7")
                .GetDocumentSnapshotAsync<Pokemon>();

            var endingAt = await sut
                .GetCollection("pokemons")
                .OrderBy("name")
                .EndingAt(snapshot)
                .GetDocumentsAsync<Pokemon>();

            var endingBefore = await sut
                .GetCollection("pokemons")
                .OrderBy("name")
                .EndingBefore(snapshot)
                .GetDocumentsAsync<Pokemon>();

            Assert.Equal(
                new[] { "Blastoise", "Bulbasaur", "Charizard", "Charmander", "Charmeleon", "Ivysaur", "Squirtle" },
                endingAt.Documents.Select(x => x.Data.Name));
            Assert.Equal(
                new[] { "Blastoise", "Bulbasaur", "Charizard", "Charmander", "Charmeleon", "Ivysaur" },
                endingBefore.Documents.Select(x => x.Data.Name));
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
        public async Task covers_query_snapshot_properties()
        {
            var sut = CrossFirebaseFirestore.Current;
            var snapshot = await sut
                .GetCollection("pokemons")
                .WhereEqualsTo("poke_type", PokeType.Fire)
                .GetDocumentsAsync<Pokemon>();

            Assert.False(snapshot.IsEmpty);
            Assert.Equal(snapshot.Documents.Count(), snapshot.Count);
            Assert.NotNull(snapshot.Query);
            Assert.NotNull(snapshot.Metadata);
            Assert.NotEmpty(snapshot.DocumentChanges);
            Assert.NotEmpty(snapshot.GetDocumentChanges(includeMetadataChanges: false));
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
        public async Task covers_document_set_overloads_with_merge()
        {
            var sut = CrossFirebaseFirestore.Current;
            var mergedDictionaryDocument = GetTestingDocument(sut, "merge-dictionary");
            var tupleDocument = GetTestingDocument(sut, "tuple-set");
            var mergedTupleDocument = GetTestingDocument(sut, "merge-tuple");

            await mergedDictionaryDocument.SetDataAsync(PokemonFactory.CreateCharmander());
            await mergedDictionaryDocument.SetDataAsync(
                new Dictionary<object, object> {
                    { "name", "Merged Charmander" }
                },
                SetOptions.Merge());

            await tupleDocument.SetDataAsync(
                ("name", "Tuple Pokemon"),
                ("sighting_count", 12L));

            await mergedTupleDocument.SetDataAsync(PokemonFactory.CreateSquirtle());
            await mergedTupleDocument.SetDataAsync(
                SetOptions.Merge(),
                ("name", "Merged Squirtle"));

            var mergedDictionarySnapshot = await mergedDictionaryDocument.GetDocumentSnapshotAsync<Pokemon>();
            var tupleSnapshot = await tupleDocument.GetDocumentSnapshotAsync<Pokemon>();
            var mergedTupleSnapshot = await mergedTupleDocument.GetDocumentSnapshotAsync<Pokemon>();

            Assert.Equal("Merged Charmander", mergedDictionarySnapshot.Data.Name);
            Assert.Equal(60, mergedDictionarySnapshot.Data.HeightInCm);
            Assert.Equal("Tuple Pokemon", tupleSnapshot.Data.Name);
            Assert.Equal(12L, tupleSnapshot.Data.SightingCount);
            Assert.Equal("Merged Squirtle", mergedTupleSnapshot.Data.Name);
            Assert.Equal(50, mergedTupleSnapshot.Data.HeightInCm);
        }

        [Fact]
        public async Task covers_batch_set_overloads_and_commit_local()
        {
            var sut = CrossFirebaseFirestore.Current;
            var mergedDictionaryDocument = GetTestingDocument(sut, "batch-merge-dictionary");
            var tupleDocument = GetTestingDocument(sut, "batch-tuple");
            var mergedTupleDocument = GetTestingDocument(sut, "batch-merge-tuple");

            await mergedDictionaryDocument.SetDataAsync(PokemonFactory.CreateCharmander());
            await mergedTupleDocument.SetDataAsync(PokemonFactory.CreateSquirtle());

            var batch = sut.CreateBatch();
            batch.SetData(
                mergedDictionaryDocument,
                new Dictionary<object, object> {
                    { "name", "Batch Merged Charmander" }
                },
                SetOptions.Merge());
            batch.SetData(
                tupleDocument,
                ("name", "Batch Tuple Pokemon"),
                ("sighting_count", 33L));
            batch.SetData(
                mergedTupleDocument,
                SetOptions.Merge(),
                ("name", "Batch Merged Squirtle"));
            batch.CommitLocal();

            await sut.WaitForPendingWritesAsync();

            var mergedDictionarySnapshot = await mergedDictionaryDocument.GetDocumentSnapshotAsync<Pokemon>();
            var tupleSnapshot = await tupleDocument.GetDocumentSnapshotAsync<Pokemon>();
            var mergedTupleSnapshot = await mergedTupleDocument.GetDocumentSnapshotAsync<Pokemon>();

            Assert.Equal("Batch Merged Charmander", mergedDictionarySnapshot.Data.Name);
            Assert.Equal(60, mergedDictionarySnapshot.Data.HeightInCm);
            Assert.Equal("Batch Tuple Pokemon", tupleSnapshot.Data.Name);
            Assert.Equal(33L, tupleSnapshot.Data.SightingCount);
            Assert.Equal("Batch Merged Squirtle", mergedTupleSnapshot.Data.Name);
            Assert.Equal(50, mergedTupleSnapshot.Data.HeightInCm);
        }

        [Fact]
        public async Task covers_transaction_set_overloads()
        {
            var sut = CrossFirebaseFirestore.Current;
            var mergedDictionaryDocument = GetTestingDocument(sut, "transaction-merge-dictionary");
            var mergedTupleDocument = GetTestingDocument(sut, "transaction-merge-tuple");

            await mergedDictionaryDocument.SetDataAsync(PokemonFactory.CreateCharmander());
            await mergedTupleDocument.SetDataAsync(PokemonFactory.CreateSquirtle());

            await sut.RunTransactionAsync(transaction => {
                transaction.SetData(
                    mergedDictionaryDocument,
                    new Dictionary<object, object> {
                        { "name", "Transaction Merged Charmander" }
                    },
                    SetOptions.Merge());
                transaction.SetData(
                    mergedTupleDocument,
                    SetOptions.Merge(),
                    ("name", "Transaction Merged Squirtle"),
                    ("sighting_count", 91L));
                return true;
            });

            var mergedDictionarySnapshot = await mergedDictionaryDocument.GetDocumentSnapshotAsync<Pokemon>();
            var mergedTupleSnapshot = await mergedTupleDocument.GetDocumentSnapshotAsync<Pokemon>();

            Assert.Equal("Transaction Merged Charmander", mergedDictionarySnapshot.Data.Name);
            Assert.Equal(60, mergedDictionarySnapshot.Data.HeightInCm);
            Assert.Equal("Transaction Merged Squirtle", mergedTupleSnapshot.Data.Name);
            Assert.Equal(91L, mergedTupleSnapshot.Data.SightingCount);
            Assert.Equal(50, mergedTupleSnapshot.Data.HeightInCm);
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
        public async Task reads_ios_dictionary_object_numeric_and_boolean_values()
        {
            if(!OperatingSystem.IsIOS()) {
                return;
            }

            var sut = CrossFirebaseFirestore.Current;
            var document = GetTestingDocument(sut, "ios-dictionary-object-values");
            await document.SetDataAsync(new DictionaryObjectValuesDocument(
                new Dictionary<string, object> {
                    { "enabled", true },
                    { "count", 5L },
                    { "ratio", 1.25 }
                }));

            var snapshot = await document.GetDocumentSnapshotAsync<DictionaryObjectValuesDocument>();

            Assert.True((bool) snapshot.Data.Values["enabled"]);
            Assert.Equal(5L, Convert.ToInt64(snapshot.Data.Values["count"]));
            Assert.Equal(1.25, Convert.ToDouble(snapshot.Data.Values["ratio"]));
        }

        [Fact]
        public async Task reads_ios_enum_dictionary_values()
        {
            if(!OperatingSystem.IsIOS()) {
                return;
            }

            var sut = CrossFirebaseFirestore.Current;
            var document = GetTestingDocument(sut, "ios-enum-dictionary-values");
            await document.SetDataAsync(new EnumDictionaryDocument(
                new Dictionary<string, PokeType> {
                    { "fire", PokeType.Fire },
                    { "water", PokeType.Water }
                }));

            var snapshot = await document.GetDocumentSnapshotAsync<EnumDictionaryDocument>();

            Assert.Equal(PokeType.Fire, snapshot.Data.Values["fire"]);
            Assert.Equal(PokeType.Water, snapshot.Data.Values["water"]);
        }

        [Fact]
        public async Task reads_android_typed_numeric_collection_values()
        {
            if(!OperatingSystem.IsAndroid()) {
                return;
            }

            var sut = CrossFirebaseFirestore.Current;
            var document = GetTestingDocument(sut, "android-typed-numeric-collections");
            await document.SetDataAsync(new Dictionary<object, object> {
                {
                    "counts",
                    new Dictionary<object, object> {
                        { "one", 1L },
                        { "two", 2L }
                    }
                },
                { "nullable_counts", new object[] { 1L, null, 3L } },
                {
                    "types",
                    new Dictionary<object, object> {
                        { "fire", PokeType.Fire },
                        { "water", PokeType.Water }
                    }
                }
            });

            var snapshot = await document.GetDocumentSnapshotAsync<AndroidNumericCollectionsDocument>();

            Assert.Equal(1, snapshot.Data.Counts["one"]);
            Assert.Equal(2, snapshot.Data.Counts["two"]);
            Assert.Equal(new int?[] { 1, null, 3 }, snapshot.Data.NullableCounts);
            Assert.Equal(PokeType.Fire, snapshot.Data.Types["fire"]);
            Assert.Equal(PokeType.Water, snapshot.Data.Types["water"]);
        }

        [Fact]
        public async Task writes_set_data_from_dictionary_and_tuple_payloads()
        {
            var sut = CrossFirebaseFirestore.Current;

            var dictionaryDocument = GetTestingDocument(sut, "setdata-string-dictionary");
            await dictionaryDocument.SetDataAsync(new Dictionary<string, object> {
                { "field_a", "dictionary-a" },
                { "field_b", "dictionary-b" }
            });
            var dictionaryResult = (await dictionaryDocument.GetDocumentSnapshotAsync<SetDataPayloadDocument>()).Data;
            Assert.Equal("dictionary-a", dictionaryResult.FieldA);
            Assert.Equal("dictionary-b", dictionaryResult.FieldB);

            var tupleDocument = GetTestingDocument(sut, "setdata-tuple");
            await tupleDocument.SetDataAsync(
                ("field_a", "tuple-a"),
                ("field_b", "tuple-b"));
            var tupleResult = (await tupleDocument.GetDocumentSnapshotAsync<SetDataPayloadDocument>()).Data;
            Assert.Equal("tuple-a", tupleResult.FieldA);
            Assert.Equal("tuple-b", tupleResult.FieldB);

            if(OperatingSystem.IsAndroid() is false) {
                return;
            }

            var transactionDictionaryDocument = GetTestingDocument(sut, "transaction-setdata-string-dictionary");
            var transactionTupleDocument = GetTestingDocument(sut, "transaction-setdata-tuple");
            await sut.RunTransactionAsync(transaction => {
                transaction.SetData(
                    transactionDictionaryDocument,
                    new Dictionary<string, object> {
                        { "field_a", "transaction-dictionary-a" },
                        { "field_b", "transaction-dictionary-b" }
                    });
                transaction.SetData(
                    transactionTupleDocument,
                    ("field_a", "transaction-tuple-a"),
                    ("field_b", "transaction-tuple-b"));
                return true;
            });

            var transactionDictionaryResult = (await transactionDictionaryDocument.GetDocumentSnapshotAsync<SetDataPayloadDocument>()).Data;
            Assert.Equal("transaction-dictionary-a", transactionDictionaryResult.FieldA);
            Assert.Equal("transaction-dictionary-b", transactionDictionaryResult.FieldB);

            var transactionTupleResult = (await transactionTupleDocument.GetDocumentSnapshotAsync<SetDataPayloadDocument>()).Data;
            Assert.Equal("transaction-tuple-a", transactionTupleResult.FieldA);
            Assert.Equal("transaction-tuple-b", transactionTupleResult.FieldB);
        }

        [Fact]
        public async Task gets_document_data_as_dictionary()
        {
            var sut = CrossFirebaseFirestore.Current;
            var document = GetTestingDocument(sut, "raw-data");
            var observedAt = DateTimeOffset.Now;

            await document.SetDataAsync(new Dictionary<object, object> { { "seed", "true" } });
            await document.UpdateDataAsync(
                ("unknown_string", "value"),
                ("unknown_long", 123L),
                ("unknown_double", 12.5),
                ("unknown_bool", true),
                ("unknown_null", null),
                ("unknown_numbers", new[] { 1L, 2L }),
                ("unknown_empty_array", Array.Empty<object>()),
                ("unknown_empty_map", new Dictionary<object, object>()),
                ("unknown_array_with_nulls", new object[] {
                    null,
                    "text",
                    3L,
                    new Dictionary<object, object> {
                        { "child_null", null },
                        { "child_text", "child" }
                    },
                    false
                }),
                ("unknown_map_array", new[] {
                    new Dictionary<object, object> {
                        { "name", "first" },
                        { "score", 1L },
                        { "active", true }
                    },
                    new Dictionary<object, object> {
                        { "name", "second" },
                        { "score", 2L },
                        { "active", false }
                    }
                }),
                ("nested.answer", 42L),
                ("nested.values", new[] { "one", "two" }),
                ("nested.deep.answer", 84L),
                ("nested.deep.null_value", null),
                ("nested.label", "nested value"),
                ("nested.null_value", null),
                ("nested.empty_values", Array.Empty<object>()),
                ("nested.empty_map", new Dictionary<object, object>()),
                ("nested.direct_map", new Dictionary<object, object> {
                    { "text", "direct" },
                    { "count", 9L },
                    { "short_count", (short) 7 },
                    { "flags", new[] { true, false } },
                    { "inner", new Dictionary<object, object> { { "value", "inside" } } }
                }),
                ("observed_at", observedAt),
                ("created_at", observedAt.UtcDateTime),
                ("generated_at", FieldValue.ServerTimestamp()),
                ("location", new GeoPoint(1.25, 2.5)),
                ("original_reference", document));

            var dictionarySnapshot = await document.GetDocumentSnapshotAsync<Dictionary<string, object>>();
            AssertRawDictionaryData(dictionarySnapshot.Data, document);

            var interfaceSnapshot = await document.GetDocumentSnapshotAsync<IDictionary<string, object>>();
            AssertRawDictionaryData(interfaceSnapshot.Data, document);

            var objectDictionarySnapshot = await document.GetDocumentSnapshotAsync<Dictionary<object, object>>();
            AssertRawObjectDictionaryData(objectDictionarySnapshot.Data, document);

            var objectSnapshot = await document.GetDocumentSnapshotAsync<object>();
            AssertRawDictionaryData(
                Assert.IsAssignableFrom<IDictionary<string, object>>(objectSnapshot.Data),
                document);

            var querySnapshot = await GetTestingCollection(sut)
                .WhereEqualsTo("unknown_string", "value")
                .GetDocumentsAsync<Dictionary<string, object>>();
            AssertRawDictionaryData(Assert.Single(querySnapshot.Documents).Data, document);
        }

        [Fact]
        public async Task gets_document_data_as_strongly_typed_dictionaries()
        {
            var sut = CrossFirebaseFirestore.Current;

            var stringDocument = GetTestingDocument(sut, "typed-string-map");
            await stringDocument.SetDataAsync(new Dictionary<object, object> {
                { "alpha", "one" },
                { "beta", "two" }
            });
            var strings = (await stringDocument.GetDocumentSnapshotAsync<Dictionary<string, string>>()).Data;
            Assert.Equal("one", strings["alpha"]);
            Assert.Equal("two", strings["beta"]);

            var boolDocument = GetTestingDocument(sut, "typed-bool-map");
            await boolDocument.SetDataAsync(new Dictionary<object, object> {
                { "enabled", true },
                { "archived", false }
            });
            var bools = (await boolDocument.GetDocumentSnapshotAsync<Dictionary<object, bool>>()).Data;
            Assert.All(bools.Keys, key => Assert.IsType<string>(key));
            Assert.True(bools["enabled"]);
            Assert.False(bools["archived"]);

            var longDocument = GetTestingDocument(sut, "typed-long-map");
            await longDocument.SetDataAsync(new Dictionary<object, object> {
                { "one", 1L },
                { "two", 2 }
            });
            var longs = (await longDocument.GetDocumentSnapshotAsync<IDictionary<string, long>>()).Data;
            Assert.Equal(1L, longs["one"]);
            Assert.Equal(2L, longs["two"]);

            var intDocument = GetTestingDocument(sut, "typed-int-map");
            await intDocument.SetDataAsync(new Dictionary<object, object> {
                { "one", 1L },
                { "two", 2 }
            });
            var ints = (await intDocument.GetDocumentSnapshotAsync<Dictionary<string, int>>()).Data;
            Assert.Equal(1, ints["one"]);
            Assert.Equal(2, ints["two"]);

            var doubleDocument = GetTestingDocument(sut, "typed-double-map");
            await doubleDocument.SetDataAsync(new Dictionary<object, object> {
                { "half", 0.5 },
                { "whole", 2L }
            });
            var doubles = (await doubleDocument.GetDocumentSnapshotAsync<Dictionary<string, double>>()).Data;
            Assert.Equal(0.5, doubles["half"]);
            Assert.Equal(2.0, doubles["whole"]);

            var floatDocument = GetTestingDocument(sut, "typed-float-map");
            await floatDocument.SetDataAsync(new Dictionary<object, object> {
                { "half", 0.5 },
                { "whole", 2L }
            });
            var floats = (await floatDocument.GetDocumentSnapshotAsync<Dictionary<string, float>>()).Data;
            Assert.Equal(0.5f, floats["half"]);
            Assert.Equal(2.0f, floats["whole"]);

            var enumDocument = GetTestingDocument(sut, "typed-enum-map");
            await enumDocument.SetDataAsync(new Dictionary<object, object> {
                { "fire", PokeType.Fire },
                { "water", PokeType.Water }
            });
            var enums = (await enumDocument.GetDocumentSnapshotAsync<Dictionary<string, PokeType>>()).Data;
            Assert.Equal(PokeType.Fire, enums["fire"]);
            Assert.Equal(PokeType.Water, enums["water"]);
        }

        [Fact]
        public async Task gets_document_data_as_additional_numeric_dictionaries()
        {
            var sut = CrossFirebaseFirestore.Current;

            var byteDocument = GetTestingDocument(sut, "typed-byte-map");
            await byteDocument.SetDataAsync(new Dictionary<object, object> {
                { "min", 0L },
                { "max", 255L }
            });
            var bytes = (await byteDocument.GetDocumentSnapshotAsync<Dictionary<string, byte>>()).Data;
            Assert.Equal((byte) 0, bytes["min"]);
            Assert.Equal(byte.MaxValue, bytes["max"]);

            var sbyteDocument = GetTestingDocument(sut, "typed-sbyte-map");
            await sbyteDocument.SetDataAsync(new Dictionary<object, object> {
                { "min", -128L },
                { "max", 127L }
            });
            var sbytes = (await sbyteDocument.GetDocumentSnapshotAsync<Dictionary<string, sbyte>>()).Data;
            Assert.Equal(sbyte.MinValue, sbytes["min"]);
            Assert.Equal(sbyte.MaxValue, sbytes["max"]);

            var shortDocument = GetTestingDocument(sut, "typed-short-map");
            await shortDocument.SetDataAsync(new Dictionary<object, object> {
                { "min", -32768L },
                { "max", 32767L }
            });
            var shorts = (await shortDocument.GetDocumentSnapshotAsync<Dictionary<string, short>>()).Data;
            Assert.Equal(short.MinValue, shorts["min"]);
            Assert.Equal(short.MaxValue, shorts["max"]);

            var ushortDocument = GetTestingDocument(sut, "typed-ushort-map");
            await ushortDocument.SetDataAsync(new Dictionary<object, object> {
                { "min", 0L },
                { "max", 65535L }
            });
            var ushorts = (await ushortDocument.GetDocumentSnapshotAsync<Dictionary<string, ushort>>()).Data;
            Assert.Equal((ushort) 0, ushorts["min"]);
            Assert.Equal(ushort.MaxValue, ushorts["max"]);

            var uintDocument = GetTestingDocument(sut, "typed-uint-map");
            await uintDocument.SetDataAsync(new Dictionary<object, object> {
                { "min", 0L },
                { "max", 4294967295L }
            });
            var uints = (await uintDocument.GetDocumentSnapshotAsync<Dictionary<string, uint>>()).Data;
            Assert.Equal(0U, uints["min"]);
            Assert.Equal(uint.MaxValue, uints["max"]);

            var ulongDocument = GetTestingDocument(sut, "typed-ulong-map");
            await ulongDocument.SetDataAsync(new Dictionary<object, object> {
                { "zero", 0L },
                { "value", 9223372036854775807L }
            });
            var ulongs = (await ulongDocument.GetDocumentSnapshotAsync<Dictionary<string, ulong>>()).Data;
            Assert.Equal(0UL, ulongs["zero"]);
            Assert.Equal(9223372036854775807UL, ulongs["value"]);

            var nullableDocument = GetTestingDocument(sut, "typed-nullable-int-map");
            await nullableDocument.SetDataAsync(new Dictionary<object, object> {
                { "present", 123L },
                { "missing", null }
            });
            var nullableInts = (await nullableDocument.GetDocumentSnapshotAsync<Dictionary<string, int?>>()).Data;
            Assert.Equal(123, nullableInts["present"]);
            Assert.Null(nullableInts["missing"]);
        }

        [Fact]
        public async Task gets_document_data_as_typed_nested_dictionaries()
        {
            var sut = CrossFirebaseFirestore.Current;

            var objectMapDocument = GetTestingDocument(sut, "typed-nested-object-map");
            await objectMapDocument.SetDataAsync(new Dictionary<object, object> {
                {
                    "outer",
                    new Dictionary<object, object> {
                        { "name", "nested" },
                        { "count", 3L },
                        { "active", true },
                        { "empty", new Dictionary<object, object>() },
                        { "inner", new Dictionary<object, object> { { "value", "deep" } } }
                    }
                }
            });
            var objectMaps = (await objectMapDocument.GetDocumentSnapshotAsync<Dictionary<string, Dictionary<string, object>>>()).Data;
            var outerObjectMap = objectMaps["outer"];
            Assert.Equal("nested", outerObjectMap["name"]);
            Assert.Equal(3L, Convert.ToInt64(outerObjectMap["count"]));
            Assert.True((bool) outerObjectMap["active"]);
            Assert.Empty(Assert.IsAssignableFrom<IDictionary<string, object>>(outerObjectMap["empty"]));
            Assert.Equal(
                "deep",
                Assert.IsAssignableFrom<IDictionary<string, object>>(outerObjectMap["inner"])["value"]);

            var interfaceMapDocument = GetTestingDocument(sut, "typed-nested-interface-map");
            await interfaceMapDocument.SetDataAsync(new Dictionary<object, object> {
                {
                    "outer",
                    new Dictionary<object, object> {
                        { "label", "interface" },
                        { "nullable", null }
                    }
                }
            });
            var interfaceMaps = (await interfaceMapDocument.GetDocumentSnapshotAsync<Dictionary<string, IDictionary<string, object>>>()).Data;
            Assert.Equal("interface", interfaceMaps["outer"]["label"]);
            Assert.Null(interfaceMaps["outer"]["nullable"]);

            var longMapDocument = GetTestingDocument(sut, "typed-nested-long-map");
            await longMapDocument.SetDataAsync(new Dictionary<object, object> {
                {
                    "outer",
                    new Dictionary<object, object> {
                        { "one", 1L },
                        { "two", 2 }
                    }
                }
            });
            var longMaps = (await longMapDocument.GetDocumentSnapshotAsync<Dictionary<string, Dictionary<string, long>>>()).Data;
            Assert.Equal(1L, longMaps["outer"]["one"]);
            Assert.Equal(2L, longMaps["outer"]["two"]);
        }

        [Fact]
        public async Task gets_document_data_as_typed_list_dictionaries()
        {
            var sut = CrossFirebaseFirestore.Current;

            var longListDocument = GetTestingDocument(sut, "typed-long-list-map");
            await longListDocument.SetDataAsync(new Dictionary<object, object> {
                { "first", new[] { 1L, 2L } },
                { "second", new[] { 3, 4 } }
            });
            var longLists = (await longListDocument.GetDocumentSnapshotAsync<Dictionary<string, IList<long>>>()).Data;
            Assert.Equal(new[] { 1L, 2L }, longLists["first"]);
            Assert.Equal(new[] { 3L, 4L }, longLists["second"]);

            var nullableListDocument = GetTestingDocument(sut, "typed-nullable-list-map");
            await nullableListDocument.SetDataAsync(new Dictionary<object, object> {
                { "values", new object[] { 1L, null, 3L } }
            });
            var nullableLists = (await nullableListDocument.GetDocumentSnapshotAsync<Dictionary<string, IList<long?>>>()).Data;
            Assert.Equal(new long?[] { 1L, null, 3L }, nullableLists["values"]);

            var objectListDocument = GetTestingDocument(sut, "typed-object-list-map");
            await objectListDocument.SetDataAsync(new Dictionary<object, object> {
                {
                    "values",
                    new object[] {
                        null,
                        "text",
                        5L,
                        new Dictionary<object, object> { { "name", "map" } },
                        new Dictionary<object, object> {
                            { "active", true },
                            { "nullable", null }
                        }
                    }
                },
                { "empty", Array.Empty<object>() }
            });
            var objectLists = (await objectListDocument.GetDocumentSnapshotAsync<Dictionary<string, IList<object>>>()).Data;
            Assert.Empty(objectLists["empty"]);

            var values = objectLists["values"];
            Assert.Null(values[0]);
            Assert.Equal("text", values[1]);
            Assert.Equal(5L, Convert.ToInt64(values[2]));
            Assert.Equal("map", Assert.IsAssignableFrom<IDictionary<string, object>>(values[3])["name"]);

            var nestedMap = Assert.IsAssignableFrom<IDictionary<string, object>>(values[4]);
            Assert.True((bool) nestedMap["active"]);
            Assert.Null(nestedMap["nullable"]);
        }

        [Fact]
        public async Task gets_document_data_as_typed_special_value_dictionaries()
        {
            var sut = CrossFirebaseFirestore.Current;
            var expectedDateTime = new DateTime(2026, 4, 29, 1, 2, 3, 456, DateTimeKind.Utc);
            var expectedOffset = new DateTimeOffset(2026, 4, 29, 4, 5, 6, 789, TimeSpan.Zero);
            var documentReference = GetTestingDocument(sut, "typed-reference-target");

            var dateTimeDocument = GetTestingDocument(sut, "typed-datetime-map");
            await dateTimeDocument.SetDataAsync(new Dictionary<object, object> {
                { "created", expectedDateTime }
            });
            var dateTimes = (await dateTimeDocument.GetDocumentSnapshotAsync<Dictionary<string, DateTime>>()).Data;
            Assert.InRange(
                Math.Abs(dateTimes["created"].Ticks - expectedDateTime.Ticks),
                0,
                TimeSpan.FromMilliseconds(1).Ticks);

            var dateTimeOffsetDocument = GetTestingDocument(sut, "typed-datetime-offset-map");
            await dateTimeOffsetDocument.SetDataAsync(new Dictionary<object, object> {
                { "observed", expectedOffset },
                { "generated", FieldValue.ServerTimestamp() }
            });
            var dateTimeOffsets = (await dateTimeOffsetDocument.GetDocumentSnapshotAsync<Dictionary<string, DateTimeOffset>>()).Data;
            Assert.InRange(
                Math.Abs(dateTimeOffsets["observed"].Ticks - expectedOffset.Ticks),
                0,
                TimeSpan.FromMilliseconds(1).Ticks);
            Assert.NotEqual(default, dateTimeOffsets["generated"]);

            var geoPointDocument = GetTestingDocument(sut, "typed-geopoint-map");
            await geoPointDocument.SetDataAsync(new Dictionary<object, object> {
                { "home", new GeoPoint(10.5, 20.25) },
                { "away", new GeoPoint(-33.875, 151.2) }
            });
            var geoPoints = (await geoPointDocument.GetDocumentSnapshotAsync<Dictionary<string, GeoPoint>>()).Data;
            Assert.Equal(10.5, geoPoints["home"].Latitude);
            Assert.Equal(20.25, geoPoints["home"].Longitude);
            Assert.Equal(-33.875, geoPoints["away"].Latitude);
            Assert.Equal(151.2, geoPoints["away"].Longitude);

            var referenceDocument = GetTestingDocument(sut, "typed-reference-map");
            await referenceDocument.SetDataAsync(new Dictionary<object, object> {
                { "original", documentReference }
            });
            var references = (await referenceDocument.GetDocumentSnapshotAsync<Dictionary<string, IDocumentReference>>()).Data;
            Assert.Equal(documentReference.Path, references["original"].Path);
        }

        [Fact]
        public async Task gets_dictionary_data_from_document_snapshot_listener()
        {
            var sut = CrossFirebaseFirestore.Current;
            var document = GetTestingDocument(sut, "raw-document-listener");
            var snapshotReceived = new TaskCompletionSource<IDictionary<string, object>>(TaskCreationOptions.RunContinuationsAsynchronously);

            await document.SetDataAsync(new Dictionary<object, object> { { "seed", "true" } });

            using var disposable = document.AddSnapshotListener<Dictionary<string, object>>(
                x => {
                    if(
                        x.Data?.TryGetValue("listener_value", out var value) == true
                        && Convert.ToInt64(value) == 5L
                    ) {
                        snapshotReceived.TrySetResult(x.Data);
                    }
                },
                e => snapshotReceived.TrySetException(e));

            await document.UpdateDataAsync(
                ("listener_value", 5L),
                ("nested.listener", "seen"));

            var data = await snapshotReceived.Task.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.Equal(5L, Convert.ToInt64(data["listener_value"]));

            var nested = Assert.IsAssignableFrom<IDictionary<string, object>>(data["nested"]);
            Assert.Equal("seen", nested["listener"]);
        }

        [Fact]
        public async Task gets_dictionary_data_from_query_snapshot_listener()
        {
            var sut = CrossFirebaseFirestore.Current;
            var collection = GetTestingCollection(sut);
            var document = collection.GetDocument("raw-query-listener");
            var snapshotReceived = new TaskCompletionSource<IDictionary<string, object>>(TaskCreationOptions.RunContinuationsAsynchronously);

            using var disposable = collection
                .WhereEqualsTo("listener_marker", "query")
                .AddSnapshotListener<Dictionary<string, object>>(
                    x => {
                        var data = x.Documents
                            .Select(y => y.Data)
                            .FirstOrDefault(y =>
                                y?.TryGetValue("query_listener_value", out var value) == true
                                && value is string text
                                && text == "ready");

                        if(data != null) {
                            snapshotReceived.TrySetResult(data);
                        }
                    },
                    e => snapshotReceived.TrySetException(e));

            await document.SetDataAsync(new Dictionary<object, object> {
                { "listener_marker", "query" },
                { "query_listener_value", "ready" }
            });

            var result = await snapshotReceived.Task.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.Equal("ready", result["query_listener_value"]);
        }

        [Fact]
        public async Task gets_null_or_empty_dictionary_data_for_missing_and_empty_documents()
        {
            var sut = CrossFirebaseFirestore.Current;
            var missingDocument = GetTestingDocument(sut, "missing-raw-data");
            var emptyDocument = GetTestingDocument(sut, "empty-raw-data");

            Assert.Null((await missingDocument.GetDocumentSnapshotAsync<Dictionary<string, object>>()).Data);
            Assert.Null((await missingDocument.GetDocumentSnapshotAsync<object>()).Data);

            await emptyDocument.SetDataAsync(new Dictionary<object, object> { { "temporary", "value" } });
            await emptyDocument.UpdateDataAsync(("temporary", FieldValue.Delete()));

            var dictionarySnapshot = await emptyDocument.GetDocumentSnapshotAsync<Dictionary<string, object>>();
            Assert.NotNull(dictionarySnapshot.Data);
            Assert.Empty(dictionarySnapshot.Data);

            var objectSnapshot = await emptyDocument.GetDocumentSnapshotAsync<object>();
            Assert.Empty(Assert.IsAssignableFrom<IDictionary<string, object>>(objectSnapshot.Data));
        }

        [Fact]
        public async Task writes_nested_dictionary_properties_on_ios()
        {
            if(!OperatingSystem.IsIOS()) {
                return;
            }

            var sut = CrossFirebaseFirestore.Current;
            var document = GetTestingDocument(sut, "ios-nested-dictionary");
            var expected = new Dictionary<string, Dictionary<string, short>> {
                {
                    "outer",
                    new Dictionary<string, short> {
                        { "inner", 7 }
                    }
                }
            };

            await document.SetDataAsync(new NestedShortDictionaryDocument(expected));

            var snapshot = await document.GetDocumentSnapshotAsync<NestedShortDictionaryDocument>();
            Assert.Equal((short) 7, snapshot.Data.Values["outer"]["inner"]);
        }

        [Fact]
        public async Task applies_ios_batch_tuple_set_options()
        {
            if(OperatingSystem.IsIOS() is false) {
                return;
            }

            var sut = CrossFirebaseFirestore.Current;
            var document = GetTestingDocument(sut, "ios-batch-tuple-set-options");
            await document.SetDataAsync(new Dictionary<object, object> {
                { "untouched", "keep" },
                { "selected", "old" }
            });

            var batch = sut.CreateBatch();
            batch.SetData(
                document,
                SetOptions.MergeFields("selected"),
                ("selected", "from-batch"),
                ("untouched", "should-not-change"));
            await batch.CommitAsync();

            var result = (await document.GetDocumentSnapshotAsync<BatchMergeFieldsDocument>()).Data;
            Assert.Equal("keep", result.Untouched);
            Assert.Equal("from-batch", result.Selected);
        }

        [Fact]
        public async Task writes_ios_dictionary_data_through_non_document_wrappers()
        {
            if(OperatingSystem.IsIOS() is false) {
                return;
            }

            var sut = CrossFirebaseFirestore.Current;
            var collection = GetTestingCollection(sut);

            var addedDocument = await collection.AddDocumentAsync(new Dictionary<object, object> {
                { "writer", "collection-add" },
                { "count", 1L }
            });
            var addedResult = (await addedDocument.GetDocumentSnapshotAsync<WriteWrapperDictionaryDocument>()).Data;
            Assert.Equal("collection-add", addedResult.Writer);
            Assert.Equal(1L, addedResult.Count);

            var batchSetDocument = collection.GetDocument("ios-batch-set-dictionary");
            var batchUpdateDocument = collection.GetDocument("ios-batch-update-dictionary");
            await batchUpdateDocument.SetDataAsync(new Dictionary<object, object> { { "writer", "seed" } });
            var batch = sut.CreateBatch();
            batch.SetData(batchSetDocument, new Dictionary<object, object> {
                { "writer", "batch-set" },
                { "count", 2L }
            });
            batch.UpdateData(batchUpdateDocument, new Dictionary<object, object> {
                { "writer", "batch-update" },
                { "count", 3L }
            });
            await batch.CommitAsync();

            var batchSetResult = (await batchSetDocument.GetDocumentSnapshotAsync<WriteWrapperDictionaryDocument>()).Data;
            Assert.Equal("batch-set", batchSetResult.Writer);
            Assert.Equal(2L, batchSetResult.Count);

            var batchUpdateResult = (await batchUpdateDocument.GetDocumentSnapshotAsync<WriteWrapperDictionaryDocument>()).Data;
            Assert.Equal("batch-update", batchUpdateResult.Writer);
            Assert.Equal(3L, batchUpdateResult.Count);

            var batchMergeDocument = collection.GetDocument("ios-batch-merge-dictionary");
            await batchMergeDocument.SetDataAsync(new Dictionary<object, object> {
                { "writer", "seed" },
                { "count", 30L },
                { "untouched", "kept-by-batch-merge" }
            });
            var mergeBatch = sut.CreateBatch();
            mergeBatch.SetData(
                batchMergeDocument,
                new Dictionary<object, object> { { "writer", "batch-merge" } },
                SetOptions.Merge()
            );
            await mergeBatch.CommitAsync();

            var batchMergeResult = (await batchMergeDocument.GetDocumentSnapshotAsync<WriteWrapperDictionaryDocument>()).Data;
            Assert.Equal("batch-merge", batchMergeResult.Writer);
            Assert.Equal(30L, batchMergeResult.Count);
            Assert.Equal("kept-by-batch-merge", batchMergeResult.Untouched);

            var transactionSetDocument = collection.GetDocument("ios-transaction-set-dictionary");
            var transactionUpdateDocument = collection.GetDocument("ios-transaction-update-dictionary");
            await transactionUpdateDocument.SetDataAsync(new Dictionary<object, object> { { "writer", "seed" } });
            await sut.RunTransactionAsync(transaction => {
                transaction.GetDocument<WriteWrapperDictionaryDocument>(transactionUpdateDocument);
                transaction.SetData(transactionSetDocument, new Dictionary<object, object> {
                    { "writer", "transaction-set" },
                    { "count", 4L }
                });
                transaction.UpdateData(transactionUpdateDocument, new Dictionary<object, object> {
                    { "writer", "transaction-update" },
                    { "count", 5L }
                });
                return true;
            });

            var transactionSetResult = (await transactionSetDocument.GetDocumentSnapshotAsync<WriteWrapperDictionaryDocument>()).Data;
            Assert.Equal("transaction-set", transactionSetResult.Writer);
            Assert.Equal(4L, transactionSetResult.Count);

            var transactionUpdateResult = (await transactionUpdateDocument.GetDocumentSnapshotAsync<WriteWrapperDictionaryDocument>()).Data;
            Assert.Equal("transaction-update", transactionUpdateResult.Writer);
            Assert.Equal(5L, transactionUpdateResult.Count);

            var transactionMergeDocument = collection.GetDocument("ios-transaction-merge-dictionary");
            await transactionMergeDocument.SetDataAsync(new Dictionary<object, object> {
                { "writer", "seed" },
                { "count", 50L },
                { "untouched", "kept-by-transaction-merge" }
            });
            await sut.RunTransactionAsync(transaction => {
                transaction.GetDocument<WriteWrapperDictionaryDocument>(transactionMergeDocument);
                transaction.SetData(
                    transactionMergeDocument,
                    new Dictionary<object, object> { { "writer", "transaction-merge" } },
                    SetOptions.Merge()
                );
                return true;
            });

            var transactionMergeResult = (await transactionMergeDocument.GetDocumentSnapshotAsync<WriteWrapperDictionaryDocument>()).Data;
            Assert.Equal("transaction-merge", transactionMergeResult.Writer);
            Assert.Equal(50L, transactionMergeResult.Count);
            Assert.Equal("kept-by-transaction-merge", transactionMergeResult.Untouched);
        }

        [Fact]
        public async Task exposes_parent_relationships()
        {
            var sut = CrossFirebaseFirestore.Current;
            var parentDocument = GetTestingDocument(sut, "parent");
            var subCollection = parentDocument.GetCollection("sub_items");
            var childDocument = subCollection.GetDocument("child");

            await parentDocument.SetDataAsync(new SimpleItem("parent"));
            await childDocument.SetDataAsync(new SimpleItem("child"));

            Assert.Equal(parentDocument.Path, subCollection.Parent.Path);
            Assert.Equal(parentDocument.Path, childDocument.Parent.Parent.Path);
            Assert.Equal(childDocument.Path, childDocument.Parent.GetDocument(childDocument.Id).Path);
        }

        [Fact]
        public async Task queries_collection_group()
        {
            var sut = CrossFirebaseFirestore.Current;
            var marker = Guid.NewGuid().ToString("N");
            var firstDocument = GetTestingDocument(sut, "group-parent-1")
                .GetCollection("sub_items")
                .GetDocument("first");
            var secondDocument = GetTestingDocument(sut, "group-parent-2")
                .GetCollection("sub_items")
                .GetDocument("second");

            await firstDocument.SetDataAsync(new SimpleItem($"{marker}-one"));
            await secondDocument.SetDataAsync(new SimpleItem($"{marker}-two"));

            var snapshot = await sut
                .GetCollectionGroup("sub_items")
                .GetDocumentsAsync<SimpleItem>();

            var matchingTitles = snapshot.Documents
                .Select(x => x.Data.Title)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Where(x => x.StartsWith(marker, StringComparison.Ordinal))
                .OrderBy(x => x, StringComparer.Ordinal)
                .ToArray();

            Assert.Equal(new[] { $"{marker}-one", $"{marker}-two" }, matchingTitles);
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
        private sealed class DictionaryObjectValuesDocument : IFirestoreObject
        {
            public DictionaryObjectValuesDocument()
            {
                // needed for firestore
            }

            public DictionaryObjectValuesDocument(Dictionary<string, object> values)
            {
                Values = values;
            }

            [FirestoreProperty("values")]
            public Dictionary<string, object> Values { get; private set; }
        }

        [Preserve(AllMembers = true)]
        private sealed class EnumDictionaryDocument : IFirestoreObject
        {
            public EnumDictionaryDocument()
            {
                // needed for firestore
            }

            public EnumDictionaryDocument(Dictionary<string, PokeType> values)
            {
                Values = values;
            }

            [FirestoreProperty("values")]
            public Dictionary<string, PokeType> Values { get; private set; }
        }

        [Preserve(AllMembers = true)]
        private sealed class AndroidNumericCollectionsDocument : IFirestoreObject
        {
            public AndroidNumericCollectionsDocument()
            {
                // needed for firestore
            }

            [FirestoreProperty("counts")]
            public Dictionary<string, int> Counts { get; private set; }

            [FirestoreProperty("nullable_counts")]
            public IList<int?> NullableCounts { get; private set; }

            [FirestoreProperty("types")]
            public Dictionary<string, PokeType> Types { get; private set; }
        }

        [Preserve(AllMembers = true)]
        private sealed class SetDataPayloadDocument : IFirestoreObject
        {
            public SetDataPayloadDocument()
            {
                // needed for firestore
            }

            [FirestoreProperty("field_a")]
            public string FieldA { get; private set; }

            [FirestoreProperty("field_b")]
            public string FieldB { get; private set; }
        }

        [Preserve(AllMembers = true)]
        private sealed class NestedShortDictionaryDocument : IFirestoreObject
        {
            public NestedShortDictionaryDocument()
            {
                // needed for firestore
            }

            public NestedShortDictionaryDocument(Dictionary<string, Dictionary<string, short>> values)
            {
                Values = values;
            }

            [FirestoreProperty("values")]
            public Dictionary<string, Dictionary<string, short>> Values { get; private set; }
        }

        [Preserve(AllMembers = true)]
        private sealed class BatchMergeFieldsDocument : IFirestoreObject
        {
            public BatchMergeFieldsDocument()
            {
                // needed for firestore
            }

            [FirestoreProperty("untouched")]
            public string Untouched { get; private set; }

            [FirestoreProperty("selected")]
            public string Selected { get; private set; }
        }

        [Preserve(AllMembers = true)]
        private sealed class WriteWrapperDictionaryDocument : IFirestoreObject
        {
            public WriteWrapperDictionaryDocument()
            {
                // needed for firestore
            }

            [FirestoreProperty("writer")]
            public string Writer { get; private set; }

            [FirestoreProperty("count")]
            public long Count { get; private set; }

            [FirestoreProperty("untouched")]
            public string Untouched { get; private set; }
        }

        private static void AssertRawDictionaryData(IDictionary<string, object> data, IDocumentReference document)
        {
            Assert.NotNull(data);
            Assert.Equal("value", data["unknown_string"]);
            Assert.Equal(123L, Convert.ToInt64(data["unknown_long"]));
            Assert.Equal(12.5, Convert.ToDouble(data["unknown_double"]));
            Assert.True((bool) data["unknown_bool"]);
            Assert.Null(data["unknown_null"]);

            var numbers = Assert.IsAssignableFrom<IList<object>>(data["unknown_numbers"]);
            Assert.Equal(new[] { 1L, 2L }, numbers.Select(Convert.ToInt64));

            Assert.Empty(Assert.IsAssignableFrom<IList<object>>(data["unknown_empty_array"]));
            Assert.Empty(Assert.IsAssignableFrom<IDictionary<string, object>>(data["unknown_empty_map"]));

            var arrayWithNulls = Assert.IsAssignableFrom<IList<object>>(data["unknown_array_with_nulls"]);
            Assert.Null(arrayWithNulls[0]);
            Assert.Equal("text", arrayWithNulls[1]);
            Assert.Equal(3L, Convert.ToInt64(arrayWithNulls[2]));

            var childMap = Assert.IsAssignableFrom<IDictionary<string, object>>(arrayWithNulls[3]);
            Assert.Null(childMap["child_null"]);
            Assert.Equal("child", childMap["child_text"]);
            Assert.False((bool) arrayWithNulls[4]);

            var mapArray = Assert.IsAssignableFrom<IList<object>>(data["unknown_map_array"]);
            Assert.Equal(2, mapArray.Count);
            var firstMap = Assert.IsAssignableFrom<IDictionary<string, object>>(mapArray[0]);
            Assert.Equal("first", firstMap["name"]);
            Assert.Equal(1L, Convert.ToInt64(firstMap["score"]));
            Assert.True((bool) firstMap["active"]);
            var secondMap = Assert.IsAssignableFrom<IDictionary<string, object>>(mapArray[1]);
            Assert.Equal("second", secondMap["name"]);
            Assert.Equal(2L, Convert.ToInt64(secondMap["score"]));
            Assert.False((bool) secondMap["active"]);

            var nested = Assert.IsAssignableFrom<IDictionary<string, object>>(data["nested"]);
            Assert.Equal(42L, Convert.ToInt64(nested["answer"]));
            Assert.Equal("nested value", nested["label"]);
            Assert.Null(nested["null_value"]);
            Assert.Empty(Assert.IsAssignableFrom<IList<object>>(nested["empty_values"]));
            Assert.Empty(Assert.IsAssignableFrom<IDictionary<string, object>>(nested["empty_map"]));

            var nestedValues = Assert.IsAssignableFrom<IList<object>>(nested["values"]);
            Assert.Equal(new[] { "one", "two" }, nestedValues.Select(x => x as string));

            var deepNested = Assert.IsAssignableFrom<IDictionary<string, object>>(nested["deep"]);
            Assert.Equal(84L, Convert.ToInt64(deepNested["answer"]));
            Assert.Null(deepNested["null_value"]);

            var directMap = Assert.IsAssignableFrom<IDictionary<string, object>>(nested["direct_map"]);
            Assert.Equal("direct", directMap["text"]);
            Assert.Equal(9L, Convert.ToInt64(directMap["count"]));
            Assert.Equal((short) 7, Convert.ToInt16(directMap["short_count"]));
            var flags = Assert.IsAssignableFrom<IList<object>>(directMap["flags"]);
            Assert.Equal(new[] { true, false }, flags.Select(x => (bool) x));
            var innerMap = Assert.IsAssignableFrom<IDictionary<string, object>>(directMap["inner"]);
            Assert.Equal("inside", innerMap["value"]);

            Assert.IsType<DateTimeOffset>(data["observed_at"]);
            Assert.IsType<DateTimeOffset>(data["created_at"]);
            Assert.IsType<DateTimeOffset>(data["generated_at"]);
            var location = Assert.IsType<GeoPoint>(data["location"]);
            Assert.Equal(1.25, location.Latitude);
            Assert.Equal(2.5, location.Longitude);

            var reference = Assert.IsAssignableFrom<IDocumentReference>(data["original_reference"]);
            Assert.Equal(document.Path, reference.Path);
        }

        private static void AssertRawObjectDictionaryData(IDictionary<object, object> data, IDocumentReference document)
        {
            Assert.NotNull(data);
            Assert.All(data.Keys, key => Assert.IsType<string>(key));
            AssertRawDictionaryData(data.ToDictionary(x => (string) x.Key, x => x.Value), document);
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
