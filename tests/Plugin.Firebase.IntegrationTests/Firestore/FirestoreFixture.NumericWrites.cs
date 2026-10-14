using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore;

public sealed partial class FirestoreFixture
{
    [Fact]
    public async Task round_trips_typed_model_numeric_writes_at_boundaries()
    {
        var sut = CrossFirebaseFirestore.Current;
        foreach(var (name, written) in new[] { ("min", NumericWritesDocument.CreateMin()), ("max", NumericWritesDocument.CreateMax()) }) {
            var document = GetTestingDocument(sut, $"typed-model-numeric-writes-{name}");
            await document.SetDataAsync(written);

            var snapshot = await document.GetDocumentSnapshotAsync<NumericWritesDocument>();
            Assert.Equivalent(written, snapshot.Data);
            await AssertStoredValuesAsync(document, written.ToWireValues());
        }
    }

    [Fact]
    public async Task round_trips_nullable_numeric_property_writes()
    {
        var sut = CrossFirebaseFirestore.Current;
        foreach(var (name, written) in new[] { ("values", NullableNumericWritesDocument.CreateWithValues()), ("nulls", new NullableNumericWritesDocument()) }) {
            var document = GetTestingDocument(sut, $"typed-model-nullable-numeric-writes-{name}");
            await document.SetDataAsync(written);

            var snapshot = await document.GetDocumentSnapshotAsync<NullableNumericWritesDocument>();
            Assert.Equivalent(written, snapshot.Data);
            await AssertStoredValuesAsync(document, written.ToWireValues());
        }
    }

    [Fact]
    public async Task stores_numeric_writes_as_int64_and_double_on_the_wire()
    {
        var sut = CrossFirebaseFirestore.Current;
        var document = GetTestingDocument(sut, "numeric-write-wire-types");
        // Android converts dictionary entries, list elements and array-union elements on separate paths
        await document.SetDataAsync(new Dictionary<object, object?> {
            { "byte", (byte) 5 },
            { "short", (short) -3 },
            { "float", 2.5f },
            { "byte_enum", ByteRating.Max },
            { "int_enum", IntRating.Min },
            { "list", new object?[] { (sbyte) -1, (ushort) 7, 0.5f, UintRating.Max, "x" } }
        });
        await document.UpdateDataAsync(("union", FieldValue.ArrayUnion((byte) 1, UshortRating.Max)));

        await AssertStoredValuesAsync(document, new Dictionary<string, object?> {
            { "byte", 5L },
            { "short", -3L },
            { "float", 2.5 },
            { "byte_enum", 255L },
            { "int_enum", (long) int.MinValue },
            { "list", new object?[] { -1L, 7L, 0.5, 4294967295L, "x" } },
            { "union", new object?[] { 1L, 65535L } }
        });
    }

    [Fact]
    public async Task round_trips_enum_writes_for_each_underlying_type()
    {
        var sut = CrossFirebaseFirestore.Current;
        foreach(var (name, written) in new[] { ("min", EnumWidthsDocument.CreateMin()), ("max", EnumWidthsDocument.CreateMax()) }) {
            var document = GetTestingDocument(sut, $"typed-model-enum-writes-{name}");
            await document.SetDataAsync(written);

            var snapshot = await document.GetDocumentSnapshotAsync<EnumWidthsDocument>();
            Assert.Equivalent(written, snapshot.Data);
            await AssertStoredValuesAsync(document, written.ToWireValues());

            var updated = GetTestingDocument(sut, $"enum-update-writes-{name}");
            await updated.SetDataAsync(("seed", 0L));
            await updated.UpdateDataAsync(written.ToFieldValues());
            await AssertStoredValuesAsync(updated, written.ToWireValues());
        }
    }

