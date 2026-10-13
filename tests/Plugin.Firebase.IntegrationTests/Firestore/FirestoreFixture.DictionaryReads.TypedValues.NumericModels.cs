using System.Globalization;
using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore;

public sealed partial class FirestoreFixture
{
    [AndroidFact]
    public async Task reads_android_typed_model_with_full_width_numeric_values()
    {
        var sut = CrossFirebaseFirestore.Current;
        var document = GetTestingDocument(sut, "android-typed-model-numeric-widths");
        await document.SetDataAsync(new Dictionary<object, object?> {
            { "byte_value", 255L },
            { "sbyte_value", -128L },
            { "short_value", -32768L },
            { "ushort_value", 65535L },
            { "int_value", 2147483647L },
            { "uint_value", 4294967295L },
            { "long_value", 9223372036854775807L },
            { "ulong_value", 9223372036854775807L },
            { "float_value", 2.5 },
            { "whole_float_value", 2L },
            { "double_value", 2.25 },
            { "whole_double_value", 2L },
            { "decimal_from_whole_value", 5L },
            { "decimal_from_fraction_value", 5.25 },
            { "char_value", 65L },
            { "byte_list_value", new[] { 0L, 255L } }
        });

        var snapshot = await document.GetDocumentSnapshotAsync<NumericWidthsDocument>();
        var data = snapshot.Data!;

        Assert.Equal((byte) 255, data.ByteValue);
        Assert.Equal((sbyte) -128, data.SbyteValue);
        Assert.Equal((short) -32768, data.ShortValue);
        Assert.Equal((ushort) 65535, data.UshortValue);
        Assert.Equal(2147483647, data.IntValue);
        Assert.Equal(4294967295U, data.UintValue);
        Assert.Equal(9223372036854775807L, data.LongValue);
        Assert.Equal(9223372036854775807UL, data.UlongValue);
        Assert.Equal(2.5f, data.FloatValue);
        Assert.Equal(2.0f, data.WholeFloatValue);
        Assert.Equal(2.25, data.DoubleValue);
        Assert.Equal(2.0, data.WholeDoubleValue);
        Assert.Equal(5m, data.DecimalFromWholeValue);
        Assert.Equal(5.25m, data.DecimalFromFractionValue);
        Assert.Equal('A', data.CharValue);
        Assert.Equal(new byte[] { 0, 255 }, data.ByteListValue);
    }

    [AndroidFact]
    public async Task reads_android_typed_model_with_nullable_numeric_values()
    {
        var sut = CrossFirebaseFirestore.Current;
        var document = GetTestingDocument(sut, "android-typed-model-nullable-numerics");
        await document.SetDataAsync(new Dictionary<object, object?> {
            { "nullable_byte_value", 200L },
            { "nullable_int_value", null },
            { "nullable_float_value", 1.5 },
            { "nullable_double_value", 2L }
        });

        var snapshot = await document.GetDocumentSnapshotAsync<NullableNumericDocument>();
        var data = snapshot.Data!;

        Assert.Equal((byte?) 200, data.NullableByteValue);
        Assert.Null(data.NullableIntValue);
        Assert.Equal((float?) 1.5f, data.NullableFloatValue);
        Assert.Equal(2.0, data.NullableDoubleValue);
        Assert.Null(data.AbsentValue);
    }

    [AndroidFact]
    public async Task reads_android_typed_model_with_enum_values()
    {
        var sut = CrossFirebaseFirestore.Current;
        var document = GetTestingDocument(sut, "android-typed-model-enum-values");
        await document.SetDataAsync(new Dictionary<object, object?> {
            { "poke_type_value", 4L },
            { "rating_value", 2L },
            { "rating_from_double_value", 2.0 }
        });

        var snapshot = await document.GetDocumentSnapshotAsync<EnumModelDocument>();
        var data = snapshot.Data!;

        Assert.Equal(PokeType.Electric, data.PokeTypeValue);
        Assert.Equal(NumericRating.High, data.RatingValue);
        Assert.Equal(NumericRating.High, data.RatingFromDoubleValue);
    }

