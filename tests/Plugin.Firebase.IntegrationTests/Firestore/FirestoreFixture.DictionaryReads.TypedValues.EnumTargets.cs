using System.Globalization;
using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore;

// Nullable enum properties, enum list elements and enum dictionary values go through the same
// conversion as enum properties, so they are range checked against the underlying type as well.
public sealed partial class FirestoreFixture
{
    [Theory]
    [InlineData(2L)]
    [InlineData(255L)]
    public async Task reads_nullable_enum_values_through_the_underlying_type(long stored)
    {
        var data = await ReadEnumDocumentAsync<NullableRatingDocument>("nullable", stored);

        Assert.Equal(stored, (long?) data.Value);
    }

    [Theory]
    [InlineData(256L)]
    [InlineData(-1L)]
    public async Task rejects_nullable_enum_values_outside_the_underlying_type_range(long stored)
    {
        await AssertEnumOutOfRangeAsync<NullableRatingDocument>("nullable", stored);
    }

    [Fact]
    public async Task reads_enum_list_elements_through_the_underlying_type()
    {
        var sut = CrossFirebaseFirestore.Current;
        var document = GetTestingDocument(sut, "enum-list");
        await document.SetDataAsync(new Dictionary<object, object?> {
            { RatingListDocument.Field, new object[] { 0L, 2L, 255L, 2.7 } }
        });

        var data = (await document.GetDocumentSnapshotAsync<RatingListDocument>()).Data!;

        Assert.Equal([NumericRating.Unrated, NumericRating.High, (NumericRating) 255, NumericRating.High], data.Ratings);
    }

    [Theory]
    [InlineData(256L)]
    [InlineData(-1L)]
    public async Task rejects_enum_list_elements_outside_the_underlying_type_range(long stored)
    {
        var sut = CrossFirebaseFirestore.Current;
        var document = GetTestingDocument(sut, $"enum-list-{stored.ToString(CultureInfo.InvariantCulture)}");
        await document.SetDataAsync(new Dictionary<object, object?> {
            // one element outside the range fails the whole list, even with valid elements around it
            { RatingListDocument.Field, new[] { 1L, stored, 2L } }
        });

        var snapshot = await document.GetDocumentSnapshotAsync<RatingListDocument>();

        Assert.Throws<OverflowException>(() => _ = snapshot.Data);
    }

    [Fact]
    public async Task reads_enum_dictionary_values_through_the_underlying_type()
    {
        var sut = CrossFirebaseFirestore.Current;
        var document = GetTestingDocument(sut, "enum-dictionary");
        await document.SetDataAsync(new Dictionary<object, object?> {
            { "min", 0L },
            { "max", 255L },
            { "fractional", 2.7 }
        });

        var data = (await document.GetDocumentSnapshotAsync<Dictionary<string, NumericRating>>()).Data!;

        Assert.Equal(NumericRating.Unrated, data["min"]);
        Assert.Equal((NumericRating) 255, data["max"]);
        Assert.Equal(NumericRating.High, data["fractional"]);
    }

    [Theory]
    [InlineData(256L)]
    [InlineData(-1L)]
    public async Task rejects_enum_dictionary_values_outside_the_underlying_type_range(long stored)
    {
        // Reading the document as a dictionary converts its only field.
        await AssertEnumOutOfRangeAsync<Dictionary<string, NumericRating>>("dictionary", stored);
    }
}