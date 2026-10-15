using System.Globalization;
using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore;

// Enums are converted through their underlying type on both platforms. A stored value outside that
// type's range throws OverflowException instead of wrapping to an unrelated value, and every value
// inside it reads, whether or not it is a named member. Every value gets a document of its own, so
// each case is asserted independently.
public sealed partial class FirestoreFixture
{
    [Theory]
    [InlineData(256L)]
    [InlineData(300L)]
    [InlineData(-1L)]
    public async Task rejects_values_outside_the_range_of_byte_backed_enums(long stored)
    {
        // Without the range check, 300 reads as (EdgeByteRating) 44 and -1 as 255.
        await AssertEnumOutOfRangeAsync<EdgeByteRatingDocument>("byte", stored);
    }

    [Theory]
    [InlineData(2147483648L)]
    [InlineData(-2147483649L)]
    [InlineData(5000000000L)]
    public async Task rejects_values_outside_the_range_of_int_backed_enums(long stored)
    {
        await AssertEnumOutOfRangeAsync<EdgeIntRatingDocument>("int", stored);
    }

    [Theory]
    [InlineData(4294967296L)]
    [InlineData(-1L)]
    public async Task rejects_values_outside_the_range_of_uint_backed_enums(long stored)
    {
        // -1 is what a wrapping (int) cast writes for uint.MaxValue.
        await AssertEnumOutOfRangeAsync<EdgeUIntRatingDocument>("uint", stored);
    }

    // Firestore integers are signed 64-bit, so only a double can be outside a long-backed enum's
    // range. 2^63 is the first double above long.MaxValue, and -2^63 - 2048 the first below
    // long.MinValue.
    [Theory]
    [InlineData(9223372036854775808.0)]
    [InlineData(-9223372036854777856.0)]
    public async Task rejects_doubles_outside_the_range_of_long_backed_enums(double stored)
    {
        await AssertEnumOutOfRangeAsync<EdgeLongRatingDocument>("long", stored);
    }

    [Theory]
    [InlineData(-1L)]
    [InlineData(long.MinValue)]
    [InlineData(18446744073709551616.0)]
    public async Task rejects_values_outside_the_range_of_ulong_backed_enums(object stored)
    {
        // long.MinValue is what a wrapping (long) cast writes for 2^63.
        await AssertEnumOutOfRangeAsync<EdgeULongRatingDocument>("ulong", stored);
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(255L)]
    public async Task reads_byte_backed_enum_values_at_the_underlying_type_edges(long stored)
    {
        var data = await ReadEnumDocumentAsync<EdgeByteRatingDocument>("byte", stored);

        Assert.Equal(stored, (long) data.Value);
    }

    [Theory]
    [InlineData(-2147483648L)]
    [InlineData(2147483647L)]
    public async Task reads_int_backed_enum_values_at_the_underlying_type_edges(long stored)
    {
        var data = await ReadEnumDocumentAsync<EdgeIntRatingDocument>("int", stored);

        Assert.Equal(stored, (long) data.Value);
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(4294967295L)]
    public async Task reads_uint_backed_enum_values_at_the_underlying_type_edges(long stored)
    {
        var data = await ReadEnumDocumentAsync<EdgeUIntRatingDocument>("uint", stored);

        Assert.Equal(stored, (long) data.Value);
    }

    [Theory]
    [InlineData(long.MinValue)]
    [InlineData(long.MaxValue)]
    public async Task reads_long_backed_enum_values_at_the_underlying_type_edges(long stored)
    {
        var data = await ReadEnumDocumentAsync<EdgeLongRatingDocument>("long", stored);

        Assert.Equal(stored, (long) data.Value);
    }

    // long.MaxValue is the largest integer Firestore stores, so it is the top edge for a ulong.
    [Theory]
    [InlineData(0L)]
    [InlineData(long.MaxValue)]
    public async Task reads_ulong_backed_enum_values_at_the_underlying_type_edges(long stored)
    {
        var data = await ReadEnumDocumentAsync<EdgeULongRatingDocument>("ulong", stored);

        Assert.Equal(stored, (long) data.Value);
    }

    // A combination of [Flags] members isn't a named member, and still reads.
    [Theory]
    [InlineData(3L)] // Read | Write
    [InlineData(2147483649L)] // Admin | Read, above int.MaxValue
    [InlineData(4294967295L)] // All
    public async Task reads_flags_enum_combinations_across_the_underlying_type(long stored)
    {
        var data = await ReadEnumDocumentAsync<UIntPermissionsDocument>("flags", stored);

        Assert.Equal(stored, (long) data.Value);
    }

    private async Task<T> ReadEnumDocumentAsync<T>(string name, object stored)
    {
        var document = await WriteEnumDocumentAsync(name, stored);
        return (await document.GetDocumentSnapshotAsync<T>()).Data!;
    }

    private async Task AssertEnumOutOfRangeAsync<T>(string name, object stored)
    {
        var document = await WriteEnumDocumentAsync(name, stored);
        var snapshot = await document.GetDocumentSnapshotAsync<T>();
        Assert.Throws<OverflowException>(() => _ = snapshot.Data);
    }

    private async Task<IDocumentReference> WriteEnumDocumentAsync(string name, object stored)
    {
        var document = GetTestingDocument(
            CrossFirebaseFirestore.Current,
            $"enum-{name}-{Convert.ToString(stored, CultureInfo.InvariantCulture)}");
        await document.SetDataAsync(new Dictionary<object, object?> {
            { SingleNumberDocument.Field, stored }
        });
        return document;
    }
}