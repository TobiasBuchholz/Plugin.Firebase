using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore;

// These tests pass in any device time zone. A Local value is built from the same instant as the Utc one, and an
// Unspecified value must throw whatever the zone. In UTC both old behaviours stored Unspecified as the Utc instant, so
// only a throw tells the new behaviour apart there.
public sealed partial class FirestoreFixture
{
    private const string DateTimeKindField = DateTimeKindDocument.ObservedField;

    // 2026-01-01T12:00:00Z, and the same wall-clock time without a kind.
    private static readonly DateTime UtcObservedAt = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime UnspecifiedObservedAt = DateTime.SpecifyKind(UtcObservedAt, DateTimeKind.Unspecified);

    [Fact]
    public async Task rejects_unspecified_date_times_in_set_data()
    {
        var sut = CrossFirebaseFirestore.Current;
        var document = GetTestingDocument(sut, "unspecified-kind-set");
        var data = new Dictionary<object, object?> {
            { DateTimeKindField, UnspecifiedObservedAt }
        };

        await RejectsUnspecifiedDateTimeKindAsync(() => document.SetDataAsync(data));
        await RejectsUnspecifiedDateTimeKindAsync(() => document.SetDataAsync(data, SetOptions.Merge()));
        await RejectsUnspecifiedDateTimeKindAsync(() => document.SetDataAsync(
            new DateTimeKindDocument { Observed = UnspecifiedObservedAt }));
        await RejectsUnspecifiedDateTimeKindAsync(() => document.SetDataAsync(new Dictionary<object, object?> {
            { "observations", new List<object> { UtcObservedAt, UnspecifiedObservedAt } }
        }));
        RejectsUnspecifiedDateTimeKind(() => sut.CreateBatch().SetData(document, data));

        Assert.Null((await document.GetDocumentSnapshotAsync<Dictionary<string, object?>>(Source.Server)).Data);
    }

    [Fact]
    public async Task rejects_unspecified_date_times_in_update_data()
    {
        var sut = CrossFirebaseFirestore.Current;
        var document = GetTestingDocument(sut, "unspecified-kind-update");
        await document.SetDataAsync(new Dictionary<object, object?> {
            { DateTimeKindField, UtcObservedAt }
        });

        await RejectsUnspecifiedDateTimeKindAsync(() => document.UpdateDataAsync(
            (DateTimeKindField, UnspecifiedObservedAt)));
        await RejectsUnspecifiedDateTimeKindAsync(() => document.UpdateDataAsync(new Dictionary<object, object?> {
            { DateTimeKindField, UnspecifiedObservedAt }
        }));
        RejectsUnspecifiedDateTimeKind(() => sut.CreateBatch().UpdateData(
            document,
            (DateTimeKindField, UnspecifiedObservedAt)));

        var stored = (await document.GetDocumentSnapshotAsync<Dictionary<string, DateTimeOffset>>(Source.Server)).Data!;
        Assert.Equal(UtcObservedAt.Ticks, stored[DateTimeKindField].UtcTicks);
    }

    [Fact]
    public async Task rejects_unspecified_date_times_in_array_union_and_array_remove()
    {
        var sut = CrossFirebaseFirestore.Current;
        var document = GetTestingDocument(sut, "unspecified-kind-array-transforms");
        await document.SetDataAsync(new Dictionary<object, object?> {
            { "observations", new List<object> { UtcObservedAt } }
        });

        await RejectsUnspecifiedDateTimeKindAsync(() => document.UpdateDataAsync(
            ("observations", FieldValue.ArrayUnion(UnspecifiedObservedAt))));
        await RejectsUnspecifiedDateTimeKindAsync(() => document.UpdateDataAsync(
            ("observations", FieldValue.ArrayRemove(UnspecifiedObservedAt))));
    }

    [Fact]
    public void rejects_unspecified_date_times_as_query_operands_and_cursors()
    {
        var collection = GetTestingCollection(CrossFirebaseFirestore.Current);

        RejectsUnspecifiedDateTimeKind(() => collection.WhereEqualsTo(DateTimeKindField, UnspecifiedObservedAt));
        RejectsUnspecifiedDateTimeKind(() => collection.WhereFieldIn(
            DateTimeKindField,
            new object?[] { UtcObservedAt, UnspecifiedObservedAt }));
        RejectsUnspecifiedDateTimeKind(() => collection.OrderBy(DateTimeKindField).StartingAt(UnspecifiedObservedAt));
    }

