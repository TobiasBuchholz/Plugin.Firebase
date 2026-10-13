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
    public async Task rejects_fractional_values_read_into_integral_targets()
    {
        // Firestore only stores fractional numbers as doubles. Reading one into an integral
        // property throws instead of silently rounding (Android) or truncating (iOS).
        var sut = CrossFirebaseFirestore.Current;
        var document = GetTestingDocument(sut, "numeric-fractional");
        await document.SetDataAsync(new Dictionary<object, object?> {
            { "f27", 2.7 },
            { "f25", 2.5 },
            { "f35", 3.5 },
            { "fm27", -2.7 }
        });

        var longs = await document.GetDocumentSnapshotAsync<FractionalLongsDocument>();
        Assert.Throws<InvalidCastException>(() => _ = longs.Data);

        var ints = await document.GetDocumentSnapshotAsync<FractionalIntsDocument>();
        Assert.Throws<InvalidCastException>(() => _ = ints.Data);

        var enums = await document.GetDocumentSnapshotAsync<FractionalEnumDocument>();
        Assert.Throws<InvalidCastException>(() => _ = enums.Data);

        var dictionary = await document.GetDocumentSnapshotAsync<Dictionary<string, long>>();
        Assert.Throws<InvalidCastException>(() => _ = dictionary.Data);
    }

    [Fact]
    public async Task reads_integral_doubles_into_integral_targets()
    {
        // Doubles without a fractional part lose nothing, so they still convert.
        var sut = CrossFirebaseFirestore.Current;
        var document = GetTestingDocument(sut, "numeric-integral-doubles");
        await document.SetDataAsync(new Dictionary<object, object?> {
            { "positive", 2.0 },
            { "negative", -3.0 }
        });

        var data = (await document.GetDocumentSnapshotAsync<IntegralDoublesDocument>()).Data!;
        Assert.Equal(2L, data.Positive);
        Assert.Equal(-3L, data.Negative);

        var dictionary = (await document.GetDocumentSnapshotAsync<Dictionary<string, long>>()).Data!;
        Assert.Equal(2L, dictionary["positive"]);
        Assert.Equal(-3L, dictionary["negative"]);
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

        await AssertOutOfRangeAsync<CharRangeDocument>(document);

        // Convert.ChangeType has no double-to-char conversion on either platform.
        var fractional = await document.GetDocumentSnapshotAsync<FractionalCharDocument>();
        Assert.Throws<InvalidCastException>(() => _ = fractional.Data);
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
            { "below_unsigned", -1L }
        });

        await AssertOutOfRangeAsync<ByteRangeDocument>(document);
        await AssertOutOfRangeAsync<SByteRangeDocument>(document);
        await AssertOutOfRangeAsync<ShortRangeDocument>(document);
        await AssertOutOfRangeAsync<UShortRangeDocument>(document);
        await AssertOutOfRangeAsync<IntRangeDocument>(document);
        await AssertOutOfRangeAsync<NullableIntRangeDocument>(document);
        await AssertOutOfRangeAsync<UIntRangeDocument>(document);
        await AssertOutOfRangeAsync<ULongRangeDocument>(document);

        // Only properties that map an out-of-range field fail; the rest of the document is fine.
        var withinRange = (await document.GetDocumentSnapshotAsync<ByteWithinRangeDocument>()).Data!;
        Assert.Equal((byte) 200, withinRange.Value);
    }

    [Fact]
    public async Task rejects_enum_values_outside_the_underlying_type_range()
    {
        var sut = CrossFirebaseFirestore.Current;
        var document = GetTestingDocument(sut, "enum-out-of-range");
        await document.SetDataAsync(new Dictionary<object, object?> {
            // without a range check Enum.ToObject narrows these to 44 and 705032704
            { "byte_enum", 300L },
            { "int_enum", 5000000000L },
            { "byte_min", 0L },
            { "byte_max", 255L },
            { "int_min", (long) int.MinValue },
            { "int_max", (long) int.MaxValue }
        });

        await AssertOutOfRangeAsync<ByteEnumRangeDocument>(document);
        await AssertOutOfRangeAsync<IntEnumRangeDocument>(document);
    }

    [Fact]
    public async Task reads_enum_values_at_the_underlying_type_edges()
    {
        var sut = CrossFirebaseFirestore.Current;
        var document = GetTestingDocument(sut, "enum-edges");
        await document.SetDataAsync(new Dictionary<object, object?> {
            { "byte_enum", 0L },
            { "int_enum", 0L },
            { "byte_min", 0L },
            { "byte_max", 255L },
            { "int_min", (long) int.MinValue },
            { "int_max", (long) int.MaxValue }
        });

        var byteData = (await document.GetDocumentSnapshotAsync<ByteEnumRangeDocument>()).Data!;
        Assert.Equal(EdgeByteRating.Min, byteData.ByteMin);
        Assert.Equal(EdgeByteRating.Max, byteData.ByteMax);

        var intData = (await document.GetDocumentSnapshotAsync<IntEnumRangeDocument>()).Data!;
        Assert.Equal(EdgeIntRating.Min, intData.IntMin);
        Assert.Equal(EdgeIntRating.Max, intData.IntMax);
    }

    private static async Task AssertOutOfRangeAsync<T>(IDocumentReference document)
    {
        var snapshot = await document.GetDocumentSnapshotAsync<T>();
        Assert.Throws<OverflowException>(() => _ = snapshot.Data);
    }
}