    [AndroidFact]
    public async Task reads_android_typed_model_text_and_number_conversions_independent_of_culture()
    {
        var sut = CrossFirebaseFirestore.Current;
        var document = GetTestingDocument(sut, "android-typed-model-culture");
        await document.SetDataAsync(new Dictionary<object, object?> {
            { "double_from_text_value", "2.5" },
            { "text_from_double_value", 2.5 },
            { "texts_from_doubles_value", new[] { 2.5, 0.75 } }
        });

        var modelSnapshot = await document.GetDocumentSnapshotAsync<CultureConversionDocument>();
        var textSnapshot = await document.GetDocumentSnapshotAsync<Dictionary<string, string>>();

        var originalCulture = CultureInfo.CurrentCulture;
        // de-DE uses ',' as the decimal separator, so a culture-sensitive conversion would read "2.5" as 25
        CultureInfo.CurrentCulture = new CultureInfo("de-DE");
        try {
            // snapshot data is converted when it is read, so the conversions below run under de-DE
            var data = modelSnapshot.Data!;
            Assert.Equal(2.5, data.DoubleFromTextValue);
            Assert.Equal("2.5", data.TextFromDoubleValue);
            Assert.Equal(new[] { "2.5", "0.75" }, data.TextsFromDoublesValue);

            Assert.Equal("2.5", textSnapshot.Data!["text_from_double_value"]);
        }
        finally {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public async Task round_trips_typed_model_numeric_writes_at_boundaries()
    {
        var sut = CrossFirebaseFirestore.Current;
        var document = GetTestingDocument(sut, "typed-model-numeric-writes");
        var written = new NumericWidthsDocument(
            byteValue: 255,
            sbyteValue: -128,
            shortValue: -32768,
            ushortValue: 65535,
            intValue: 2147483647,
            uintValue: 4294967295,
            longValue: 9223372036854775807,
            ulongValue: 9223372036854775807,
            floatValue: 2.5f,
            wholeFloatValue: 2.0f,
            doubleValue: 2.25,
            wholeDoubleValue: 2.0,
            decimalFromWholeValue: 5m,
            decimalFromFractionValue: 19.99m,
            charValue: 'A',
            byteListValue: [0, 255]);
        await document.SetDataAsync(written);

        var snapshot = await document.GetDocumentSnapshotAsync<NumericWidthsDocument>();
        var data = snapshot.Data!;

        Assert.Equal(255, data.ByteValue);
        Assert.Equal(-128, data.SbyteValue);
        Assert.Equal(-32768, data.ShortValue);
        Assert.Equal(65535, data.UshortValue);
        Assert.Equal(2147483647, data.IntValue);
        Assert.Equal(4294967295U, data.UintValue);
        Assert.Equal(9223372036854775807L, data.LongValue);
        Assert.Equal(9223372036854775807UL, data.UlongValue);
        Assert.Equal(2.5f, data.FloatValue);
        Assert.Equal(2.0f, data.WholeFloatValue);
        Assert.Equal(2.25, data.DoubleValue);
        Assert.Equal(2.0, data.WholeDoubleValue);
        Assert.Equal(5m, data.DecimalFromWholeValue);
        // typed decimal reads on iOS depend on the numeric-read fix in PR #712
        Assert.Equal(19.99m, data.DecimalFromFractionValue);
        Assert.Equal('A', data.CharValue);
        Assert.Equal(new byte[] { 0, 255 }, data.ByteListValue);
    }

    [Fact]
    public async Task stores_numeric_writes_as_int64_and_double_on_the_wire()
    {
        var sut = CrossFirebaseFirestore.Current;
        var document = GetTestingDocument(sut, "numeric-write-wire-types");
        await document.SetDataAsync(new Dictionary<object, object?> {
            { "whole", (byte) 5 },
            { "fraction", 19.99m },
            { "enum_value", NumericRating.High },
            { "list", new object?[] { (byte) 5, "x" } }
        });

        var snapshot = await document.GetDocumentSnapshotAsync<Dictionary<string, object?>>();
        var data = snapshot.Data!;

        Assert.IsType<long>(data["whole"]);
        Assert.IsType<double>(data["fraction"]);
        Assert.IsType<long>(data["enum_value"]);
        var list = Assert.IsAssignableFrom<IList<object?>>(data["list"]);
        Assert.IsType<long>(list[0]);
        Assert.Equal("x", list[1]);
    }

    [Fact]
    public async Task round_trips_enum_writes_for_each_underlying_type()
    {
        var sut = CrossFirebaseFirestore.Current;
        var document = GetTestingDocument(sut, "typed-model-enum-widths");
        var written = new EnumWidthsDocument(
            byteEnum: NumericRating.High,
            sbyteEnum: SbyteRating.Negative,
            shortEnum: ShortRating.Negative,
            ushortEnum: UshortRating.High,
            intEnum: PokeType.Electric,
            uintEnum: UintRating.High,
            longEnum: LongRating.High,
            ulongEnum: UlongRating.High);
        await document.SetDataAsync(written);

        var snapshot = await document.GetDocumentSnapshotAsync<EnumWidthsDocument>();
        var data = snapshot.Data!;

        Assert.Equal(NumericRating.High, data.ByteEnum);
        Assert.Equal(SbyteRating.Negative, data.SbyteEnum);
        Assert.Equal(ShortRating.Negative, data.ShortEnum);
        Assert.Equal(UshortRating.High, data.UshortEnum);
        Assert.Equal(PokeType.Electric, data.IntEnum);
        Assert.Equal(UintRating.High, data.UintEnum);
        Assert.Equal(LongRating.High, data.LongEnum);
        Assert.Equal(UlongRating.High, data.UlongEnum);

        var raw = (await document.GetDocumentSnapshotAsync<Dictionary<string, object?>>()).Data!;
        Assert.IsType<long>(raw["byte_enum"]);
        Assert.IsType<long>(raw["ulong_enum"]);
    }

    [Fact]
    public async Task fails_writing_ulong_values_above_int64_max()
    {
        var sut = CrossFirebaseFirestore.Current;
        var document = GetTestingDocument(sut, "ulong-overflow-write");

        await Assert.ThrowsAnyAsync<ArgumentException>(
            () => document.SetDataAsync(
                new Dictionary<object, object?> { { "too_big", ulong.MaxValue } }
            )
        );

        var modelDocument = GetTestingDocument(sut, "ulong-overflow-model-write");
        var model = new NumericWidthsDocument(
            byteValue: 0,
            sbyteValue: 0,
            shortValue: 0,
            ushortValue: 0,
            intValue: 0,
            uintValue: 0,
            longValue: 0,
            ulongValue: ulong.MaxValue,
            floatValue: 0,
            wholeFloatValue: 0,
            doubleValue: 0,
            wholeDoubleValue: 0,
            decimalFromWholeValue: 0,
            decimalFromFractionValue: 0,
            charValue: 'a',
            byteListValue: null);
        await Assert.ThrowsAnyAsync<ArgumentException>(() => modelDocument.SetDataAsync(model));
    }

    [AndroidFact]
    public async Task rejects_out_of_range_values_in_android_typed_model_numeric_reads()
    {
        var sut = CrossFirebaseFirestore.Current;

        var byteDocument = GetTestingDocument(sut, "android-typed-model-out-of-range-byte");
        await byteDocument.SetDataAsync(new Dictionary<object, object?> {
            { "byte_value", 300L }
        });
        await Assert.ThrowsAsync<OverflowException>(async () => {
            var snapshot = await byteDocument.GetDocumentSnapshotAsync<NumericWidthsDocument>();
            _ = snapshot.Data;
        });

        var shortDocument = GetTestingDocument(sut, "android-typed-model-out-of-range-short");
        await shortDocument.SetDataAsync(new Dictionary<object, object?> {
            { "short_value", 40000L }
        });
        await Assert.ThrowsAsync<OverflowException>(async () => {
            var snapshot = await shortDocument.GetDocumentSnapshotAsync<NumericWidthsDocument>();
            _ = snapshot.Data;
        });

        var unsignedDocument = GetTestingDocument(sut, "android-typed-model-negative-unsigned");
        await unsignedDocument.SetDataAsync(new Dictionary<object, object?> {
            { "uint_value", -1L }
        });
        await Assert.ThrowsAsync<OverflowException>(async () => {
            var snapshot = await unsignedDocument.GetDocumentSnapshotAsync<NumericWidthsDocument>();
            _ = snapshot.Data;
        });
    }
}