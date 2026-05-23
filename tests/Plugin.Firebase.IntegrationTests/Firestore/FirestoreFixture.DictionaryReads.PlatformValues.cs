using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore
{
    public sealed partial class FirestoreFixture
    {
        [Fact]
        public async Task round_trips_typed_boolean_and_datetime_map_values()
        {
            var sut = CrossFirebaseFirestore.Current;
            var document = GetTestingDocument(sut, "typed-boolean-and-datetime-map-values");
            var expectedEarlyDate = new DateTimeOffset(2026, 5, 2, 13, 14, 15, 123, TimeSpan.Zero);
            var expectedLateDate = new DateTimeOffset(2026, 5, 3, 16, 17, 18, 456, TimeSpan.Zero);

            await document.SetDataAsync(new TypedMapValuesDocument(
                new Dictionary<string, bool> {
                    { "enabled", true },
                    { "disabled", false }
                },
                new Dictionary<string, DateTimeOffset> {
                    { "early", expectedEarlyDate },
                    { "late", expectedLateDate }
                }));

            var snapshot = await document.GetDocumentSnapshotAsync<TypedMapValuesDocument>(Source.Server);

            Assert.True(snapshot.Data!.BooleanMaps["enabled"]);
            Assert.False(snapshot.Data!.BooleanMaps["disabled"]);
            Assert.InRange(
                Math.Abs(snapshot.Data!.DateMaps["early"].Ticks - expectedEarlyDate.Ticks),
                0,
                IntegrationTestTimeouts.OneMillisecondTicks);
            Assert.InRange(
                Math.Abs(snapshot.Data!.DateMaps["late"].Ticks - expectedLateDate.Ticks),
                0,
                IntegrationTestTimeouts.OneMillisecondTicks);
        }


        [IosFact]
        public async Task reads_ios_dictionary_object_numeric_and_boolean_values()
        {
            var sut = CrossFirebaseFirestore.Current;
            var document = GetTestingDocument(sut, "ios-dictionary-object-values");
            await document.SetDataAsync(new DictionaryObjectValuesDocument(
                new Dictionary<string, object?> {
                    { "enabled", true },
                    { "count", 5L },
                    { "ratio", 1.25 }
                }));

            var snapshot = await document.GetDocumentSnapshotAsync<DictionaryObjectValuesDocument>();

            Assert.True((bool) snapshot.Data!.Values["enabled"]!);
            Assert.Equal(5L, Convert.ToInt64(snapshot.Data!.Values["count"]));
            Assert.Equal(1.25, Convert.ToDouble(snapshot.Data!.Values["ratio"]));
        }


        [IosFact]
        public async Task reads_ios_enum_dictionary_values()
        {
            var sut = CrossFirebaseFirestore.Current;
            var document = GetTestingDocument(sut, "ios-enum-dictionary-values");
            await document.SetDataAsync(new EnumDictionaryDocument(
                new Dictionary<string, PokeType> {
                    { "fire", PokeType.Fire },
                    { "water", PokeType.Water }
                }));

            var snapshot = await document.GetDocumentSnapshotAsync<EnumDictionaryDocument>();

            Assert.Equal(PokeType.Fire, snapshot.Data!.Values["fire"]);
            Assert.Equal(PokeType.Water, snapshot.Data!.Values["water"]);
        }


        [AndroidFact]
        public async Task reads_android_typed_numeric_collection_values()
        {
            var sut = CrossFirebaseFirestore.Current;
            var document = GetTestingDocument(sut, "android-typed-numeric-collections");
            await document.SetDataAsync(new Dictionary<object, object?> {
                {
                    "counts",
                    new Dictionary<object, object?> {
                        { "one", 1L },
                        { "two", 2L }
                    }
                },
                { "nullable_counts", new object?[] { 1L, null, 3L } },
                {
                    "types",
                    new Dictionary<object, object?> {
                        { "fire", PokeType.Fire },
                        { "water", PokeType.Water }
                    }
                }
            });

            var snapshot = await document.GetDocumentSnapshotAsync<AndroidNumericCollectionsDocument>();

            Assert.Equal(1, snapshot.Data!.Counts["one"]);
            Assert.Equal(2, snapshot.Data!.Counts["two"]);
            Assert.Equal([1, null, 3], snapshot.Data!.NullableCounts);
            Assert.Equal(PokeType.Fire, snapshot.Data!.Types["fire"]);
            Assert.Equal(PokeType.Water, snapshot.Data!.Types["water"]);
        }


        [Fact]
        public async Task writes_set_data_from_dictionary_and_tuple_payloads()
        {
            var sut = CrossFirebaseFirestore.Current;

            var dictionaryDocument = GetTestingDocument(sut, "setdata-string-dictionary");
            await dictionaryDocument.SetDataAsync(new Dictionary<string, object?> {
                { "field_a", "dictionary-a" },
                { "field_b", "dictionary-b" }
            });
            var dictionaryResult = (await dictionaryDocument.GetDocumentSnapshotAsync<SetDataPayloadDocument>()).Data!;
            Assert.Equal("dictionary-a", dictionaryResult.FieldA);
            Assert.Equal("dictionary-b", dictionaryResult.FieldB);

            var tupleDocument = GetTestingDocument(sut, "setdata-tuple");
            await tupleDocument.SetDataAsync(
                ("field_a", "tuple-a"),
                ("field_b", "tuple-b"));
            var tupleResult = (await tupleDocument.GetDocumentSnapshotAsync<SetDataPayloadDocument>()).Data!;
            Assert.Equal("tuple-a", tupleResult.FieldA);
            Assert.Equal("tuple-b", tupleResult.FieldB);
        }


        [AndroidFact]
        public async Task writes_transaction_set_data_from_dictionary_and_tuple_payloads_on_android()
        {
            var sut = CrossFirebaseFirestore.Current;
            var transactionDictionaryDocument = GetTestingDocument(sut, "transaction-setdata-string-dictionary");
            var transactionTupleDocument = GetTestingDocument(sut, "transaction-setdata-tuple");
            await sut.RunTransactionAsync(transaction => {
                transaction.SetData(
                    transactionDictionaryDocument,
                    new Dictionary<string, object?> {
                        { "field_a", "transaction-dictionary-a" },
                        { "field_b", "transaction-dictionary-b" }
                    });
                transaction.SetData(
                    transactionTupleDocument,
                    ("field_a", "transaction-tuple-a"),
                    ("field_b", "transaction-tuple-b"));
                return true;
            });

            var transactionDictionaryResult = (await transactionDictionaryDocument.GetDocumentSnapshotAsync<SetDataPayloadDocument>()).Data!;
            Assert.Equal("transaction-dictionary-a", transactionDictionaryResult.FieldA);
            Assert.Equal("transaction-dictionary-b", transactionDictionaryResult.FieldB);

            var transactionTupleResult = (await transactionTupleDocument.GetDocumentSnapshotAsync<SetDataPayloadDocument>()).Data!;
            Assert.Equal("transaction-tuple-a", transactionTupleResult.FieldA);
            Assert.Equal("transaction-tuple-b", transactionTupleResult.FieldB);
        }

    }
}