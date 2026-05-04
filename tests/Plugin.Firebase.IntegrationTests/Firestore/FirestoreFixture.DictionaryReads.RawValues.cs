using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore
{
    public sealed partial class FirestoreFixture
    {
        [Fact]
        public async Task gets_document_data_as_dictionary()
        {
            var sut = CrossFirebaseFirestore.Current;
            var document = GetTestingDocument(sut, "raw-data");
            var observedAt = DateTimeOffset.Now;

            await document.SetDataAsync(new Dictionary<object, object?> { { "seed", "true" } });
            await document.UpdateDataAsync(
                ("unknown_string", "value"),
                ("unknown_long", 123L),
                ("unknown_double", 12.5),
                ("unknown_bool", true),
                ("unknown_null", null),
                ("unknown_numbers", new[] { 1L, 2L }),
                ("unknown_empty_array", Array.Empty<object?>()),
                ("unknown_empty_map", new Dictionary<object, object?>()),
                ("unknown_array_with_nulls", new object?[] {
                    null,
                    "text",
                    3L,
                    new Dictionary<object, object?> {
                        { "child_null", null },
                        { "child_text", "child" }
                    },
                    false
                }),
                ("unknown_map_array", new[] {
                    new Dictionary<object, object?> {
                        { "name", "first" },
                        { "score", 1L },
                        { "active", true }
                    },
                    new Dictionary<object, object?> {
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
                ("nested.empty_values", Array.Empty<object?>()),
                ("nested.empty_map", new Dictionary<object, object?>()),
                ("nested.direct_map", new Dictionary<object, object?> {
                    { "text", "direct" },
                    { "count", 9L },
                    { "short_count", 7L },
                    { "flags", new[] { true, false } },
                    { "inner", new Dictionary<object, object?> { { "value", "inside" } } }
                }),
                ("observed_at", observedAt),
                ("created_at", observedAt.UtcDateTime),
                ("generated_at", FieldValue.ServerTimestamp()),
                ("original_reference", document));

            var dictionarySnapshot = await document.GetDocumentSnapshotAsync<Dictionary<string, object?>>();
            FirestoreAssertions.RawDictionaryData(dictionarySnapshot.Data!, document);

            var interfaceSnapshot = await document.GetDocumentSnapshotAsync<IDictionary<string, object?>>();
            FirestoreAssertions.RawDictionaryData(interfaceSnapshot.Data!, document);

            var objectDictionarySnapshot = await document.GetDocumentSnapshotAsync<Dictionary<object, object?>>();
            FirestoreAssertions.RawObjectDictionaryData(objectDictionarySnapshot.Data!, document);

            var objectSnapshot = await document.GetDocumentSnapshotAsync<object>();
            FirestoreAssertions.RawDictionaryData(
                Assert.IsAssignableFrom<IDictionary<string, object?>>(objectSnapshot.Data!),
                document);

            var querySnapshot = await GetTestingCollection(sut)
                .WhereEqualsTo("unknown_string", "value")
                .GetDocumentsAsync<Dictionary<string, object?>>();
            FirestoreAssertions.RawDictionaryData(Assert.Single(querySnapshot.Documents).Data!, document);
        }

    }
}