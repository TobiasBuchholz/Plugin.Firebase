#nullable enable

using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore
{
    [Collection("Sequential")]
    [TestLogging]
    [Preserve(AllMembers = true)]
    public sealed class FirestoreNullabilityFixture : IAsyncLifetime
    {
        private readonly string _testingCollectionPath = $"nullability_testing_{Guid.NewGuid():N}";

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public async Task writes_and_reads_null_values_from_supported_payload_shapes()
        {
            var sut = CrossFirebaseFirestore.Current;
            var collection = GetTestingCollection(sut);

            var objectDocument = GetTestingDocument(sut, "object");
            await objectDocument.SetDataAsync(CreateNullableItem("object"));

            var dictionaryDocument = GetTestingDocument(sut, "dictionary");
            await dictionaryDocument.SetDataAsync(CreateNullableDictionary("dictionary"));

            var tupleDocument = GetTestingDocument(sut, "tuple");
            await tupleDocument.SetDataAsync(
                ("nullable_string", null),
                ("nullable_number", null),
                ("nullable_map", CreateNestedMap()),
                ("nullable_list", CreateNullableList()),
                ("query_marker", "tuple")
            );

            var addedDocument = await collection.AddDocumentAsync(CreateNullableItem("added"));

            await AssertNullableDocumentAsync(objectDocument, "object");
            await AssertNullableDocumentAsync(dictionaryDocument, "dictionary");
            await AssertNullableDocumentAsync(tupleDocument, "tuple");
            await AssertNullableDocumentAsync(addedDocument, "added");
        }

        [Fact]
        public async Task updates_null_values_from_document_batch_and_transaction_writes()
        {
            var sut = CrossFirebaseFirestore.Current;

            var documentUpdate = GetTestingDocument(sut, "document-update");
            await documentUpdate.SetDataAsync(CreateNonNullItem("document-update-seed"));
            await documentUpdate.UpdateDataAsync(
                ("nullable_string", null),
                ("nullable_number", null),
                ("nullable_map.inner_null", null),
                ("nullable_map.inner_value", "nested-value"),
                ("nullable_list", CreateNullableList()),
                ("query_marker", "document-update")
            );

            var batchSet = GetTestingDocument(sut, "batch-set");
            var batchUpdate = GetTestingDocument(sut, "batch-update");
            await batchUpdate.SetDataAsync(CreateNonNullItem("batch-update-seed"));

            var batch = sut.CreateBatch();
            batch.SetData(batchSet, CreateNullableDictionary("batch-set"));
            batch.UpdateData(
                batchUpdate,
                new Dictionary<object, object?> {
                    { "nullable_string", null },
                    { "nullable_number", null },
                    { "nullable_map.inner_null", null },
                    { "nullable_map.inner_value", "nested-value" },
                    { "nullable_list", CreateNullableList() },
                    { "query_marker", "batch-update" }
                }
            );
            await batch.CommitAsync();

            var transactionDocument = GetTestingDocument(sut, "transaction");
            await transactionDocument.SetDataAsync(CreateNonNullItem("transaction-seed"));
            var transactionResult = await sut.RunTransactionAsync<string?>(transaction => {
                transaction.UpdateData(
                    transactionDocument,
                    ("nullable_string", null),
                    ("nullable_number", null),
                    ("nullable_map.inner_null", null),
                    ("nullable_map.inner_value", "nested-value"),
                    ("nullable_list", CreateNullableList()),
                    ("query_marker", null)
                );
                return null;
            });

            Assert.Null(transactionResult);
            await AssertNullableDocumentAsync(documentUpdate, "document-update");
            await AssertNullableDocumentAsync(batchSet, "batch-set");
            await AssertNullableDocumentAsync(batchUpdate, "batch-update");
            await AssertNullableDocumentAsync(transactionDocument, null);
        }

        [Fact]
        public async Task queries_documents_by_null_field_values()
        {
            var sut = CrossFirebaseFirestore.Current;
            var collection = GetTestingCollection(sut);

            await GetTestingDocument(sut, "null-a").SetDataAsync(CreateNullableItem(null));
            await GetTestingDocument(sut, "null-b").SetDataAsync(CreateNullableItem(null));
            await GetTestingDocument(sut, "value").SetDataAsync(CreateNullableItem("value"));

            var stringFieldSnapshot = await collection
                .WhereEqualsTo("query_marker", null)
                .GetDocumentsAsync<NullableFirestoreItem>();
            var fieldPathSnapshot = await collection
                .WhereEqualsTo(FieldPath.Of(["query_marker"]), null)
                .GetDocumentsAsync<NullableFirestoreItem>();

            Assert.Equal(
                ["null-a", "null-b"],
                stringFieldSnapshot.Documents.Select(x => Require(x.Data).Id).OrderBy(x => x)
            );
            Assert.Equal(
                ["null-a", "null-b"],
                fieldPathSnapshot.Documents.Select(x => Require(x.Data).Id).OrderBy(x => x)
            );
        }

        [Fact]
        public async Task applies_null_array_transforms()
        {
            var sut = CrossFirebaseFirestore.Current;
            var document = GetTestingDocument(sut, "array-transforms");
            await document.SetDataAsync(new Dictionary<object, object?> {
                { "nullable_list", new List<object?> { "existing" } },
                { "query_marker", "array-transforms" }
            });

            await document.UpdateDataAsync(("nullable_list", FieldValue.ArrayUnion(null, "added")));

            var afterUnion = Require(
                (await document.GetDocumentSnapshotAsync<NullableFirestoreItem>()).Data?.NullableList
            );
            Assert.Equal(["existing", null, "added"], afterUnion);

            await document.UpdateDataAsync(("nullable_list", FieldValue.ArrayRemove(new object?[] { null })));

            var afterRemove = Require(
                (await document.GetDocumentSnapshotAsync<NullableFirestoreItem>()).Data?.NullableList
            );
            Assert.Equal(["existing", "added"], afterRemove);
        }

        [Fact]
        public async Task rejects_required_api_arguments_when_null()
        {
            var sut = CrossFirebaseFirestore.Current;
            var collection = GetTestingCollection(sut);
            var document = GetTestingDocument(sut, "required-null-rejection");
            await document.SetDataAsync(CreateNonNullItem("required-null-rejection"));

            AssertRejects(() => sut.GetDocument(RequiredNull<string>()));
            AssertRejects(() => sut.GetCollection(RequiredNull<string>()));
            AssertRejects(() => collection.GetDocument(RequiredNull<string>()));
            AssertRejects(() => document.GetCollection(RequiredNull<string>()));
            AssertRejects(() => collection.WhereEqualsTo(RequiredNull<string>(), "value"));
            AssertRejects(() => collection.OrderBy(RequiredNull<string>()));
            AssertRejects(() => collection.WhereFieldIn("query_marker", RequiredNull<object?[]>()));
            AssertRejects(() => collection.StartingAt(RequiredNull<object?[]>()));
            AssertRejects(
                () => document.AddSnapshotListener<NullableFirestoreItem>(
                    RequiredNull<Action<IDocumentSnapshot<NullableFirestoreItem>>>()
                )
            );
            AssertRejects(
                () => collection.AddSnapshotListener<NullableFirestoreItem>(
                    RequiredNull<Action<IQuerySnapshot<NullableFirestoreItem>>>()
                )
            );

            await AssertRejectsAsync(() => collection.AddDocumentAsync(RequiredNull<NullableFirestoreItem>()));
            await AssertRejectsAsync(() => document.SetDataAsync(RequiredNull<NullableFirestoreItem>()));
            await AssertRejectsAsync(() => document.SetDataAsync(RequiredNull<Dictionary<object, object?>>()));
            await AssertRejectsAsync(() => document.UpdateDataAsync(RequiredNull<Dictionary<object, object?>>()));

            var batch = sut.CreateBatch();
            AssertRejects(() => batch.SetData(RequiredNull<IDocumentReference>(), CreateNonNullItem("batch")));
            AssertRejects(() => batch.SetData(document, RequiredNull<Dictionary<object, object?>>()));
            AssertRejects(() => batch.UpdateData(document, RequiredNull<Dictionary<object, object?>>()));

            await AssertRejectsAsync(() => sut.RunTransactionAsync<string?>(transaction => {
                transaction.SetData(RequiredNull<IDocumentReference>(), CreateNonNullItem("transaction"));
                return "unreachable";
            }));
        }

        public async Task DisposeAsync()
        {
            TestLog.Write($"[FIRESTORE NULLABILITY CLEANUP START] {_testingCollectionPath}");

            try {
                await CrossFirebaseFirestore.Current
                    .DeleteCollectionAsync<NullableFirestoreItem>(_testingCollectionPath, batchSize: 10)
                    .WaitAsync(TimeSpan.FromSeconds(15));
                TestLog.Write($"[FIRESTORE NULLABILITY CLEANUP END] {_testingCollectionPath}");
            } catch(TimeoutException) {
                TestLog.Write($"[FIRESTORE NULLABILITY CLEANUP TIMEOUT] {_testingCollectionPath}");
            } catch(Exception e) {
                TestLog.Write($"[FIRESTORE NULLABILITY CLEANUP ERROR] {_testingCollectionPath}: {e}");
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

        private static NullableFirestoreItem CreateNullableItem(string? queryMarker)
        {
            return new NullableFirestoreItem(
                nullableString: null,
                nullableNumber: null,
                nullableMap: new Dictionary<string, object?> {
                    { "inner_null", null },
                    { "inner_value", "nested-value" }
                },
                nullableList: CreateNullableList(),
                queryMarker: queryMarker
            );
        }

        private static NullableFirestoreItem CreateNonNullItem(string queryMarker)
        {
            return new NullableFirestoreItem(
                nullableString: "seed",
                nullableNumber: 42,
                nullableMap: new Dictionary<string, object?> {
                    { "inner_null", "seed" },
                    { "inner_value", "seed" }
                },
                nullableList: new List<object?> { "seed" },
                queryMarker: queryMarker
            );
        }

        private static Dictionary<object, object?> CreateNullableDictionary(string? queryMarker)
        {
            return new Dictionary<object, object?> {
                { "nullable_string", null },
                { "nullable_number", null },
                { "nullable_map", CreateNestedMap() },
                { "nullable_list", CreateNullableList() },
                { "query_marker", queryMarker }
            };
        }

        private static Dictionary<object, object?> CreateNestedMap()
        {
            return new Dictionary<object, object?> {
                { "inner_null", null },
                { "inner_value", "nested-value" }
            };
        }

        private static List<object?> CreateNullableList()
        {
            return new List<object?> { "first", null, "last" };
        }

        private static async Task AssertNullableDocumentAsync(IDocumentReference document, string? expectedMarker)
        {
            var snapshot = await document.GetDocumentSnapshotAsync<NullableFirestoreItem>(Source.Server);
            var item = Require(snapshot.Data);
            Assert.Equal(expectedMarker, item.QueryMarker);
            Assert.Null(item.NullableString);
            Assert.Null(item.NullableNumber);

            var map = Require(item.NullableMap);
            Assert.True(map.ContainsKey("inner_null"));
            Assert.Null(map["inner_null"]);
            Assert.Equal("nested-value", map["inner_value"]);

            var list = Require(item.NullableList);
            Assert.Equal(["first", null, "last"], list);
        }

        private static T Require<T>(T? value) where T : class
        {
            if(value is null) {
                throw new InvalidOperationException("Expected a non-null value.");
            }

            return value;
        }

        private static void AssertRejects(Action action)
        {
            var exception = Record.Exception(action);
            Assert.NotNull(exception);
        }

        private static async Task AssertRejectsAsync(Func<Task> action)
        {
            var exception = await Record.ExceptionAsync(action);
            Assert.NotNull(exception);
        }

#nullable disable
        private static T RequiredNull<T>() where T : class
        {
            return null;
        }
#nullable enable
    }
}