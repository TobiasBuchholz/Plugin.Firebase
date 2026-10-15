using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore;

// .NET dates and Firestore timestamps both use the proleptic Gregorian calendar. iOS used to convert dates through
// NSDate and an NSCalendar that switches to Julian dates before 1582-10-15. It stored such dates up to ten days away
// from their instant, read them back shifted the other way, and couldn't write DateTime.MinValue at all.
public sealed partial class FirestoreFixture
{
    // Whole microseconds, so Firestore stores each value exactly.
    private static readonly DateTimeOffset[] DatesBeforeGregorianReform = [
        // iOS stored these 2 days early, 1 day late, 5 days late and 10 days late. Reading the correct instants of
        // 0300-03-01 and 1500-03-10, it got Feb 29 of years that DateTime treats as common years, and threw.
        new(1, 3, 1, 0, 0, 0, TimeSpan.Zero),
        new(300, 3, 1, 0, 0, 0, TimeSpan.Zero),
        new DateTimeOffset(1000, 1, 1, 12, 34, 56, TimeSpan.Zero).AddTicks(7_890_120),
        new(1500, 3, 10, 0, 0, 0, TimeSpan.Zero),
        // The last Julian day, which iOS stored 10 days late.
        new DateTimeOffset(1582, 10, 4, 23, 59, 59, TimeSpan.Zero).AddTicks(9_999_990),
        // The ten days the calendar reform skipped. iOS stored them 10 days late and read them back as 1582-10-15 to
        // 1582-10-24, so these fail even when the write and read shifts cancel out for the dates above.
        ..Enumerable.Range(5, 10).Select(day => new DateTimeOffset(1582, 10, day, 0, 0, 0, TimeSpan.Zero)),
        // The first Gregorian day, which iOS always converted correctly.
        new(1582, 10, 15, 0, 0, 0, TimeSpan.Zero)
    ];

    [Fact]
    public async Task round_trips_dates_before_the_gregorian_calendar_reform_to_the_same_instant()
    {
        var sut = CrossFirebaseFirestore.Current;
        var document = GetTestingDocument(sut, "dates-before-gregorian-reform");
        var data = new Dictionary<object, object?>();
        for(var i = 0; i < DatesBeforeGregorianReform.Length; i++) {
            data[$"offset{i}"] = DatesBeforeGregorianReform[i];
            data[$"utc{i}"] = DatesBeforeGregorianReform[i].UtcDateTime;
        }

        await document.SetDataAsync(data);

        var raw = (await document.GetDocumentSnapshotAsync<Dictionary<string, object?>>(Source.Server)).Data!;
        var typed = (await document.GetDocumentSnapshotAsync<Dictionary<string, DateTime>>(Source.Server)).Data!;
        for(var i = 0; i < DatesBeforeGregorianReform.Length; i++) {
            var expected = DatesBeforeGregorianReform[i];
            Assert.Equal(expected, Assert.IsType<DateTimeOffset>(raw[$"offset{i}"]));
            Assert.Equal(expected, Assert.IsType<DateTimeOffset>(raw[$"utc{i}"]));
            Assert.Equal(expected.UtcDateTime, typed[$"offset{i}"]);
            Assert.Equal(expected.UtcDateTime, typed[$"utc{i}"]);
        }
    }

    [Fact]
    public async Task reads_the_smallest_timestamp_as_min_value()
    {
        var sut = CrossFirebaseFirestore.Current;
        var document = GetTestingDocument(sut, "smallest-timestamp");

        // 0001-01-01T00:00:00Z, the smallest instant a Firestore timestamp can hold, as a backend writes it for
        // DateTime.MinValue. iOS used to fail this write, and read the stored instant as 0001-01-03.
        await document.SetDataAsync(new Dictionary<object, object?> {
            { DateTimeKindDocument.ObservedField, DateTimeOffset.MinValue }
        });

        var raw = (await document.GetDocumentSnapshotAsync<Dictionary<string, object?>>(Source.Server)).Data!;
        Assert.Equal(DateTimeOffset.MinValue, Assert.IsType<DateTimeOffset>(raw[DateTimeKindDocument.ObservedField]));
        var typed = (await document.GetDocumentSnapshotAsync<DateTimeKindDocument>(Source.Server)).Data!;
        Assert.Equal(DateTime.MinValue, typed.Observed);
        // An unset property also holds DateTime.MinValue, but with no kind.
        Assert.Equal(DateTimeKind.Utc, typed.Observed.Kind);
    }
}