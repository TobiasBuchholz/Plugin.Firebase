using System.Globalization;
using Plugin.Firebase.Firestore.Internal;

namespace Plugin.Firebase.UnitTests;

public class DateTimeKindGuardTests
{
    public static TheoryData<DateTime> UnspecifiedValues()
    {
        return new TheoryData<DateTime> {
            new DateTime(2026, 1, 1, 12, 0, 0),
            // Also DateTime.MinValue.
            default(DateTime),
            DateTime.MaxValue,
            // A string without an offset parses to an Unspecified value.
            DateTime.Parse("2026-02-03T04:05:06", CultureInfo.InvariantCulture),
            DateTime.SpecifyKind(new DateTime(2026, 3, 4, 5, 6, 7, DateTimeKind.Utc), DateTimeKind.Unspecified)
        };
    }

    public static TheoryData<DateTime> UtcAndLocalValues()
    {
        return new TheoryData<DateTime> {
            new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc).ToLocalTime(),
            DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc),
            DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc)
        };
    }

    [Theory]
    [MemberData(nameof(UnspecifiedValues))]
    public void rejects_unspecified_values(DateTime value)
    {
        var exception = Assert.Throws<ArgumentException>(() => DateTimeKindGuard.ThrowIfUnspecified(value));

        Assert.Equal("value", exception.ParamName);
    }

    [Theory]
    [MemberData(nameof(UtcAndLocalValues))]
    public void accepts_utc_and_local_values(DateTime value)
    {
        var exception = Record.Exception(() => DateTimeKindGuard.ThrowIfUnspecified(value));

        Assert.Null(exception);
    }

    [Fact]
    public void names_the_value_and_the_ways_to_fix_it()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => DateTimeKindGuard.ThrowIfUnspecified(new DateTime(2026, 1, 1, 12, 0, 0)));

        Assert.Contains("2026-01-01T12:00:00.0000000", exception.Message);
        Assert.Contains("DateTimeKind.Unspecified", exception.Message);
        Assert.Contains("DateTime.SpecifyKind(value, DateTimeKind.Utc)", exception.Message);
        Assert.Contains("DateTime.SpecifyKind(value, DateTimeKind.Local)", exception.Message);
        Assert.Contains("DateTimeOffset", exception.Message);
    }
}