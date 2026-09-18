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
            { "char_value", 65L }
        });

        var snapshot = await document.GetDocumentSnapshotAsync<NumericWidthsDocument>();

        Assert.Equal((byte) 255, snapshot.Data!.ByteValue);
        Assert.Equal((sbyte) -128, snapshot.Data.SbyteValue);
        Assert.Equal((short) -32768, snapshot.Data.ShortValue);
        Assert.Equal((ushort) 65535, snapshot.Data.UshortValue);
        Assert.Equal(2147483647, snapshot.Data.IntValue);
        Assert.Equal(4294967295U, snapshot.Data.UintValue);
        Assert.Equal(9223372036854775807L, snapshot.Data.LongValue);
        Assert.Equal(9223372036854775807UL, snapshot.Data.UlongValue);
        Assert.Equal(2.5f, snapshot.Data.FloatValue);
        Assert.Equal(2.0f, snapshot.Data.WholeFloatValue);
        Assert.Equal(2.25, snapshot.Data.DoubleValue);
        Assert.Equal(2.0, snapshot.Data.WholeDoubleValue);
        Assert.Equal(5m, snapshot.Data.DecimalFromWholeValue);
        Assert.Equal(5.25m, snapshot.Data.DecimalFromFractionValue);
        Assert.Equal('A', snapshot.Data.CharValue);
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

        Assert.Equal((byte?) 200, snapshot.Data!.NullableByteValue);
        Assert.Null(snapshot.Data.NullableIntValue);
        Assert.Equal((float?) 1.5f, snapshot.Data.NullableFloatValue);
        Assert.Equal(2.0, snapshot.Data.NullableDoubleValue);
        Assert.Null(snapshot.Data.AbsentValue);
    }

    [AndroidFact]
    public async Task reads_android_typed_model_with_enum_values()
    {
        var sut = CrossFirebaseFirestore.Current;
        var document = GetTestingDocument(sut, "android-typed-model-enum-values");
        await document.SetDataAsync(new Dictionary<object, object?> {
            { "poke_type_value", 4L },
            { "rating_value", 2L }
        });

        var snapshot = await document.GetDocumentSnapshotAsync<EnumModelDocument>();

        Assert.Equal(PokeType.Electric, snapshot.Data!.PokeTypeValue);
        Assert.Equal(NumericRating.High, snapshot.Data.RatingValue);
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
    }
}