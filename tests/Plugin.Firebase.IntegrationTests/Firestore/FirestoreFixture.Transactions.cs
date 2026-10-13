using Plugin.Firebase.Core.Exceptions;
using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore;

public sealed partial class FirestoreFixture
{
    [Fact]
    public async Task surfaces_cross_platform_exception_when_transaction_commit_is_rejected()
    {
        var sut = CrossFirebaseFirestore.Current;
        var charmander = PokemonFactory.CreateCharmander();
        var document = GetTestingDocument(sut, "4");
        await document.SetDataAsync(charmander);

        // A field value over the 1 MiB document limit is rejected by the backend at commit time,
        // after the update function has already run.
        var oversized = new string('x', 1_048_789);
        var exception = await Record.ExceptionAsync(() => sut.RunTransactionAsync<object?>(transaction => {
            transaction.UpdateData(document, ("oversized", oversized));
            return null;
        }));

        var firestoreException = Assert.IsType<CrossPlatformFirebaseFirestoreException>(exception);
        Assert.NotNull(firestoreException.InnerException);
        Assert.Equal(
            charmander,
            (await document.GetDocumentSnapshotAsync<Pokemon>()).Data
        );
    }
}