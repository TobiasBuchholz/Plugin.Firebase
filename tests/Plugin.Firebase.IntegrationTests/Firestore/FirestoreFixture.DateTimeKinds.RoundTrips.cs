using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore;

// DateTime values the Kind check accepts. These pass in any device time zone: each Local value is built from the same
// instant as the Utc one it is compared with.
public sealed partial class FirestoreFixture
{
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

    // Pins an existing iOS bug that the Kind check doesn't cause (#761). Microsoft.iOS converts DateTime to NSDate
    // through an NSCalendar Gregorian calendar, which uses Julian dates before 1582-10-15. That moves DateTime.MinValue
    // two days earlier, below the smallest Firestore timestamp, so the native SDK rejects it. The native exception still
    // shows that the Kind check let the value through. #762 fixes the conversion and removes this test, so the test
    // above runs on both platforms.
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

    // Nullable properties are the documented way to store "not set" without writing default(DateTime).
    [Fact]
    public async Task round_trips_nullable_date_time_properties_set_and_unset()
    {
        var sut = CrossFirebaseFirestore.Current;
        var set = GetTestingDocument(sut, "nullable-kind-set");
        var unset = GetTestingDocument(sut, "nullable-kind-unset");
        var missing = GetTestingDocument(sut, "nullable-kind-missing");

        await set.SetDataAsync(new NullableDateTimeDocument {
            Observed = UtcObservedAt.ToLocalTime(),
            ObservedOffset = new DateTimeOffset(UtcObservedAt).ToOffset(new TimeSpan(5, 30, 0))
        });
        await unset.SetDataAsync(new NullableDateTimeDocument());
        await missing.SetDataAsync(new Dictionary<object, object?> {
            { "unrelated", 1L }
        });

        var readSet = (await set.GetDocumentSnapshotAsync<NullableDateTimeDocument>(Source.Server)).Data!;
        Assert.Equal(UtcObservedAt, readSet.Observed);
        Assert.Equal(DateTimeKind.Utc, readSet.Observed!.Value.Kind);
        Assert.Equal(UtcObservedAt.Ticks, readSet.ObservedOffset!.Value.UtcTicks);
        var typedSet = (await set.GetDocumentSnapshotAsync<Dictionary<string, DateTime?>>(Source.Server)).Data!;
        Assert.Equal(UtcObservedAt, typedSet[NullableDateTimeDocument.ObservedField]);

        var rawUnset = (await unset.GetDocumentSnapshotAsync<Dictionary<string, object?>>(Source.Server)).Data!;
        Assert.Null(rawUnset[NullableDateTimeDocument.ObservedField]);
        Assert.Null(rawUnset[NullableDateTimeDocument.ObservedOffsetField]);
        foreach(var document in new[] { unset, missing }) {
            var read = (await document.GetDocumentSnapshotAsync<NullableDateTimeDocument>(Source.Server)).Data!;
            Assert.Null(read.Observed);
            Assert.Null(read.ObservedOffset);
        }
    }
}