    [Fact]
    public async Task rejects_default_and_min_value_date_times()
    {
        var sut = CrossFirebaseFirestore.Current;
        var document = GetTestingDocument(sut, "unspecified-kind-defaults");

        // A DateTime property that was never set holds default(DateTime): DateTime.MinValue with no kind.
        await RejectsUnspecifiedDateTimeKindAsync(() => document.SetDataAsync(new DateTimeKindDocument()));
        await RejectsUnspecifiedDateTimeKindAsync(() => document.SetDataAsync(new Dictionary<object, object?> {
            { DateTimeKindField, DateTime.MinValue }
        }));

        Assert.Null((await document.GetDocumentSnapshotAsync<Dictionary<string, object?>>(Source.Server)).Data);
    }

    // Android only. iOS accepts the value too, but an existing iOS bug then fails the write: see the next test.
    [AndroidFact]
    public async Task writes_min_value_date_times_with_utc_kind()
    {
        var sut = CrossFirebaseFirestore.Current;
        var document = GetTestingDocument(sut, "utc-kind-min-value");

        // The smallest instant a Firestore timestamp can hold.
        await document.SetDataAsync(new DateTimeKindDocument {
            Observed = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc)
        });

        var raw = (await document.GetDocumentSnapshotAsync<Dictionary<string, object?>>(Source.Server)).Data!;
        Assert.Equal(DateTimeOffset.MinValue, Assert.IsType<DateTimeOffset>(raw[DateTimeKindField]));
    }

    // Pins an existing iOS bug that the Kind check doesn't cause. Microsoft.iOS converts DateTime to NSDate through an
    // NSCalendar Gregorian calendar, which uses Julian dates before 1582-10-15. That moves DateTime.MinValue two days
    // earlier, below the smallest Firestore timestamp, so the native SDK rejects it. The native exception still shows
    // that the Kind check let the value through. Once the conversion is fixed this test fails: delete it and run the
    // test above on both platforms.
    [IosFact]
    public async Task utc_min_value_passes_the_kind_check_but_shifts_below_the_smallest_timestamp_on_ios()
    {
        var sut = CrossFirebaseFirestore.Current;
        var document = GetTestingDocument(sut, "utc-kind-min-value-ios");

        var exception = await Assert.ThrowsAnyAsync<Exception>(() => document.SetDataAsync(new DateTimeKindDocument {
            Observed = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc)
        }));

        Assert.IsNotType<ArgumentException>(exception);
        // 0001-01-01 read as a Julian date: 172,800 s (two days) before the minimum of -62,135,596,800 s.
        Assert.Contains("-62135769600", exception.Message);
    }

    [Fact]
    public async Task round_trips_utc_and_local_date_times_to_the_same_instant()
    {
        var sut = CrossFirebaseFirestore.Current;
        var local = UtcObservedAt.ToLocalTime();
        var document = GetTestingDocument(sut, "utc-and-local-kinds");

        await document.SetDataAsync(new Dictionary<object, object?> {
            { "utc", UtcObservedAt },
            { "local", local }
        });

        var raw = (await document.GetDocumentSnapshotAsync<Dictionary<string, object?>>(Source.Server)).Data!;
        Assert.Equal(UtcObservedAt.Ticks, Assert.IsType<DateTimeOffset>(raw["utc"]).UtcTicks);
        Assert.Equal(UtcObservedAt.Ticks, Assert.IsType<DateTimeOffset>(raw["local"]).UtcTicks);
        var typed = (await document.GetDocumentSnapshotAsync<Dictionary<string, DateTime>>(Source.Server)).Data!;
        Assert.Equal(DateTimeKind.Utc, typed["local"].Kind);
        Assert.Equal(UtcObservedAt, typed["local"]);
        var byLocalOperand = await GetTestingCollection(sut)
            .WhereEqualsTo("utc", local)
            .GetDocumentsAsync<Dictionary<string, object?>>(Source.Server);
        Assert.Equal(document.Id, Assert.Single(byLocalOperand.Documents).Reference.Id);
    }

    private static void RejectsUnspecifiedDateTimeKind(Func<object?> convert)
    {
        var exception = Assert.Throws<ArgumentException>(convert);
        Assert.Contains("DateTime.SpecifyKind", exception.Message);
    }

    private static async Task RejectsUnspecifiedDateTimeKindAsync(Func<Task> write)
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(write);
        Assert.Contains("DateTime.SpecifyKind", exception.Message);
    }
}