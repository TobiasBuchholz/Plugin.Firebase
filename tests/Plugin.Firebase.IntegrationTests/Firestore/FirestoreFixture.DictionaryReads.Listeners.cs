using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore
{
    public sealed partial class FirestoreFixture
    {
        [Fact]
        public async Task gets_dictionary_data_from_document_snapshot_listener()
        {
            var sut = CrossFirebaseFirestore.Current;
            var document = GetTestingDocument(sut, "raw-document-listener");
            var snapshotReceived = new CallbackProbe<IDictionary<string, object?>>();

            await document.SetDataAsync(new Dictionary<object, object?> { { "seed", "true" } });

            using var disposable = document.AddSnapshotListener<Dictionary<string, object?>>(
                x => {
                    if(
                        x.Data?.TryGetValue("listener_value", out var value) == true
                        && Convert.ToInt64(value) == 5L
                    ) {
                        snapshotReceived.TrySetResult(x.Data!);
                    }
                },
                e => snapshotReceived.TrySetException(e));

            await document.UpdateDataAsync(
                ("listener_value", 5L),
                ("nested.listener", "seen"));

            var data = await snapshotReceived.WaitAsync(
                IntegrationTestTimeouts.Callback,
                "Firestore tuple listener snapshot");
            Assert.Equal(5L, Convert.ToInt64(data["listener_value"]));

            var nested = Assert.IsAssignableFrom<IDictionary<string, object?>>(data["nested"]);
            Assert.Equal("seen", nested["listener"]);
        }


        [Fact]
        public async Task gets_dictionary_data_from_query_snapshot_listener()
        {
            var sut = CrossFirebaseFirestore.Current;
            var collection = GetTestingCollection(sut);
            var document = collection.GetDocument("raw-query-listener");
            var snapshotReceived = new CallbackProbe<IDictionary<string, object?>>();

            using var disposable = collection
                .WhereEqualsTo("listener_marker", "query")
                .AddSnapshotListener<Dictionary<string, object?>>(
                    x => {
                        var data = x.Documents
                            .Select(y => y.Data!)
                            .FirstOrDefault(y =>
                                y?.TryGetValue("query_listener_value", out var value) == true
                                && value is string text
                                && text == "ready");

                        if(data != null) {
                            snapshotReceived.TrySetResult(data);
                        }
                    },
                    e => snapshotReceived.TrySetException(e));

            await document.SetDataAsync(new Dictionary<object, object?> {
                { "listener_marker", "query" },
                { "query_listener_value", "ready" }
            });

            var result = await snapshotReceived.WaitAsync(
                IntegrationTestTimeouts.Callback,
                "Firestore dictionary listener snapshot");
            Assert.Equal("ready", result["query_listener_value"]);
        }

    }
}