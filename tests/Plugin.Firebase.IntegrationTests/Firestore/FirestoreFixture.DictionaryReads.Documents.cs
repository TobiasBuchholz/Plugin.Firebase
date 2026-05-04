using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore
{
    public sealed partial class FirestoreFixture
    {
        [Fact]
        public async Task deletes_document()
        {
            var sut = CrossFirebaseFirestore.Current;
            var pokemon = PokemonFactory.CreateCharmander();
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
            var document = GetTestingDocument(sut, pokemon.Id);
            await document.SetDataAsync(pokemon);

            await document.UpdateDataAsync(
                (Pokemon.MovesField, FieldValue.Delete()),
                (Pokemon.ItemsField, FieldValue.Delete()),
                (Pokemon.FirstSightingLocationField, FieldValue.Delete()),
                (Pokemon.PokeTypeField, FieldValue.Delete()));

            var snapshot = await document.GetDocumentSnapshotAsync<Pokemon>();
            Assert.Null(snapshot.Data!.Moves);
            Assert.Null(snapshot.Data!.FirstSightingLocation);
            Assert.Null(snapshot.Data!.Items);
            Assert.Equal(PokeType.Undefined, snapshot.Data!.PokeType);
        }


        [Fact]
        public async Task copies_document_id_in_firestore_document_id_attributed_property()
        {
            var sut = CrossFirebaseFirestore.Current;
            var item = new SimpleItem(title: "test");
            var document = GetTestingDocument(sut, "1337");

            await document.SetDataAsync(item);

            var snapshot = await document.GetDocumentSnapshotAsync<SimpleItem>();
            Assert.Equal("1337", snapshot.Data!.Id);
            Assert.Equal("1337", snapshot.Reference.Id);
        }


        [Fact]
        public async Task clones_pokemon_with_original_reference()
        {
            var sut = CrossFirebaseFirestore.Current;
            var bulbasurReference = sut.GetDocument($"pokemons/1");
            var bulbasur = (await bulbasurReference.GetDocumentSnapshotAsync<Pokemon>()).Data!;
            var copy = bulbasur.Clone(bulbasurReference);
            var copyPath = TestingDocumentPath(copy.Id);
            var copyDocument = GetTestingDocument(sut, copy.Id);
            await copyDocument.SetDataAsync(copy);

            var copySnapshot = await copyDocument.GetDocumentSnapshotAsync<Pokemon>();
            Assert.False(copySnapshot.Metadata.HasPendingWrites);
            Assert.Equal($"{bulbasur.Id}_copied", copySnapshot.Reference.Id);
            Assert.Equal(copyPath, copySnapshot.Reference.Path);
            Assert.Equal(copy, copySnapshot.Data!);
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
            await subDocument.SetDataAsync(new Dictionary<object, object?>() { { "foo", "bar" } });

            var subCollectionRef1 = sut.GetCollection(subCollectionPath);
            var subCollectionRef2 = document.GetCollection(subCollectionName);
            var snapshot1 = await subCollectionRef1.GetDocumentsAsync<object>();
            var snapshot2 = await subCollectionRef2.GetDocumentsAsync<object>();
            Assert.Single(snapshot1.Documents);
            Assert.Single(snapshot2.Documents);
        }

    }
}