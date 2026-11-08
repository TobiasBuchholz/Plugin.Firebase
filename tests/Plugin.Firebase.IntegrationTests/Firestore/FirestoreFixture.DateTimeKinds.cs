using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore;

// DateTime values the Kind check rejects. These tests pass in any device time zone: an Unspecified value must throw
// whatever the zone. In UTC both old behaviours stored Unspecified as the Utc instant, so only a throw tells the new
// behaviour apart there. Accepted values are in FirestoreFixture.DateTimeKinds.RoundTrips.cs.
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

    [Fact]
    public async Task rejects_unspecified_date_times_in_transaction_writes()
    {
        var sut = CrossFirebaseFirestore.Current;
        var document = GetTestingDocument(sut, "unspecified-kind-transaction");
        await document.SetDataAsync(new Dictionary<object, object?> {
            { DateTimeKindField, UtcObservedAt }
        });

        var updateFailure = await Record.ExceptionAsync(() => sut.RunTransactionAsync<string?>(transaction => {
            transaction.UpdateData(document, (DateTimeKindField, UnspecifiedObservedAt));
            return null;
        }));
        var setFailure = await Record.ExceptionAsync(() => sut.RunTransactionAsync<string?>(transaction => {
            transaction.SetData(document, new DateTimeKindDocument { Observed = UnspecifiedObservedAt });
            return null;
        }));

        // The conversion throws inside the update function, so the transaction fails like any other exception thrown
        // there: the ArgumentException is the inner exception, and nothing is committed.
        foreach(var failure in new[] { updateFailure, setFailure }) {
            Assert.NotNull(failure);
            var argumentException = Assert.IsType<ArgumentException>(failure.InnerException);
            Assert.Contains("DateTime.SpecifyKind", argumentException.Message);
        }
        var stored = (await document.GetDocumentSnapshotAsync<Dictionary<string, DateTimeOffset>>(Source.Server)).Data!;
        Assert.Equal(UtcObservedAt.Ticks, stored[DateTimeKindField].UtcTicks);
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