using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore;

public sealed partial class FirestoreFixture
{
    [Fact]
    public async Task gets_document_data_as_typed_special_value_dictionaries()
    {
        var sut = CrossFirebaseFirestore.Current;
        var expectedDateTime = new DateTime(2026, 4, 29, 1, 2, 3, 456, DateTimeKind.Utc);
        var expectedOffset = new DateTimeOffset(2026, 4, 29, 4, 5, 6, 789, TimeSpan.Zero);
        var documentReference = GetTestingDocument(sut, "typed-reference-target");

        var dateTimeDocument = GetTestingDocument(sut, "typed-datetime-map");
        await dateTimeDocument.SetDataAsync(new Dictionary<object, object?> {
            { "created", expectedDateTime }
        });
        var dateTimes = (await dateTimeDocument.GetDocumentSnapshotAsync<Dictionary<string, DateTime>>()).Data!;
        Assert.InRange(
            Math.Abs(dateTimes["created"].Ticks - expectedDateTime.Ticks),
            0,
            IntegrationTestTimeouts.OneMillisecondTicks);

        var dateTimeOffsetDocument = GetTestingDocument(sut, "typed-datetime-offset-map");
        await dateTimeOffsetDocument.SetDataAsync(new Dictionary<object, object?> {
            { "observed", expectedOffset },
            { "generated", FieldValue.ServerTimestamp() }
        });
        var dateTimeOffsets = (await dateTimeOffsetDocument.GetDocumentSnapshotAsync<Dictionary<string, DateTimeOffset>>()).Data!;
        Assert.InRange(
            Math.Abs(dateTimeOffsets["observed"].Ticks - expectedOffset.Ticks),
            0,
            IntegrationTestTimeouts.OneMillisecondTicks);
        Assert.NotEqual(default, dateTimeOffsets["generated"]);

        var referenceDocument = GetTestingDocument(sut, "typed-reference-map");
        await referenceDocument.SetDataAsync(new Dictionary<object, object?> {
            { "original", documentReference }
        });
        var references = (await referenceDocument.GetDocumentSnapshotAsync<Dictionary<string, IDocumentReference>>()).Data!;
        Assert.Equal(documentReference.Path, references["original"].Path);
    }

    [Fact]
    public async Task round_trips_datetime_offsets_with_non_zero_utc_offsets()
    {
        var sut = CrossFirebaseFirestore.Current;
        // 2026-01-01 12:00:00 -05:00 is the instant 17:00:00 UTC; a discarded offset would store 12:00 UTC instead.
        var expected = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.FromHours(-5));
        // The bounds use a zero offset so a discarded offset on the stored value cannot shift them by the same amount.
        var lowerBound = new DateTimeOffset(2026, 1, 1, 16, 59, 0, TimeSpan.Zero);
        var upperBound = new DateTimeOffset(2026, 1, 1, 17, 1, 0, TimeSpan.Zero);
        var document = GetTestingDocument(sut, "typed-datetime-offset-non-zero-offset");

        await document.SetDataAsync(new Dictionary<object, object?> {
            { "observed", expected }
        });

        var typed = (await document.GetDocumentSnapshotAsync<Dictionary<string, DateTimeOffset>>()).Data!;
        FirestoreAssertions.SameInstant(expected, typed["observed"]);

        var raw = (await document.GetDocumentSnapshotAsync<Dictionary<string, object?>>()).Data!;
        FirestoreAssertions.SameInstant(expected, Assert.IsType<DateTimeOffset>(raw["observed"]));

        var windowSnapshot = await GetTestingCollection(sut)
            .WhereGreaterThan("observed", lowerBound)
            .WhereLessThan("observed", upperBound)
            .GetDocumentsAsync<Dictionary<string, object?>>();
        var match = Assert.Single(windowSnapshot.Documents);
        Assert.Equal(document.Id, match.Reference.Id);
    }
}