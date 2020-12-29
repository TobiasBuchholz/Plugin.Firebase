using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Runtime;
using Plugin.Firebase.Firestore;
using Xamarin.Essentials;
using Xunit;

namespace Plugin.Firebase.IntegrationTests.Firestore
{
    [Collection("Sequential")]
    [Preserve(AllMembers = true)]
    public sealed class FirestoreFixture : IAsyncLifetime
    {
        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }
        
        [Fact]
        public async Task adds_document_to_collection()
        {
            var sut = CrossFirebaseFirestore.Current;
            var pokemon = PokemonFactory.CreateBulbasur();
            var path = $"testing/{pokemon.Id}";
            var document = sut.GetDocument(path);
            
            await document.SetDataAsync(pokemon);

            var snapshot = await document.GetDocumentSnapshotAsync<Pokemon>();
            Assert.False(snapshot.Metadata.HasPendingWrites);
            Assert.Equal(pokemon.Id, snapshot.Reference.Id);
            Assert.Equal(path, snapshot.Reference.Path);
            Assert.Equal(pokemon, snapshot.Data);
        }

        [Fact]
        public async Task updates_existing_document()
        {
            var sut = CrossFirebaseFirestore.Current;
            var pokemon = PokemonFactory.CreateSquirtle();
            var path = $"testing/{pokemon.Id}";
            var document = sut.GetDocument(path);

            await document.SetDataAsync(pokemon);
            Assert.Equal(pokemon, (await document.GetDocumentSnapshotAsync<Pokemon>()).Data);

            var update = new Dictionary<object, object> {
                { "name", "Cool Squirtle" },
                { "moves", FieldValue.ArrayUnion("Bubble-Blast") },
                { "first_sighting_location.latitude", 13.37 }
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
            var documentBulbasur = sut.GetDocument("testing/1");
            var documentCharmander = sut.GetDocument("testing/4");
            var documentSquirtle = sut.GetDocument("testing/7");
            await documentBulbasur.SetDataAsync(bulbasur);
            await documentCharmander.SetDataAsync(charmander);
            
            var charmanderSightingCount = await sut.RunTransactionAsync(transaction => {
                var snapshotCharmander = transaction.GetDocument<Pokemon>(documentCharmander);
                var newSightingCount = snapshotCharmander.Data.SightingCount + 1;
                transaction.SetData(documentSquirtle, squirtle);
                transaction.UpdateData(documentCharmander, ("sighting_count", newSightingCount));
                transaction.DeleteDocument(documentBulbasur);
                return newSightingCount;
            });
            
            Assert.Equal(squirtle, (await documentSquirtle.GetDocumentSnapshotAsync<Pokemon>()).Data);
            Assert.Equal(charmander.SightingCount + 1, charmanderSightingCount);
            Assert.Null((await documentBulbasur.GetDocumentSnapshotAsync<Pokemon>()).Data);
        }

        [Fact]
        public async Task writes_data_as_batch()
        {
            var sut = CrossFirebaseFirestore.Current;
            var bulbasur = PokemonFactory.CreateBulbasur();
            var charmander = PokemonFactory.CreateCharmander();
            var squirtle = PokemonFactory.CreateSquirtle();
            var documentBulbasur = sut.GetDocument("testing/1");
            var documentCharmander = sut.GetDocument("testing/4");
            var documentSquirtle = sut.GetDocument("testing/7");
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
            
            Assert.Equal(new[] { "Ivysaur", "Squirtle", "Venusaur", "Wartortle"  }, pokemons.Documents.Select(x => x.Data.Name));
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
            var document = sut.GetDocument("testing/1");
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
        public async Task gets_real_time_updates_on_multiple_documents()
        {
            var sut = CrossFirebaseFirestore.Current;
            var collection = sut.GetCollection("testing");

            var changes = new List<IEnumerable<DocumentChangeType>>();
            var disposable = collection
                .WhereEqualsTo("poke_type", PokeType.Fire)
                .AddSnapshotListener<Pokemon>(x => {
                    changes.Add(x.DocumentChanges.Select(y => y.ChangeType));
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

            var expectedChangesOnAndroid = new[] {
                DocumentChangeType.Added,
                DocumentChangeType.Added,
                DocumentChangeType.Added,
                DocumentChangeType.Modified,
                DocumentChangeType.Removed
            };
            
            var expectedChangesOniOS = new[] {
                DocumentChangeType.Added,
                DocumentChangeType.Modified,
                DocumentChangeType.Added,
                DocumentChangeType.Modified,
                DocumentChangeType.Added,
                DocumentChangeType.Modified,
                DocumentChangeType.Modified,
                DocumentChangeType.Removed
            };
            
            Assert.Equal(DeviceInfo.Platform == DevicePlatform.Android ? expectedChangesOnAndroid : expectedChangesOniOS, changes.SelectMany(x => x));
            disposable.Dispose();
        }
        
        [Fact]
        public async Task deletes_document()
        {
            var sut = CrossFirebaseFirestore.Current;
            var pokemon = PokemonFactory.CreateCharmander();
            var path = $"testing/{pokemon.Id}";
            var document = sut.GetDocument(path);
            
            await document.SetDataAsync(pokemon);
            Assert.NotNull((await sut.GetDocument(path).GetDocumentSnapshotAsync<Pokemon>()).Data);
            
            await document.DeleteDocumentAsync();
            Assert.Null((await sut.GetDocument(path).GetDocumentSnapshotAsync<Pokemon>()).Data);
        }

        [Fact]
        public async Task deletes_fields_of_document()
        {
            var sut = CrossFirebaseFirestore.Current;
            var pokemon = PokemonFactory.CreateCharmander();
            var path = $"testing/{pokemon.Id}";
            var document = sut.GetDocument(path);
            await document.SetDataAsync(pokemon);

            await document.UpdateDataAsync(
                ("moves", FieldValue.Delete()),
                ("evolutions", FieldValue.Delete()),
                ("first_sighting_location", FieldValue.Delete()),
                ("poke_type", FieldValue.Delete()));

            var snapshot = await document.GetDocumentSnapshotAsync<Pokemon>();
            Assert.Null(snapshot.Data.Moves);
            Assert.Null(snapshot.Data.FirstSightingLocation);
            Assert.Null(snapshot.Data.Evolutions);
            Assert.Equal(PokeType.Undefined, snapshot.Data.PokeType);
        }

        [Fact]
        public void gets_firestore_settings()
        {
            var settings = CrossFirebaseFirestore.Current.Settings;
            Assert.Equal("firestore.googleapis.com", settings.Host);
            Assert.True(settings.IsPersistenceEnabled);
            Assert.True(settings.IsSslEnabled);
            Assert.Equal(104857600, settings.CacheSizeBytes);
        }

        public async Task DisposeAsync()
        {
            await CrossFirebaseFirestore.Current.DeleteCollectionAsync<Pokemon>("testing", batchSize:10);
        }
    }
}