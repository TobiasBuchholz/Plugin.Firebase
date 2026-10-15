using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore;

public sealed partial class FirestoreFixture
{
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
        await AssertCommittedSightingCountAsync(document, charmander.SightingCount + 1);

        var expectedMoves = new[] { "ember", "slash" };
        var listResult = await sut.RunTransactionAsync(transaction => {
            transaction.UpdateData(document, (Pokemon.SightingCountField, charmander.SightingCount + 2));
            return expectedMoves;
        });
        Assert.Same(expectedMoves, listResult);
        await AssertCommittedSightingCountAsync(document, charmander.SightingCount + 2);

        var expectedPokemon = PokemonFactory.CreateSquirtle();
        var objectResult = await sut.RunTransactionAsync(transaction => {
            transaction.UpdateData(document, (Pokemon.SightingCountField, charmander.SightingCount + 3));
            return expectedPokemon;
        });
        Assert.Same(expectedPokemon, objectResult);
        await AssertCommittedSightingCountAsync(document, charmander.SightingCount + 3);
    }

    [Fact]
    public async Task returns_tuple_and_record_transaction_results()
    {
        var sut = CrossFirebaseFirestore.Current;
        var charmander = PokemonFactory.CreateCharmander();
        var document = GetTestingDocument(sut, "4");
        await document.SetDataAsync(charmander);

        // The native conversion could not express these types, so before this fix they aborted the transaction
        // inside the update function.
        var tupleResult = await sut.RunTransactionAsync(transaction => {
            transaction.UpdateData(document, (Pokemon.SightingCountField, charmander.SightingCount + 1));
            return (charmander.Name, SightingCount: charmander.SightingCount + 1);
        });
        Assert.Equal((charmander.Name, charmander.SightingCount + 1), tupleResult);
        await AssertCommittedSightingCountAsync(document, charmander.SightingCount + 1);

        var expectedSighting = new TransactionSighting(charmander.Name, charmander.SightingCount + 2);
        var recordResult = await sut.RunTransactionAsync(transaction => {
            transaction.UpdateData(document, (Pokemon.SightingCountField, expectedSighting.SightingCount));
            return expectedSighting;
        });
        Assert.Same(expectedSighting, recordResult);
        await AssertCommittedSightingCountAsync(document, charmander.SightingCount + 2);
    }

    [Fact]
    public async Task rejects_async_transaction_update_function_without_committing()
    {
        var sut = CrossFirebaseFirestore.Current;
        var charmander = PokemonFactory.CreateCharmander();
        var document = GetTestingDocument(sut, "4");
        await document.SetDataAsync(charmander);

        // The native SDKs commit as soon as the update function returns, and an async update function returns at
        // its first await, so only the writes made before it would commit. The compiler infers a Task TResult for
        // an async lambda, which is rejected before the transaction starts.
        var asyncAttempts = 0;
        var asyncException = await Record.ExceptionAsync(() => sut.RunTransactionAsync(async transaction => {
            asyncAttempts++;
            transaction.UpdateData(document, (Pokemon.SightingCountField, charmander.SightingCount + 1));
            await Task.Yield();
        }));

        AssertAsyncUpdateFunctionRejected(asyncException);
        Assert.Equal(0, asyncAttempts);
        Assert.Equal(charmander, (await document.GetDocumentSnapshotAsync<Pokemon>()).Data);

        // A task returned through an object result is only seen once the update function has returned, so that
        // attempt is aborted before its write commits, and it isn't retried.
        var objectAttempts = 0;
        var objectException = await Record.ExceptionAsync(() => sut.RunTransactionAsync<object?>(transaction => {
            objectAttempts++;
            return UpdateSightingCountAsync(transaction, document, charmander.SightingCount + 2);
        }));

        AssertAsyncUpdateFunctionRejected(objectException);
        Assert.Equal(1, objectAttempts);
        Assert.Equal(charmander, (await document.GetDocumentSnapshotAsync<Pokemon>()).Data);
    }

    private static async Task AssertCommittedSightingCountAsync(IDocumentReference document, long expected)
    {
        Assert.Equal(expected, (await document.GetDocumentSnapshotAsync<Pokemon>()).Data!.SightingCount);
    }

    private static void AssertAsyncUpdateFunctionRejected(Exception? exception)
    {
        var argumentException = Assert.IsType<ArgumentException>(exception);
        Assert.Equal("updateFunc", argumentException.ParamName);
    }

    /// <summary>
    /// Writes before its first await, like an async update function, and leaves the transaction alone afterwards.
    /// </summary>
    private static async Task UpdateSightingCountAsync(
        ITransaction transaction,
        IDocumentReference document,
        long sightingCount)
    {
        transaction.UpdateData(document, (Pokemon.SightingCountField, sightingCount));
        await Task.Yield();
    }

    private sealed record TransactionSighting(string Name, long SightingCount);
}