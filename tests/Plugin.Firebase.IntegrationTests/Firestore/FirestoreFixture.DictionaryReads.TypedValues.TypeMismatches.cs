using System.Globalization;
using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore;

// Like the native Android SDK, a typed read only converts between numeric types. Any other mismatch
// between the stored value and the target type throws InvalidCastException on both platforms,
// instead of converting text or leaving the property at its default value. Every case writes a
// document of its own, so each one is asserted independently.
public sealed partial class FirestoreFixture
{
    [Theory]
    [InlineData(123L)]
    [InlineData(2.5)]
    public async Task throws_when_a_number_is_read_into_a_string_property(object stored)
    {
        // iOS used to leave the property null, and Android turned the number into text.
        await AssertTypeMismatchAsync<ValueDocument<string>>($"number-to-string-{Invariant(stored)}", stored);
    }

    [Fact]
    public async Task throws_when_a_bool_is_read_into_a_string_property()
    {
        await AssertTypeMismatchAsync<ValueDocument<string>>("bool-to-string", true);
    }

    [Fact]
    public async Task throws_when_text_is_read_into_an_int_property()
    {
        await AssertTypeMismatchAsync<ValueDocument<int>>("text-to-int", "123");
    }

    [Theory]
    [InlineData("2")]
    [InlineData("High")]
    public async Task throws_when_text_is_read_into_an_enum_property(string stored)
    {
        await AssertTypeMismatchAsync<ValueDocument<NumericRating>>($"text-to-enum-{stored}", stored);
    }

    [Fact]
    public async Task throws_when_a_bool_is_read_into_an_int_property()
    {
        await AssertTypeMismatchAsync<ValueDocument<int>>("bool-to-int", true);
    }

    [Theory]
    [InlineData(1L)]
    [InlineData(0L)]
    public async Task throws_when_a_number_is_read_into_a_bool_property(long stored)
    {
        await AssertTypeMismatchAsync<ValueDocument<bool>>($"number-to-bool-{Invariant(stored)}", stored);
    }

    [Fact]
    public async Task throws_when_a_number_is_read_into_a_datetime_property()
    {
        // milliseconds since the Unix epoch, a common way to store a date as a number
        await AssertTypeMismatchAsync<ValueDocument<DateTime>>("number-to-datetime", 1767225600000L);
    }

    [Fact]
    public async Task throws_when_text_is_read_into_a_datetime_property()
    {
        await AssertTypeMismatchAsync<ValueDocument<DateTime>>("text-to-datetime", "2026-01-01T00:00:00Z");
    }

    [Theory]
    [InlineData(65L)]
    [InlineData(70000L)]
    [InlineData(65.5)]
    public async Task throws_when_a_number_is_read_into_a_char_property(object stored)
    {
        // char isn't a Firestore type, so no value reads into one, whether or not it fits.
        await AssertTypeMismatchAsync<ValueDocument<char>>($"number-to-char-{Invariant(stored)}", stored);
    }

    [Fact]
    public async Task throws_when_text_is_read_into_a_char_property()
    {
        await AssertTypeMismatchAsync<ValueDocument<char>>("text-to-char", "A");
    }

    [Fact]
    public async Task throws_when_a_map_is_read_into_an_int_property()
    {
        // without the check the property was left at 0
        await AssertTypeMismatchAsync<ValueDocument<int>>(
            "map-to-int",
            new Dictionary<string, object> { { "count", 1L } });
    }

    [Fact]
    public async Task throws_when_an_array_is_read_into_a_string_property()
    {
        await AssertTypeMismatchAsync<ValueDocument<string>>("array-to-string", new[] { "a", "b" });
    }

    [Fact]
    public async Task throws_when_a_number_is_read_into_a_string_dictionary_value()
    {
        await AssertTypeMismatchAsync<Dictionary<string, string>>("number-to-string-dictionary", 123L);
    }

    [Fact]
    public async Task throws_when_text_is_read_into_an_int_dictionary_value()
    {
        await AssertTypeMismatchAsync<Dictionary<string, int>>("text-to-int-dictionary", "123");
    }

    [Fact]
    public async Task throws_when_numbers_are_read_into_a_string_list()
    {
        await AssertTypeMismatchAsync<ValueDocument<IList<string>>>("numbers-to-string-list", new[] { 1L, 2L });
    }

    private async Task AssertTypeMismatchAsync<T>(string name, object stored)
    {
        var document = GetTestingDocument(CrossFirebaseFirestore.Current, $"type-mismatch-{name}");
        await document.SetDataAsync(new Dictionary<object, object?> {
            { ValueDocument.Field, stored }
        });

        var snapshot = await document.GetDocumentSnapshotAsync<T>();

        var exception = Assert.Throws<InvalidCastException>(() => _ = snapshot.Data);
        // the message names the field, so the value can be found in the document
        Assert.Contains($"'{ValueDocument.Field}'", exception.Message);
    }

    private static string Invariant(object stored)
    {
        return Convert.ToString(stored, CultureInfo.InvariantCulture)!;
    }
}