using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore
{
    public sealed partial class FirestoreFixture
    {
        [Fact]
        public async Task exposes_parent_relationships()
        {
            var sut = CrossFirebaseFirestore.Current;
            var parentDocument = GetTestingDocument(sut, "parent");
            var subCollection = parentDocument.GetCollection("sub_items");
            var childDocument = subCollection.GetDocument("child");

            await parentDocument.SetDataAsync(new SimpleItem("parent"));
            await childDocument.SetDataAsync(new SimpleItem("child"));

            Assert.Equal(parentDocument.Path, subCollection.Parent!.Path);
            Assert.Equal(parentDocument.Path, childDocument.Parent!.Parent!.Path);
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
                .Select(x => x.Data!.Title)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Where(x => x.StartsWith(marker, StringComparison.Ordinal))
                .OrderBy(x => x, StringComparer.Ordinal)
                .ToArray();

            Assert.Equal(new[] { $"{marker}-one", $"{marker}-two" }, matchingTitles);
        }
    }
}