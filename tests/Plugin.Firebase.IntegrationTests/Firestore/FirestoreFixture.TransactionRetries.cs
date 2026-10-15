using Plugin.Firebase.Core.Exceptions;
using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore;

public sealed partial class FirestoreFixture
{
    [Fact]
    public async Task retries_transaction_when_document_changes_between_transactional_reads()
    {
        var sut = CrossFirebaseFirestore.Current;
        var charmander = PokemonFactory.CreateCharmander();
        var document = GetTestingDocument(sut, "4");
        await document.SetDataAsync(charmander);
        var changedSightingCount = charmander.SightingCount + 10;

        var attempts = 0;
        var readFailures = new List<Exception>();
        var sightingCount = await sut.RunTransactionAsync(transaction => {
            attempts++;
            Pokemon pokemon;
            try {
                pokemon = ReadAroundOutsideChange(transaction, document, attempts, changedSightingCount);
            } catch(Exception e) {
                readFailures.Add(e);
                throw;
            }
            var newSightingCount = pokemon.SightingCount + 1;
            transaction.UpdateData(document, (Pokemon.SightingCountField, newSightingCount));
            return newSightingCount;
        });

        // The native retry rules retry the ABORTED read of the first attempt, and the second attempt reads the
        // changed document and commits.
        Assert.Equal(2, attempts);
        FirestoreErrorAssertions.AbortedFailure(Assert.Single(readFailures));
        Assert.Equal(changedSightingCount + 1, sightingCount);
        Assert.Equal(
            changedSightingCount + 1,
            (await document.GetDocumentSnapshotAsync<Pokemon>()).Data!.SightingCount
        );
    }

    [Fact]
    public async Task does_not_retry_transaction_when_update_function_wraps_transactional_read_failure()
    {
        var sut = CrossFirebaseFirestore.Current;
        var charmander = PokemonFactory.CreateCharmander();
        var document = GetTestingDocument(sut, "4");
        await document.SetDataAsync(charmander);

        // The native retry rules only see the exception the update function throws, so an exception of the
        // caller's own that wraps the retryable read failure aborts the transaction like any other exception.
        var attempts = 0;
        var exception = await Record.ExceptionAsync(() => sut.RunTransactionAsync<object?>(transaction => {
            attempts++;
            try {
                ReadAroundOutsideChange(transaction, document, attempts, charmander.SightingCount + 10);
            } catch(CrossPlatformFirebaseFirestoreException e) {
                throw new TransactionAbortedByCallerException(e);
            }
            return null;
        }));

        Assert.Equal(1, attempts);
        var firebaseException = Assert.IsType<FirebaseException>(exception);
        var original = Assert.IsType<TransactionAbortedByCallerException>(firebaseException.InnerException);
        FirestoreErrorAssertions.AbortedFailure(original.InnerException);
    }

    /// <summary>
    /// Reads the document twice. On the first attempt the document is changed outside the transaction between
    /// the two reads, so the second read finds a newer version and fails with <c>ABORTED</c>, which the native
    /// transaction retry rules treat as retryable.
    /// </summary>
    private static Pokemon ReadAroundOutsideChange(
        ITransaction transaction,
        IDocumentReference document,
        int attempt,
        long changedSightingCount)
    {
        transaction.GetDocument<Pokemon>(document);
        if(attempt == 1) {
            // The update function is synchronous and runs on a background thread, so wait there until the
            // backend has acknowledged the write.
            document
                .UpdateDataAsync((Pokemon.SightingCountField, changedSightingCount))
                .WaitForTestAsync(IntegrationTestTimeouts.Callback, "Firestore write outside the transaction")
                .GetAwaiter()
                .GetResult();
        }
        return transaction.GetDocument<Pokemon>(document)!.Data!;
    }

    private sealed class TransactionAbortedByCallerException(Exception innerException)
        : Exception("transaction aborted by update function", innerException);
}