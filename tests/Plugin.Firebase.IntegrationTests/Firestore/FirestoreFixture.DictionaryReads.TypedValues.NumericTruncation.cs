using System.Globalization;
using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore;

// Firestore only stores fractional numbers as doubles. Reading one into an integral or enum target
// truncates toward zero on both platforms, as the native Android SDK's CustomClassMapper does.
// Every value gets a document of its own, so each case is asserted independently.
public sealed partial class FirestoreFixture
{
    [Theory]
    [InlineData(2.7, 2L)]
    [InlineData(2.5, 2L)]
    [InlineData(3.5, 3L)]
    [InlineData(-2.7, -2L)]
    [InlineData(3.0, 3L)]
    public async Task truncates_fractional_values_read_into_long_properties(double stored, long expected)
    {
        var document = await WriteSingleNumberDocumentAsync(stored);

        var data = (await document.GetDocumentSnapshotAsync<TruncatedLongDocument>()).Data!;

        Assert.Equal(expected, data.Value);
    }

    [Theory]
    [InlineData(2.7, 2)]
    [InlineData(2.5, 2)]
    [InlineData(3.5, 3)]
    [InlineData(-2.7, -2)]
    [InlineData(3.0, 3)]
    public async Task truncates_fractional_values_read_into_int_properties(double stored, int expected)
    {
        var document = await WriteSingleNumberDocumentAsync(stored);

        var data = (await document.GetDocumentSnapshotAsync<TruncatedIntDocument>()).Data!;

        Assert.Equal(expected, data.Value);
    }

    [Theory]
    [InlineData(2.7, (byte) NumericRating.High)]
    [InlineData(2.5, (byte) NumericRating.High)]
    [InlineData(1.5, (byte) NumericRating.Low)]
    [InlineData(-0.5, (byte) NumericRating.Unrated)]
    [InlineData(2.0, (byte) NumericRating.High)]
    public async Task truncates_fractional_values_read_into_byte_enum_properties(double stored, byte expected)
    {
        var document = await WriteSingleNumberDocumentAsync(stored);

        var data = (await document.GetDocumentSnapshotAsync<TruncatedRatingDocument>()).Data!;

        Assert.Equal((NumericRating) expected, data.Value);
    }

    [Theory]
    [InlineData(2.7, 2L)]
    [InlineData(2.5, 2L)]
    [InlineData(3.5, 3L)]
    [InlineData(-2.7, -2L)]
    [InlineData(3.0, 3L)]
    public async Task truncates_fractional_values_read_into_long_dictionary_values(double stored, long expected)
    {
        var document = await WriteSingleNumberDocumentAsync(stored);

        var data = (await document.GetDocumentSnapshotAsync<Dictionary<string, long>>()).Data!;

        Assert.Equal(expected, data[SingleNumberDocument.Field]);
    }

    private async Task<IDocumentReference> WriteSingleNumberDocumentAsync(double stored)
    {
        var document = GetTestingDocument(
            CrossFirebaseFirestore.Current,
            $"numeric-truncation-{stored.ToString(CultureInfo.InvariantCulture)}");
        await document.SetDataAsync(new Dictionary<object, object?> {
            { SingleNumberDocument.Field, stored }
        });
        return document;
    }
}