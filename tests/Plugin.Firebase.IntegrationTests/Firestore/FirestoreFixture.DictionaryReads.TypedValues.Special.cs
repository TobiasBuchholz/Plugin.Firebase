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
}