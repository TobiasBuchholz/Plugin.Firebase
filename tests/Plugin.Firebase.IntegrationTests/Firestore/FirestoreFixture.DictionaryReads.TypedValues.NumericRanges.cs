using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore;

public sealed partial class FirestoreFixture
{
    [Fact]
    public async Task gets_document_data_with_numeric_values_at_type_boundaries()
    {
        var sut = CrossFirebaseFirestore.Current;
        var document = GetTestingDocument(sut, "numeric-boundaries");
        await document.SetDataAsync(new Dictionary<object, object?> {
            { "byte_min", 0L },
            { "byte_max", 255L },
            { "sbyte_min", -128L },
            { "sbyte_max", 127L },
            { "short_min", -32768L },
            { "short_max", 32767L },
            { "ushort_min", 0L },
            { "ushort_max", 65535L },
            { "int_min", (long) int.MinValue },
            { "int_max", (long) int.MaxValue },
            { "uint_min", 0L },
            { "uint_max", (long) uint.MaxValue },
            { "long_min", long.MinValue },
            { "long_max", long.MaxValue },
            { "ulong_min", 0L },
            { "ulong_max", long.MaxValue }
        });

        var data = (await document.GetDocumentSnapshotAsync<NumericBoundariesDocument>()).Data!;

        Assert.Equal(byte.MinValue, data.ByteMin);
        Assert.Equal(byte.MaxValue, data.ByteMax);
        Assert.Equal(sbyte.MinValue, data.SByteMin);
        Assert.Equal(sbyte.MaxValue, data.SByteMax);
        Assert.Equal(short.MinValue, data.ShortMin);
        Assert.Equal(short.MaxValue, data.ShortMax);
        Assert.Equal(ushort.MinValue, data.UShortMin);
        Assert.Equal(ushort.MaxValue, data.UShortMax);
        Assert.Equal(int.MinValue, data.IntMin);
        Assert.Equal(int.MaxValue, data.IntMax);
        Assert.Equal(uint.MinValue, data.UIntMin);
        Assert.Equal(uint.MaxValue, data.UIntMax);
        Assert.Equal(long.MinValue, data.LongMin);
        Assert.Equal(long.MaxValue, data.LongMax);
        Assert.Equal(ulong.MinValue, data.ULongMin);
        Assert.Equal((ulong) long.MaxValue, data.ULongMax);
    }

    [Fact]
    public async Task gets_nullable_and_dictionary_decimal_values()
    {
        // Non-nullable decimal properties are covered by reads_typed_model_with_full_width_numeric_values.
        var sut = CrossFirebaseFirestore.Current;
        var document = GetTestingDocument(sut, "numeric-decimals");
        await document.SetDataAsync(new Dictionary<object, object?> {
            { "integral", 12345L },
            { "fractional", 12.5 },
            { "nullable_present", 67890L },
            { "nullable_missing", null }
        });

        var data = (await document.GetDocumentSnapshotAsync<NullableDecimalDocument>()).Data!;

        Assert.Equal(67890m, data.NullablePresent);
        Assert.Null(data.NullableMissing);

        var decimals = (await document.GetDocumentSnapshotAsync<Dictionary<string, decimal?>>()).Data!;

        Assert.Equal(12345m, decimals["integral"]);
        Assert.Equal(12.5m, decimals["fractional"]);
        Assert.Equal(67890m, decimals["nullable_present"]);
        Assert.Null(decimals["nullable_missing"]);
    }

    [Fact]
    public async Task rejects_out_of_range_and_fractional_char_values()
    {
        var sut = CrossFirebaseFirestore.Current;
        var document = GetTestingDocument(sut, "numeric-chars");
        await document.SetDataAsync(new Dictionary<object, object?> {
            { "above_char", 70000L },
            { "fractional", 65.5 }
        });

        // Convert.ChangeType has no double-to-char conversion on either platform.
        var fractional = await document.GetDocumentSnapshotAsync<FractionalCharDocument>();
        Assert.Throws<InvalidCastException>(() => _ = fractional.Data);

        await AssertOutOfRangeAsync<CharRangeDocument>(document);
    }

    [Fact]
    public async Task throws_when_document_data_exceeds_the_target_numeric_range()
    {
        var sut = CrossFirebaseFirestore.Current;
        var document = GetTestingDocument(sut, "numeric-out-of-range");
        await document.SetDataAsync(new Dictionary<object, object?> {
            { "within_byte", 200L },
            { "above_byte", 300L },
            { "above_sbyte", 128L },
            { "above_short", 40000L },
            { "above_ushort", 70000L },
            { "above_int", 5000000000L },
            { "below_unsigned", -1L },
            { "above_float", 1e300 }
        });

        // Only properties that map an out-of-range field fail; the rest of the document is fine.
        var withinRange = (await document.GetDocumentSnapshotAsync<ByteWithinRangeDocument>()).Data!;
        Assert.Equal((byte) 200, withinRange.Value);

        await AssertOutOfRangeAsync<ByteRangeDocument>(document);
        await AssertOutOfRangeAsync<SByteRangeDocument>(document);
        await AssertOutOfRangeAsync<ShortRangeDocument>(document);
        await AssertOutOfRangeAsync<UShortRangeDocument>(document);
        await AssertOutOfRangeAsync<IntRangeDocument>(document);
        await AssertOutOfRangeAsync<NullableIntRangeDocument>(document);
        await AssertOutOfRangeAsync<UIntRangeDocument>(document);
        await AssertOutOfRangeAsync<ULongRangeDocument>(document);
        // A double too large for a float throws instead of becoming infinity.
        await AssertOutOfRangeAsync<FloatRangeDocument>(document);
    }

    private static async Task AssertOutOfRangeAsync<T>(IDocumentReference document)
    {
        // Snapshots are typed, so each target type needs a snapshot of its own. Callers read the
        // document from the server first, so these come from the local cache instead of repeating
        // the round trip.
        var snapshot = await document.GetDocumentSnapshotAsync<T>(Source.Cache);
        Assert.Throws<OverflowException>(() => _ = snapshot.Data);
    }
}