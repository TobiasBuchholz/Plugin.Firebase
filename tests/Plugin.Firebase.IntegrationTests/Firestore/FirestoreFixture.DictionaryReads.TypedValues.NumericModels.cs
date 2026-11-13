using System.Globalization;
using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore;

public sealed partial class FirestoreFixture
{
    [Fact]
    public async Task reads_typed_model_with_full_width_numeric_values()
    {
        var sut = CrossFirebaseFirestore.Current;
        var document = GetTestingDocument(sut, "typed-model-numeric-widths");
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
        Assert.Equal(new byte[] { 0, 255 }, data.ByteListValue);
    }

    [Fact]
    public async Task reads_typed_model_with_nullable_numeric_values()
    {
        var sut = CrossFirebaseFirestore.Current;
        var document = GetTestingDocument(sut, "typed-model-nullable-numerics");
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

    [Fact]
    public async Task reads_typed_model_with_enum_values()
    {
        var sut = CrossFirebaseFirestore.Current;
        var document = GetTestingDocument(sut, "typed-model-enum-values");
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

    // Typed reads don't convert between text and numbers on either platform, so the result can't
    // depend on the device culture: under de-DE a culture-sensitive conversion would read "2.5" as 25
    // and write 2.5 as "2,5". Each conversion reads a document of its own.
    [Fact]
    public async Task rejects_typed_model_text_and_number_conversions_independent_of_culture()
    {
        var sut = CrossFirebaseFirestore.Current;
        var doubleFromText = GetTestingDocument(sut, "typed-model-culture-double-from-text");
        await doubleFromText.SetDataAsync(new Dictionary<object, object?> {
            { "double_from_text_value", "2.5" }
        });
        var textFromDouble = GetTestingDocument(sut, "typed-model-culture-text-from-double");
        await textFromDouble.SetDataAsync(new Dictionary<object, object?> {
            { "text_from_double_value", 2.5 }
        });
        var textsFromDoubles = GetTestingDocument(sut, "typed-model-culture-texts-from-doubles");
        await textsFromDoubles.SetDataAsync(new Dictionary<object, object?> {
            { "texts_from_doubles_value", new[] { 2.5, 0.75 } }
        });

        var doubleFromTextSnapshot = await doubleFromText.GetDocumentSnapshotAsync<CultureConversionDocument>();
        var textFromDoubleSnapshot = await textFromDouble.GetDocumentSnapshotAsync<CultureConversionDocument>();
        var textsFromDoublesSnapshot = await textsFromDoubles.GetDocumentSnapshotAsync<CultureConversionDocument>();
        var textDictionarySnapshot = await textFromDouble.GetDocumentSnapshotAsync<Dictionary<string, string>>();

        var originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("de-DE");
        try {
            // snapshot data is converted when it is read, so these reads run under de-DE
            Assert.Throws<InvalidCastException>(() => _ = doubleFromTextSnapshot.Data);
            Assert.Throws<InvalidCastException>(() => _ = textFromDoubleSnapshot.Data);
            Assert.Throws<InvalidCastException>(() => _ = textsFromDoublesSnapshot.Data);
            Assert.Throws<InvalidCastException>(() => _ = textDictionarySnapshot.Data);
        }
        finally {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }
}