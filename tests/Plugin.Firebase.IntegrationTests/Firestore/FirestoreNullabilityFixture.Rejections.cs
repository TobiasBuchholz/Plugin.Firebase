using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore;

public sealed partial class FirestoreNullabilityFixture
{
    [Fact]
    public async Task rejects_required_api_arguments_when_null()
    {
        var sut = CrossFirebaseFirestore.Current;
        var collection = GetTestingCollection(sut);
        var document = GetTestingDocument(sut, "required-null-rejection");
        await document.SetDataAsync(NullableFirestoreItemFactory.CreateNonNullItem("required-null-rejection"));

        AssertRejects(() => sut.GetDocument(RequiredNull<string>()));
        AssertRejects(() => sut.GetCollection(RequiredNull<string>()));
        AssertRejects(() => collection.GetDocument(RequiredNull<string>()));
        AssertRejects(() => document.GetCollection(RequiredNull<string>()));
        AssertRejects(() => collection.WhereEqualsTo(RequiredNull<string>(), "value"));
        AssertRejects(() => collection.OrderBy(RequiredNull<string>()));
        AssertRejects(() => collection.WhereFieldIn(NullableFirestoreItem.QueryMarkerField, RequiredNull<object?[]>()));
        AssertRejects(() => collection.StartingAt(RequiredNull<object?[]>()));
        AssertRejects(
            () => document.AddSnapshotListener(
                RequiredNull<Action<IDocumentSnapshot<NullableFirestoreItem>>>()
            )
        );
        AssertRejects(
            () => collection.AddSnapshotListener(
                RequiredNull<Action<IQuerySnapshot<NullableFirestoreItem>>>()
            )
        );

        await AssertRejectsAsync(() => collection.AddDocumentAsync(RequiredNull<NullableFirestoreItem>()));
        await AssertRejectsAsync(() => document.SetDataAsync(RequiredNull<NullableFirestoreItem>()));
        await AssertRejectsAsync(() => document.SetDataAsync(RequiredNull<Dictionary<object, object?>>()));
        await AssertRejectsAsync(() => document.UpdateDataAsync(RequiredNull<Dictionary<object, object?>>()));

        var batch = sut.CreateBatch();
        AssertRejects(() => batch.SetData(RequiredNull<IDocumentReference>(), NullableFirestoreItemFactory.CreateNonNullItem("batch")));
        AssertRejects(() => batch.SetData(document, RequiredNull<Dictionary<object, object?>>()));
        AssertRejects(() => batch.UpdateData(document, RequiredNull<Dictionary<object, object?>>()));

        await AssertRejectsAsync(() => sut.RunTransactionAsync<string?>(transaction => {
            transaction.SetData(RequiredNull<IDocumentReference>(), NullableFirestoreItemFactory.CreateNonNullItem("transaction"));
            return "unreachable";
        }));
    }
}