    [Fact]
    public async Task writes_numeric_values_through_update_batch_and_transaction_paths()
    {
        var sut = CrossFirebaseFirestore.Current;
        var document = GetTestingDocument(sut, "numeric-write-paths");
        await document.SetDataAsync(("seed", 0L));

        await document.UpdateDataAsync(("update_ushort", ushort.MaxValue), ("update_uint_enum", UintRating.Max));
        await sut.CreateBatch()
            .SetData(document, SetOptions.Merge(), ("batch_set_uint", uint.MaxValue))
            .UpdateData(document, ("batch_update_sbyte", sbyte.MinValue), ("batch_update_ulong_enum", UlongRating.Max))
            .CommitAsync();
        var result = await sut.RunTransactionAsync<object>(transaction => {
            transaction.SetData(document, SetOptions.Merge(), ("transaction_set_byte_enum", ByteRating.Max));
            transaction.UpdateData(document, ("transaction_update_byte", byte.MaxValue), ("transaction_update_short_enum", ShortRating.Min));
            return (byte) 5;
        });

        await AssertStoredValuesAsync(document, new Dictionary<string, object?> {
            { "update_ushort", 65535L },
            { "update_uint_enum", 4294967295L },
            { "batch_set_uint", 4294967295L },
            { "batch_update_sbyte", -128L },
            { "batch_update_ulong_enum", long.MaxValue },
            { "transaction_set_byte_enum", 255L },
            { "transaction_update_byte", 255L },
            { "transaction_update_short_enum", (long) short.MinValue }
        });
        // a transaction result passes through the same conversion, so an untyped result comes back widened
        Assert.Equal(5L, Assert.IsType<long>(result));
        Assert.Equal(2.5, Assert.IsType<double>(await sut.RunTransactionAsync<object>(_ => 2.5f)));
    }

    [Fact]
    public async Task rejects_decimal_and_char_writes()
    {
        var sut = CrossFirebaseFirestore.Current;
        var document = GetTestingDocument(sut, "decimal-and-char-writes");
        await document.SetDataAsync(("seed", 0L));

        // Firestore has no decimal or character type, so neither is converted silently
        await Assert.ThrowsAsync<ArgumentException>(() => document.SetDataAsync(new DecimalPriceDocument()));
        await Assert.ThrowsAsync<ArgumentException>(() => document.SetDataAsync(("price", 19.99m)));
        await Assert.ThrowsAsync<ArgumentException>(() => document.UpdateDataAsync(("price", 19.99m)));
        await Assert.ThrowsAsync<ArgumentException>(() => document.SetDataAsync(("initial", 'A')));
        await Assert.ThrowsAsync<ArgumentException>(() => document.UpdateDataAsync(("initials", new object?[] { 'A' })));

        await AssertStoredValuesAsync(document, new Dictionary<string, object?> { { "seed", 0L } });
    }

    [Fact]
    public async Task rejects_ulong_writes_above_int64_max()
    {
        var sut = CrossFirebaseFirestore.Current;
        var document = GetTestingDocument(sut, "ulong-overflow-writes");
        await document.SetDataAsync(("seed", 0L));

        // values above long.MaxValue have no Firestore representation, so they fail instead of wrapping negative
        await Assert.ThrowsAsync<ArgumentException>(() => document.SetDataAsync(("too_big", (ulong) long.MaxValue + 1)));
        await Assert.ThrowsAsync<ArgumentException>(() => document.SetDataAsync(("too_big", UlongRating.AboveInt64Max)));
        await Assert.ThrowsAsync<ArgumentException>(() => document.UpdateDataAsync(("too_big", UlongRating.AboveInt64Max)));
        await Assert.ThrowsAsync<ArgumentException>(() => document.UpdateDataAsync(("too_big", new object?[] { ulong.MaxValue })));

        await AssertStoredValuesAsync(document, new Dictionary<string, object?> { { "seed", 0L } });
    }

    private static async Task AssertStoredValuesAsync(IDocumentReference document, IReadOnlyDictionary<string, object?> expected)
    {
        var stored = (await document.GetDocumentSnapshotAsync<Dictionary<string, object?>>()).Data!;
        foreach(var (field, value) in expected) {
            AssertStoredValue(field, value, Assert.Contains(field, stored));
        }
    }

    private static void AssertStoredValue(string field, object? expected, object? actual)
    {
        if(expected is object?[] expectedItems) {
            var items = Assert.IsAssignableFrom<IList<object?>>(actual);
            Assert.Equal(expectedItems.Length, items.Count);
            for(var i = 0; i < items.Count; i++) {
                AssertStoredValue($"{field}[{i}]", expectedItems[i], items[i]);
            }
            return;
        }

        // boxed numbers are only equal when their types match, so an int64 can't pass for a double or a boolean
        Assert.True(Equals(expected, actual), $"'{field}' was stored as {Describe(actual)}, expected {Describe(expected)}");
    }

    private static string Describe(object? value) => value is null ? "null" : $"{value} ({value.GetType().Name})";
}