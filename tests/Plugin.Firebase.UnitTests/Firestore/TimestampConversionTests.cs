using Plugin.Firebase.Firestore.Internal;

namespace Plugin.Firebase.UnitTests;

public class TimestampConversionTests
{
    // 2026-01-01T17:00:00Z
    private const long InstantUnixSeconds = 1_767_286_800;
    private const long SubMillisecondTicks = 1_234_567;
    private const int SubMillisecondNanoseconds = 123_456_700;

    private static readonly DateTimeOffset Instant = DateTimeOffset.FromUnixTimeSeconds(InstantUnixSeconds)
        .AddTicks(SubMillisecondTicks);

    public static TheoryData<int> UtcOffsetsInMinutes()
    {
        var offsets = new TheoryData<int>();
        for(var minutes = -14 * 60; minutes <= 14 * 60; minutes += 15) {
            offsets.Add(minutes);
        }
        return offsets;
    }

    [Theory]
    [MemberData(nameof(UtcOffsetsInMinutes))]
    public void converts_values_with_any_utc_offset_through_their_utc_instant(int offsetMinutes)
    {
        var value = Instant.ToOffset(TimeSpan.FromMinutes(offsetMinutes));

        var (seconds, nanoseconds) = TimestampConversion.ToUnixSecondsAndNanoseconds(value);

        Assert.Equal(InstantUnixSeconds, seconds);
        Assert.Equal(SubMillisecondNanoseconds, nanoseconds);
    }

    [Theory]
    [MemberData(nameof(UtcOffsetsInMinutes))]
    public void round_trips_values_with_any_utc_offset_to_the_same_instant(int offsetMinutes)
    {
        var value = Instant.ToOffset(TimeSpan.FromMinutes(offsetMinutes));

        var (seconds, nanoseconds) = TimestampConversion.ToUnixSecondsAndNanoseconds(value);
        var roundTripped = TimestampConversion.ToDateTimeOffset(seconds, nanoseconds);

        Assert.Equal(value.UtcTicks, roundTripped.UtcTicks);
        Assert.Equal(TimeSpan.Zero, roundTripped.Offset);
    }

    [Theory]
    [InlineData(0L, 0L, 0)]
    [InlineData(1L, 0L, 100)]
    [InlineData(-1L, -1L, 999_999_900)]
    [InlineData(-5_000_000L, -1L, 500_000_000)]
    [InlineData(-10_000_000L, -1L, 0)]
    [InlineData(-15_000_000L, -2L, 500_000_000)]
    [InlineData(-10_000_001L, -2L, 999_999_900)]
    public void floors_seconds_and_keeps_nanoseconds_positive_around_the_epoch(
        long ticksFromEpoch,
        long expectedSeconds,
        int expectedNanoseconds)
    {
        var value = DateTimeOffset.UnixEpoch.AddTicks(ticksFromEpoch);

        var (seconds, nanoseconds) = TimestampConversion.ToUnixSecondsAndNanoseconds(value);

        Assert.Equal(expectedSeconds, seconds);
        Assert.Equal(expectedNanoseconds, nanoseconds);
        Assert.Equal(value, TimestampConversion.ToDateTimeOffset(seconds, nanoseconds));
    }

    [Fact]
    public void converts_sub_millisecond_values_long_before_the_epoch()
    {
        // 1969-07-20T20:17:40.1234567-05:00 is 1969-07-21T01:17:40.1234567Z: Unix time -14,164,940 s plus 123,456,700 ns.
        var value = new DateTimeOffset(1969, 7, 20, 20, 17, 40, TimeSpan.FromHours(-5)).AddTicks(SubMillisecondTicks);

        var (seconds, nanoseconds) = TimestampConversion.ToUnixSecondsAndNanoseconds(value);

        Assert.Equal(-14_164_940L, seconds);
        Assert.Equal(SubMillisecondNanoseconds, nanoseconds);
        Assert.Equal(value.UtcTicks, TimestampConversion.ToDateTimeOffset(seconds, nanoseconds).UtcTicks);
    }

    [Fact]
    public void converts_the_smallest_and_largest_date_time_offsets()
    {
        var (minSeconds, minNanoseconds) = TimestampConversion.ToUnixSecondsAndNanoseconds(DateTimeOffset.MinValue);
        var (maxSeconds, maxNanoseconds) = TimestampConversion.ToUnixSecondsAndNanoseconds(DateTimeOffset.MaxValue);

        // These are also the bounds of a native Firestore Timestamp: 0001-01-01T00:00:00Z to 9999-12-31T23:59:59.999999999Z.
        Assert.Equal(-62_135_596_800L, minSeconds);
        Assert.Equal(0, minNanoseconds);
        Assert.Equal(253_402_300_799L, maxSeconds);
        Assert.Equal(999_999_900, maxNanoseconds);
        Assert.Equal(DateTimeOffset.MinValue, TimestampConversion.ToDateTimeOffset(minSeconds, minNanoseconds));
        Assert.Equal(DateTimeOffset.MaxValue, TimestampConversion.ToDateTimeOffset(maxSeconds, maxNanoseconds));
    }

