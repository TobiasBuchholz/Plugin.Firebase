using Plugin.Firebase.Core.Exceptions;
using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore;

public sealed partial class FirestoreFixture
{
    [Fact]
    public async Task runs_transaction()
    {
        var sut = CrossFirebaseFirestore.Current;
        var bulbasur = PokemonFactory.CreateBulbasur();
        var charmander = PokemonFactory.CreateCharmander();
        var squirtle = PokemonFactory.CreateSquirtle();
        var documentBulbasur = GetTestingDocument(sut, "1");
        var documentCharmander = GetTestingDocument(sut, "4");
        var documentSquirtle = GetTestingDocument(sut, "7");
        var otherMoves = new[] { "other_move", "another_move" };
        await documentBulbasur.SetDataAsync(bulbasur);
        await documentCharmander.SetDataAsync(charmander);

        var charmanderSightingCount = await sut.RunTransactionAsync(transaction => {
            var snapshotCharmander = transaction.GetDocument<Pokemon>(documentCharmander)!;
            var newSightingCount = snapshotCharmander.Data!.SightingCount + 1;
            transaction.SetData(documentSquirtle, squirtle);
            transaction.UpdateData(documentCharmander, (Pokemon.SightingCountField, newSightingCount));
            transaction.UpdateData(documentCharmander, (Pokemon.MovesField, otherMoves));
            transaction.UpdateData(documentCharmander, (Pokemon.ItemsField, FieldValue.Delete()));
            transaction.DeleteDocument(documentBulbasur);
            return newSightingCount;
        });

        var charmanderSnapshot = await documentCharmander.GetDocumentSnapshotAsync<Pokemon>();
        Assert.Equal(squirtle, (await documentSquirtle.GetDocumentSnapshotAsync<Pokemon>()).Data!);
        Assert.Equal(charmander.SightingCount + 1, charmanderSightingCount);
        Assert.Equal(otherMoves, charmanderSnapshot.Data!.Moves);
        Assert.Null(charmanderSnapshot.Data!.Items);
        Assert.Null((await documentBulbasur.GetDocumentSnapshotAsync<Pokemon>()).Data);
    }

    [Fact]
    public async Task does_not_commit_transaction_when_update_function_throws()
    {
        var sut = CrossFirebaseFirestore.Current;
        var charmander = PokemonFactory.CreateCharmander();
        var document = GetTestingDocument(sut, "4");
        await document.SetDataAsync(charmander);

        var exception = await Record.ExceptionAsync(() => sut.RunTransactionAsync<string?>(transaction => {
            transaction.UpdateData(document, (Pokemon.SightingCountField, charmander.SightingCount + 100));
            throw new InvalidOperationException("transaction aborted by update function");
        }));

        // Both platforms surface a FirebaseException that keeps the exception thrown by the update
        // function as the inner exception.
        var firebaseException = Assert.IsType<FirebaseException>(exception);
        var original = Assert.IsType<InvalidOperationException>(firebaseException.InnerException);
        Assert.Equal("transaction aborted by update function", original.Message);
        Assert.Equal(charmander, (await document.GetDocumentSnapshotAsync<Pokemon>()).Data);
    }

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

        FirestoreErrorAssertions.InvalidArgumentFailure(exception);
        Assert.Equal(
            charmander,
            (await document.GetDocumentSnapshotAsync<Pokemon>()).Data
        );
    }

    [Fact]
    public async Task surfaces_cross_platform_exception_when_transactional_read_fails()
    {
        var sut = CrossFirebaseFirestore.Current;
        var charmander = PokemonFactory.CreateCharmander();
        var document = GetTestingDocument(sut, "4");
        await document.SetDataAsync(charmander);

        // The native SDKs reject a transactional read after a write locally with INVALID_ARGUMENT, which
        // their transaction retry rules treat as permanent, so the update function runs only once.
        Exception? readException = null;
        var attempts = 0;
        var exception = await Record.ExceptionAsync(() => sut.RunTransactionAsync<object?>(transaction => {
            attempts++;
            transaction.UpdateData(document, (Pokemon.SightingCountField, charmander.SightingCount + 1));
            try {
                transaction.GetDocument<Pokemon>(document);
            } catch(Exception e) {
                readException = e;
                throw;
            }
            return null;
        }));

        Assert.Equal(1, attempts);
        FirestoreErrorAssertions.InvalidArgumentFailure(readException);
        FirestoreErrorAssertions.InvalidArgumentFailure(exception);
        Assert.Equal(charmander, (await document.GetDocumentSnapshotAsync<Pokemon>()).Data);
    }

    [Fact]
    public async Task returns_transaction_results_without_native_conversion()
    {
        var sut = CrossFirebaseFirestore.Current;
        var charmander = PokemonFactory.CreateCharmander();
        var document = GetTestingDocument(sut, "4");
        await document.SetDataAsync(charmander);

        // A transaction result never needs to be a Firestore value: these types either failed
        // conversion after the commit or could not round-trip at all before this fix.
        var dateResult = await sut.RunTransactionAsync(transaction => {
            transaction.UpdateData(document, (Pokemon.SightingCountField, charmander.SightingCount + 1));
            return new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        });
        Assert.Equal(new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc), dateResult);
        Assert.Equal(
            charmander.SightingCount + 1,
            (await document.GetDocumentSnapshotAsync<Pokemon>()).Data!.SightingCount
        );

        var expectedMoves = new[] { "ember", "slash" };
        var listResult = await sut.RunTransactionAsync(transaction => {
            transaction.UpdateData(document, (Pokemon.SightingCountField, charmander.SightingCount + 2));
            return expectedMoves;
        });
        Assert.Same(expectedMoves, listResult);

        var expectedPokemon = PokemonFactory.CreateSquirtle();
        var objectResult = await sut.RunTransactionAsync(transaction => {
            transaction.UpdateData(document, (Pokemon.SightingCountField, charmander.SightingCount + 3));
            return expectedPokemon;
        });
        Assert.Same(expectedPokemon, objectResult);
        Assert.Equal(
            charmander.SightingCount + 3,
            (await document.GetDocumentSnapshotAsync<Pokemon>()).Data!.SightingCount
        );
    }
}