using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plugin.Firebase.Firestore;
using Xunit;

namespace Plugin.Firebase.IntegrationTests.Firestore
{
    public sealed class FirestoreFixture
    {
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
            var document = sut.GetDocument("pokemons/1");
            var pokemon = await document.GetDocumentSnapshotAsync<Pokemon>();
            
            var result = await sut.RunTransactionAsync(transaction => {
                var snapshot = transaction.GetDocument<Pokemon>(document);
                var newSightCount = snapshot.Data.SightingCount + 1;
                transaction.UpdateData(document, ("sight_count", newSightCount));
                return newSightCount;
            });
            
            Assert.Equal(pokemon.Data.SightingCount + 1, result);
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
    }
}