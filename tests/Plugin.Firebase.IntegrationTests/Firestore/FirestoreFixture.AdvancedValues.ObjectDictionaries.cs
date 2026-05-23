using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore
{
    public sealed partial class FirestoreFixture
    {
        [Fact]
        public async Task gets_dictionary_properties_inside_firestore_objects()
        {
            var sut = CrossFirebaseFirestore.Current;
            var document = GetTestingDocument(sut, "dictionary-container");
            var container = DictionaryContainerFactory.CreateDefault();

            await document.SetDataAsync(container);

            var result = (await document.GetDocumentSnapshotAsync<DictionaryContainer>()).Data!;
            Assert.Equal("dictionary-container", result.Id);
            Assert.Equal("container", result.Metadata["title"]);
            Assert.Equal(5L, Convert.ToInt64(result.Metadata["count"]));
            Assert.Null(result.Metadata["nullable"]);
            Assert.Equal("nested", Assert.IsAssignableFrom<IDictionary<string, object?>>(result.Metadata["details"])["label"]);
            Assert.Equal(10L, result.Scores["first"]);
            Assert.Equal(20L, result.Scores["second"]);
            Assert.True(result.Flags["active"]);
            Assert.False(result.Flags["archived"]);
            Assert.Equal(["first", null, 3L], result.MixedLists["values"]);
            Assert.Empty(result.MixedLists["empty"]);
            Assert.Equal("outer", result.Nested["outer"]["name"]);
            Assert.Equal(2L, Convert.ToInt64(result.Nested["outer"]["count"]));
        }
    }
}