    [Theory]
    // Seconds on the proleptic Gregorian calendar that .NET and Firestore share. The Julian/Gregorian hybrid calendar
    // that Microsoft.iOS converts NSDate through puts these dates up to ten days away, except the last one.
    [InlineData(1, 3, 1, -62_130_499_200L)]
    [InlineData(300, 3, 1, -52_695_014_400L)]
    [InlineData(1000, 1, 1, -30_610_224_000L)]
    [InlineData(1582, 10, 4, -12_220_243_200L)]
    [InlineData(1582, 10, 5, -12_220_156_800L)]
    [InlineData(1582, 10, 14, -12_219_379_200L)]
    [InlineData(1582, 10, 15, -12_219_292_800L)]
    public void converts_dates_before_the_gregorian_calendar_reform_on_the_proleptic_calendar(
        int year,
        int month,
        int day,
        long expectedSeconds)
    {
        var value = new DateTimeOffset(year, month, day, 0, 0, 0, TimeSpan.Zero);

        var (seconds, nanoseconds) = TimestampConversion.ToUnixSecondsAndNanoseconds(value);

        Assert.Equal(expectedSeconds, seconds);
        Assert.Equal(0, nanoseconds);
        Assert.Equal(value, TimestampConversion.ToDateTimeOffset(seconds, nanoseconds));
    }

    [Fact]
    public void converts_utc_date_times_through_their_instant()
    {
        var value = Instant.UtcDateTime;

        var (seconds, nanoseconds) = TimestampConversion.ToUnixSecondsAndNanoseconds(value);

        Assert.Equal(DateTimeKind.Utc, value.Kind);
        Assert.Equal(InstantUnixSeconds, seconds);
        Assert.Equal(SubMillisecondNanoseconds, nanoseconds);
    }

    [Fact]
    public void converts_local_date_times_through_their_utc_instant()
    {
        // Passes in any machine time zone: the local wall-clock time differs, but the instant does not.
        var value = Instant.UtcDateTime.ToLocalTime();

        var (seconds, nanoseconds) = TimestampConversion.ToUnixSecondsAndNanoseconds(value);

        Assert.Equal(DateTimeKind.Local, value.Kind);
        Assert.Equal(InstantUnixSeconds, seconds);
        Assert.Equal(SubMillisecondNanoseconds, nanoseconds);
    }

    public static TheoryData<DateTime> UnspecifiedDateTimes()
    {
        return new TheoryData<DateTime> {
            DateTime.SpecifyKind(Instant.UtcDateTime, DateTimeKind.Unspecified),
            // Also default(DateTime).
            DateTime.MinValue,
            DateTime.MaxValue
        };
    }

    [Theory]
    [MemberData(nameof(UnspecifiedDateTimes))]
    public void rejects_unspecified_date_times(DateTime value)
    {
        // Passes in any machine time zone. Reading these as local time would give a different instant per zone, and
        // throw ArgumentOutOfRangeException for DateTime.MinValue east of UTC and DateTime.MaxValue west of it.
        Assert.Equal(DateTimeKind.Unspecified, value.Kind);
        Assert.Throws<ArgumentException>(() => TimestampConversion.ToUnixSecondsAndNanoseconds(value));
    }

    [Fact]
    public void converts_the_smallest_and_largest_utc_date_times()
    {
        var (minSeconds, minNanoseconds) = TimestampConversion.ToUnixSecondsAndNanoseconds(
            DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc));
        var (maxSeconds, maxNanoseconds) = TimestampConversion.ToUnixSecondsAndNanoseconds(
            DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc));

        Assert.Equal(-62_135_596_800L, minSeconds);
        Assert.Equal(0, minNanoseconds);
        Assert.Equal(253_402_300_799L, maxSeconds);
        Assert.Equal(999_999_900, maxNanoseconds);
    }

    [Theory]
    [InlineData(0, 0L)]
    [InlineData(99, 0L)]
    [InlineData(100, 1L)]
    [InlineData(123_456_789, 1_234_567L)]
    [InlineData(999_999_999, 9_999_999L)]
    public void truncates_nanoseconds_below_one_tick(int nanoseconds, long expectedTicks)
    {
        var value = TimestampConversion.ToDateTimeOffset(InstantUnixSeconds, nanoseconds);

        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(InstantUnixSeconds).AddTicks(expectedTicks), value);
        Assert.Equal(TimeSpan.Zero, value.Offset);
    }

    [Fact]
    public void truncates_nanoseconds_below_one_tick_before_the_epoch()
    {
        // -1 s + 999,999,999 ns is 1 ns before the epoch. Dropping the 99 ns below a whole tick lands on -100 ns.
        var value = TimestampConversion.ToDateTimeOffset(-1, 999_999_999);

        Assert.Equal(DateTimeOffset.UnixEpoch.AddTicks(-1), value);
    }

    [Fact]
    public void round_trips_ticks_across_the_epoch_at_100_ns_granularity()
    {
        var start = DateTimeOffset.UnixEpoch.AddTicks(-TimeSpan.TicksPerSecond / 2).ToOffset(TimeSpan.FromHours(5.5));
        for(var tick = 0L; tick < TimeSpan.TicksPerSecond; tick += 7_919) {
            var value = start.AddTicks(tick);

            var (seconds, nanoseconds) = TimestampConversion.ToUnixSecondsAndNanoseconds(value);

            Assert.Equal(value.UtcTicks, TimestampConversion.ToDateTimeOffset(seconds, nanoseconds).UtcTicks);
        }
    }
}