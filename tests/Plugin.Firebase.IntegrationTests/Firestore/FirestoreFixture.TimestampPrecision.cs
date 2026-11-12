using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore;

public sealed partial class FirestoreFixture
{
    // Half a microsecond past a whole microsecond. Firestore keeps the microsecond and drops the rest.
    private const long SubMillisecondTicks = 1_234_565;

    [Fact]
    public async Task round_trips_sub_millisecond_datetime_offsets_with_non_utc_offsets()
    {
        var sut = CrossFirebaseFirestore.Current;
        // 12:00:00.1234565Z and 13:00:00.1234565Z. Reading the wall-clock ticks as UTC would put them hours away.
        var westOfUtc = new DateTimeOffset(2026, 1, 1, 7, 0, 0, TimeSpan.FromHours(-5)).AddTicks(SubMillisecondTicks);
        var eastOfUtc = new DateTimeOffset(2026, 1, 1, 18, 30, 0, new TimeSpan(5, 30, 0)).AddTicks(SubMillisecondTicks);
        var document = GetTestingDocument(sut, "sub-millisecond-non-utc-offsets");

        await document.SetDataAsync(new Dictionary<object, object?> {
            { "west", westOfUtc },
            { "east", eastOfUtc }
        });

        var typed = (await document.GetDocumentSnapshotAsync<Dictionary<string, DateTimeOffset>>(Source.Server)).Data!;
        FirestoreAssertions.SameInstantToTheMicrosecond(westOfUtc, typed["west"]);
        FirestoreAssertions.SameInstantToTheMicrosecond(eastOfUtc, typed["east"]);

        var raw = (await document.GetDocumentSnapshotAsync<Dictionary<string, object?>>(Source.Server)).Data!;
        FirestoreAssertions.SameInstantToTheMicrosecond(westOfUtc, Assert.IsType<DateTimeOffset>(raw["west"]));
        FirestoreAssertions.SameInstantToTheMicrosecond(eastOfUtc, Assert.IsType<DateTimeOffset>(raw["east"]));
    }

    [Fact]
    public async Task finds_a_document_by_the_sub_millisecond_timestamp_read_from_it()
    {
        var sut = CrossFirebaseFirestore.Current;
        var observedAt = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero).AddTicks(SubMillisecondTicks);
        var target = GetTestingDocument(sut, "sub-millisecond-equality-target");
        // Two microseconds later, in the same millisecond: it matches too if a conversion drops sub-millisecond ticks.
        var neighbour = GetTestingDocument(sut, "sub-millisecond-equality-neighbour");
        await target.SetDataAsync(new Dictionary<object, object?> {
            { "observed", observedAt }
        });
        await neighbour.SetDataAsync(new Dictionary<object, object?> {
            { "observed", observedAt.AddTicks(2 * IntegrationTestTimeouts.OneMicrosecondTicks) }
        });

        var read = (await target.GetDocumentSnapshotAsync<Dictionary<string, DateTimeOffset>>(Source.Server))
            .Data!["observed"];
        var byReadValue = await GetTestingCollection(sut)
            .WhereEqualsTo("observed", read)
            .GetDocumentsAsync<Dictionary<string, object?>>(Source.Server);
        var bySameInstantWithOffset = await GetTestingCollection(sut)
            .WhereEqualsTo("observed", read.ToOffset(TimeSpan.FromHours(-5)))
            .GetDocumentsAsync<Dictionary<string, object?>>(Source.Server);

        Assert.Equal(target.Id, Assert.Single(byReadValue.Documents).Reference.Id);
        Assert.Equal(target.Id, Assert.Single(bySameInstantWithOffset.Documents).Reference.Id);
    }

    [Fact]
    public async Task pages_past_the_sub_millisecond_timestamp_read_from_the_last_document()
    {
        var sut = CrossFirebaseFirestore.Current;
        var firstObservedAt = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero).AddTicks(SubMillisecondTicks);
        var first = GetTestingDocument(sut, "sub-millisecond-page-first");
        // Two microseconds later, in the same millisecond: a cursor without sub-millisecond ticks skips it or
        // returns the first document again.
        var second = GetTestingDocument(sut, "sub-millisecond-page-second");
        await first.SetDataAsync(new Dictionary<object, object?> {
            { "observed", firstObservedAt }
        });
        await second.SetDataAsync(new Dictionary<object, object?> {
            { "observed", firstObservedAt.AddTicks(2 * IntegrationTestTimeouts.OneMicrosecondTicks) }
        });

        var query = GetTestingCollection(sut).OrderBy("observed");
        var firstPage = await query
            .LimitedTo(1)
            .GetDocumentsAsync<Dictionary<string, DateTimeOffset>>(Source.Server);
        var lastOfFirstPage = Assert.Single(firstPage.Documents);
        var nextPage = await query
            .StartingAfter(lastOfFirstPage.Data!["observed"])
            .GetDocumentsAsync<Dictionary<string, DateTimeOffset>>(Source.Server);

        Assert.Equal(first.Id, lastOfFirstPage.Reference.Id);
        Assert.Equal(second.Id, Assert.Single(nextPage.Documents).Reference.Id);
    }

    [Fact]
    public async Task returns_sub_millisecond_date_results_from_transactions()
    {
        var sut = CrossFirebaseFirestore.Current;
        var expected = new DateTimeOffset(2026, 1, 1, 7, 0, 0, TimeSpan.FromHours(-5)).AddTicks(SubMillisecondTicks);

        // A transaction result never reaches Firestore, so nothing truncates it to the microsecond.
        var offsetResult = await sut.RunTransactionAsync(_ => expected);
        var dateTimeResult = await sut.RunTransactionAsync(_ => expected.UtcDateTime);

        Assert.Equal(expected.UtcTicks, offsetResult.UtcTicks);
        Assert.Equal(expected.UtcDateTime, dateTimeResult);
        Assert.Equal(DateTimeKind.Utc, dateTimeResult.Kind);
    }
}