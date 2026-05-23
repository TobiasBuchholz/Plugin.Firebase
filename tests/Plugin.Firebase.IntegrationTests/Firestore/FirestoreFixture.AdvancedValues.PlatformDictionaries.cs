using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore
{
    public sealed partial class FirestoreFixture
    {
        [IosFact]
        public async Task writes_nested_dictionary_properties_on_ios()
        {
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
            Assert.Equal((short) 7, snapshot.Data!.Values["outer"]["inner"]);
        }

        [IosFact]
        public async Task applies_ios_batch_tuple_set_options()
        {
            var sut = CrossFirebaseFirestore.Current;
            var document = GetTestingDocument(sut, "ios-batch-tuple-set-options");
            await document.SetDataAsync(new Dictionary<object, object?> {
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

            var result = (await document.GetDocumentSnapshotAsync<BatchMergeFieldsDocument>()).Data!;
            Assert.Equal("keep", result.Untouched);
            Assert.Equal("from-batch", result.Selected);
        }

        [IosFact]
        public async Task writes_ios_dictionary_data_through_non_document_wrappers()
        {
            var sut = CrossFirebaseFirestore.Current;
            var collection = GetTestingCollection(sut);

            var addedDocument = await collection.AddDocumentAsync(new Dictionary<object, object?> {
                { "writer", "collection-add" },
                { "count", 1L }
            });
            var addedResult = (await addedDocument.GetDocumentSnapshotAsync<WriteWrapperDictionaryDocument>()).Data!;
            Assert.Equal("collection-add", addedResult.Writer);
            Assert.Equal(1L, addedResult.Count);

            var batchSetDocument = collection.GetDocument("ios-batch-set-dictionary");
            var batchUpdateDocument = collection.GetDocument("ios-batch-update-dictionary");
            await batchUpdateDocument.SetDataAsync(new Dictionary<object, object?> { { "writer", "seed" } });
            var batch = sut.CreateBatch();
            batch.SetData(batchSetDocument, new Dictionary<object, object?> {
                { "writer", "batch-set" },
                { "count", 2L }
            });
            batch.UpdateData(batchUpdateDocument, new Dictionary<object, object?> {
                { "writer", "batch-update" },
                { "count", 3L }
            });
            await batch.CommitAsync();

            var batchSetResult = (await batchSetDocument.GetDocumentSnapshotAsync<WriteWrapperDictionaryDocument>()).Data!;
            Assert.Equal("batch-set", batchSetResult.Writer);
            Assert.Equal(2L, batchSetResult.Count);

            var batchUpdateResult = (await batchUpdateDocument.GetDocumentSnapshotAsync<WriteWrapperDictionaryDocument>()).Data!;
            Assert.Equal("batch-update", batchUpdateResult.Writer);
            Assert.Equal(3L, batchUpdateResult.Count);

            var batchMergeDocument = collection.GetDocument("ios-batch-merge-dictionary");
            await batchMergeDocument.SetDataAsync(new Dictionary<object, object?> {
                { "writer", "seed" },
                { "count", 30L },
                { "untouched", "kept-by-batch-merge" }
            });
            var mergeBatch = sut.CreateBatch();
            mergeBatch.SetData(
                batchMergeDocument,
                new Dictionary<object, object?> { { "writer", "batch-merge" } },
                SetOptions.Merge()
            );
            await mergeBatch.CommitAsync();

            var batchMergeResult = (await batchMergeDocument.GetDocumentSnapshotAsync<WriteWrapperDictionaryDocument>()).Data!;
            Assert.Equal("batch-merge", batchMergeResult.Writer);
            Assert.Equal(30L, batchMergeResult.Count);
            Assert.Equal("kept-by-batch-merge", batchMergeResult.Untouched);

            var transactionSetDocument = collection.GetDocument("ios-transaction-set-dictionary");
            var transactionUpdateDocument = collection.GetDocument("ios-transaction-update-dictionary");
            await transactionUpdateDocument.SetDataAsync(new Dictionary<object, object?> { { "writer", "seed" } });
            await sut.RunTransactionAsync(transaction => {
                transaction.GetDocument<WriteWrapperDictionaryDocument>(transactionUpdateDocument);
                transaction.SetData(transactionSetDocument, new Dictionary<object, object?> {
                    { "writer", "transaction-set" },
                    { "count", 4L }
                });
                transaction.UpdateData(transactionUpdateDocument, new Dictionary<object, object?> {
                    { "writer", "transaction-update" },
                    { "count", 5L }
                });
                return true;
            });

            var transactionSetResult = (await transactionSetDocument.GetDocumentSnapshotAsync<WriteWrapperDictionaryDocument>()).Data!;
            Assert.Equal("transaction-set", transactionSetResult.Writer);
            Assert.Equal(4L, transactionSetResult.Count);

            var transactionUpdateResult = (await transactionUpdateDocument.GetDocumentSnapshotAsync<WriteWrapperDictionaryDocument>()).Data!;
            Assert.Equal("transaction-update", transactionUpdateResult.Writer);
            Assert.Equal(5L, transactionUpdateResult.Count);

            var transactionMergeDocument = collection.GetDocument("ios-transaction-merge-dictionary");
            await transactionMergeDocument.SetDataAsync(new Dictionary<object, object?> {
                { "writer", "seed" },
                { "count", 50L },
                { "untouched", "kept-by-transaction-merge" }
            });
            await sut.RunTransactionAsync(transaction => {
                transaction.GetDocument<WriteWrapperDictionaryDocument>(transactionMergeDocument);
                transaction.SetData(
                    transactionMergeDocument,
                    new Dictionary<object, object?> { { "writer", "transaction-merge" } },
                    SetOptions.Merge()
                );
                return true;
            });

            var transactionMergeResult = (await transactionMergeDocument.GetDocumentSnapshotAsync<WriteWrapperDictionaryDocument>()).Data!;
            Assert.Equal("transaction-merge", transactionMergeResult.Writer);
            Assert.Equal(50L, transactionMergeResult.Count);
            Assert.Equal("kept-by-transaction-merge", transactionMergeResult.Untouched);
        }

        [IosFact]
        public async Task updates_ios_transaction_dictionary_data_with_field_value_and_date_time_offset()
        {
            var sut = CrossFirebaseFirestore.Current;
            var document = GetTestingDocument(sut, "ios-transaction-update-dictionary-field-value");
            var seedDate = new DateTimeOffset(2025, 8, 27, 2, 9, 54, TimeSpan.Zero);
            var expectedDate = new DateTimeOffset(2025, 8, 27, 3, 9, 54, TimeSpan.Zero);
            await document.SetDataAsync(new Dictionary<object, object?> {
                { "array_values", new List<string> { "seed" } },
                { "updated_at", seedDate }
            });

            await sut.RunTransactionAsync(transaction => {
                transaction.GetDocument<Issue522TransactionUpdateDocument>(document);
                transaction.UpdateData(document, new Dictionary<object, object?> {
                    { "array_values", FieldValue.ArrayUnion("added") },
                    { "updated_at", expectedDate }
                });
                return true;
            });

            var result = (await document.GetDocumentSnapshotAsync<Issue522TransactionUpdateDocument>()).Data!;
            Assert.Contains("seed", result.ArrayValues);
            Assert.Contains("added", result.ArrayValues);
            Assert.Equal(expectedDate.ToUnixTimeMilliseconds(), result.UpdatedAt.ToUnixTimeMilliseconds());
        }
    }
}