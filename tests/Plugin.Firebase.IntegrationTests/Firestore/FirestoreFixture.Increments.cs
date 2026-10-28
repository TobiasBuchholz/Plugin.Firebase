using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore;

public sealed partial class FirestoreFixture
{
    // 2^53 + 1 is the smallest positive long a double can't represent; as a double it rounds to 2^53. An integer
    // increment by this operand only reads back exactly if the write path hands the long itself to the native SDK.
    private const long BeyondDoublePrecision = 9_007_199_254_740_993L;

    // Increment targets start at 1, so the exact result is 2^53 + 2 and every way of losing the operand reads back
    // differently. A plain write of the operand gives 2^53 + 1. An operand rounded to the integer 2^53, as before
    // #729, is added as an integer and also gives 2^53 + 1. A double increment by 2^53 gives 2^53, because the
    // server adds 1 + 2^53 in floating point and that rounds back to 2^53.
    private const long IncrementSeed = 1L;
    private const long SeedPlusBeyondDoublePrecision = IncrementSeed + BeyondDoublePrecision;

    [Fact]
    public async Task increments_double_field_values()
    {
        var sut = CrossFirebaseFirestore.Current;
        var pokemon = PokemonFactory.CreateBulbasur();
        var document = GetTestingDocument(sut, "double-increment");

        await document.SetDataAsync(pokemon);
        await document.UpdateDataAsync(("weight_in_kg", FieldValue.DoubleIncrement(0.25)));

        var snapshot = await document.GetDocumentSnapshotAsync<Pokemon>();
        Assert.Equal(pokemon.WeightInKg + 0.25, snapshot.Data!.WeightInKg, 6);
    }

    [Fact]
    public async Task increments_integer_field_values_beyond_double_precision()
    {
        var sut = CrossFirebaseFirestore.Current;
        var document = GetTestingDocument(sut, "integer-increment-precision");
        // long.MaxValue - 2 rounds up to 2^63 as a double, which converts back to long as long.MaxValue. Firestore
        // caps integer increments at long.MaxValue, so the expected result stays one below it to remain distinct.
        const long belowMaxResult = long.MaxValue - 1;
        const long belowMaxOperand = belowMaxResult - IncrementSeed;
        await document.SetDataAsync(new Dictionary<object, object?> {
            { "positive", IncrementSeed },
            { "negative", IncrementSeed },
            { "below_max", IncrementSeed }
        });

        await document.UpdateDataAsync(
            ("positive", FieldValue.IntegerIncrement(BeyondDoublePrecision)),
            ("negative", FieldValue.IntegerIncrement(-BeyondDoublePrecision)),
            ("below_max", FieldValue.IntegerIncrement(belowMaxOperand)));

        var data = (await document.GetDocumentSnapshotAsync<Dictionary<string, long>>()).Data!;
        Assert.Equal(SeedPlusBeyondDoublePrecision, data["positive"]);
        Assert.Equal(IncrementSeed - BeyondDoublePrecision, data["negative"]);
        Assert.Equal(belowMaxResult, data["below_max"]);
    }

    [Fact]
    public async Task increments_integer_field_values_beyond_double_precision_in_batch_writes()
    {
        var sut = CrossFirebaseFirestore.Current;
        await AssertUpdateAndMergeIncrementsAsync(sut, "batch", async (updateDocument, mergeDocument) => {
            var batch = sut.CreateBatch();
            batch.UpdateData(updateDocument, ("tuple_overload", FieldValue.IntegerIncrement(BeyondDoublePrecision)));
            batch.UpdateData(updateDocument, new Dictionary<object, object?> {
                { "dictionary_overload", FieldValue.IntegerIncrement(BeyondDoublePrecision) }
            });
            batch.SetData(
                mergeDocument,
                new Dictionary<object, object?> { { "count", FieldValue.IntegerIncrement(BeyondDoublePrecision) } },
                SetOptions.Merge());
            await batch.CommitAsync();
        });
    }

    [Fact]
    public async Task increments_integer_field_values_beyond_double_precision_in_transaction_writes()
    {
        var sut = CrossFirebaseFirestore.Current;
        await AssertUpdateAndMergeIncrementsAsync(sut, "transaction", (updateDocument, mergeDocument) =>
            sut.RunTransactionAsync(transaction => {
                transaction.UpdateData(
                    updateDocument, ("tuple_overload", FieldValue.IntegerIncrement(BeyondDoublePrecision)));
                transaction.UpdateData(updateDocument, new Dictionary<object, object?> {
                    { "dictionary_overload", FieldValue.IntegerIncrement(BeyondDoublePrecision) }
                });
                transaction.SetData(
                    mergeDocument,
                    new Dictionary<object, object?> { { "count", FieldValue.IntegerIncrement(BeyondDoublePrecision) } },
                    SetOptions.Merge());
                return true;
            }));
    }

    [Fact]
    public async Task increments_integer_field_values_beyond_double_precision_in_document_merge_set()
    {
        var sut = CrossFirebaseFirestore.Current;
        var document = GetTestingDocument(sut, "integer-increment-document-merge");
        await document.SetDataAsync(new Dictionary<object, object?> {
            { "count", IncrementSeed },
            { "untouched", "kept-by-merge" }
        });

        await document.SetDataAsync(
            new Dictionary<object, object?> { { "count", FieldValue.IntegerIncrement(BeyondDoublePrecision) } },
            SetOptions.Merge());

        var data = (await document.GetDocumentSnapshotAsync<WriteWrapperDictionaryDocument>()).Data!;
        Assert.Equal(SeedPlusBeyondDoublePrecision, data.Count);
        Assert.Equal("kept-by-merge", data.Untouched);
    }

    // Batch and transaction wrappers call value conversion separately in each write method, and that per-method
    // conversion is where these paths regressed before (#520, #522, #623). So the tuple UpdateData, the dictionary
    // UpdateData and the merge SetData each carry their own increment, on an update target and a merge target.
    private async Task AssertUpdateAndMergeIncrementsAsync(
        IFirebaseFirestore firestore,
        string writer,
        Func<IDocumentReference, IDocumentReference, Task> writeIncrements)
    {
        var updateDocument = GetTestingDocument(firestore, $"integer-increment-{writer}-update");
        var mergeDocument = GetTestingDocument(firestore, $"integer-increment-{writer}-merge");
        await updateDocument.SetDataAsync(new Dictionary<object, object?> {
            { "tuple_overload", IncrementSeed },
            { "dictionary_overload", IncrementSeed }
        });
        await mergeDocument.SetDataAsync(new Dictionary<object, object?> {
            { "count", IncrementSeed },
            { "untouched", "kept-by-merge" }
        });

        await writeIncrements(updateDocument, mergeDocument);

        var updateData = (await updateDocument.GetDocumentSnapshotAsync<Dictionary<string, long>>()).Data!;
        Assert.Equal(SeedPlusBeyondDoublePrecision, updateData["tuple_overload"]);
        Assert.Equal(SeedPlusBeyondDoublePrecision, updateData["dictionary_overload"]);
        var mergeData = (await mergeDocument.GetDocumentSnapshotAsync<WriteWrapperDictionaryDocument>()).Data!;
        Assert.Equal(SeedPlusBeyondDoublePrecision, mergeData.Count);
        Assert.Equal("kept-by-merge", mergeData.Untouched);
    }
}