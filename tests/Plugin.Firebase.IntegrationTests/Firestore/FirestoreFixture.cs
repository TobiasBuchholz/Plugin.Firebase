using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plugin.Firebase.Firestore;
using Xunit;

namespace Plugin.Firebase.IntegrationTests.Firestore
{
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

        public async Task DisposeAsync()
        {
            await CrossFirebaseFirestore.Current.DeleteCollectionAsync<Pokemon>("testing", batchSize:10);
        }
    }
}