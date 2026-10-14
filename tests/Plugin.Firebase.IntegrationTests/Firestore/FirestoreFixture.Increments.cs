using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore;

public sealed partial class FirestoreFixture
{
    // 2^53 + 1 is the smallest positive long a double can't represent; as a double it rounds to 2^53. An integer
    // increment by this operand only reads back exactly if the write path hands the long itself to the native SDK.
    private const long BeyondDoublePrecision = 9_007_199_254_740_993L;

    // Increment targets start at 1 rather than 0, so the result also differs from a plain write of the operand. A
    // rounded operand still fails: the server then adds 1 + 2^53 in floating point, which rounds back to 2^53.
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
        // long.MaxValue - 1 rounds up to 2^63 as a double
        const long belowMaxValue = long.MaxValue - 1;
        await document.SetDataAsync(new Dictionary<object, object?> {
            { "positive", 0L },
            { "negative", 0L },
            { "below_max", 0L }
        });

        await document.UpdateDataAsync(
            ("positive", FieldValue.IntegerIncrement(BeyondDoublePrecision)),
            ("negative", FieldValue.IntegerIncrement(-BeyondDoublePrecision)),
            ("below_max", FieldValue.IntegerIncrement(belowMaxValue)));

        var data = (await document.GetDocumentSnapshotAsync<Dictionary<string, long>>()).Data!;
        Assert.Equal(BeyondDoublePrecision, data["positive"]);
        Assert.Equal(-BeyondDoublePrecision, data["negative"]);
        Assert.Equal(belowMaxValue, data["below_max"]);
    }

    [Fact]
    public async Task increments_integer_field_values_beyond_double_precision_in_batch_writes()
    {
        // The tuple and dictionary UpdateData overloads and SetData each enter value conversion through their
        // own helper overload, so each one carries an increment.
        var sut = CrossFirebaseFirestore.Current;
        var updateDocument = GetTestingDocument(sut, "integer-increment-batch-update");
        var mergeDocument = GetTestingDocument(sut, "integer-increment-batch-merge");
        await updateDocument.SetDataAsync(new Dictionary<object, object?> {
            { "tuple_overload", IncrementSeed },
            { "dictionary_overload", IncrementSeed }
        });
        await mergeDocument.SetDataAsync(new Dictionary<object, object?> {
            { "value", IncrementSeed },
            { "untouched", "kept-by-merge" }
        });

        var batch = sut.CreateBatch();
        batch.UpdateData(updateDocument, ("tuple_overload", FieldValue.IntegerIncrement(BeyondDoublePrecision)));
        batch.UpdateData(updateDocument, new Dictionary<object, object?> {
            { "dictionary_overload", FieldValue.IntegerIncrement(BeyondDoublePrecision) }
        });
        batch.SetData(
            mergeDocument,
            new Dictionary<object, object?> { { "value", FieldValue.IntegerIncrement(BeyondDoublePrecision) } },
            SetOptions.Merge());
        await batch.CommitAsync();

        var updateData = (await updateDocument.GetDocumentSnapshotAsync<Dictionary<string, long>>()).Data!;
        Assert.Equal(SeedPlusBeyondDoublePrecision, updateData["tuple_overload"]);
        Assert.Equal(SeedPlusBeyondDoublePrecision, updateData["dictionary_overload"]);
        var mergeData = (await mergeDocument.GetDocumentSnapshotAsync<Dictionary<string, object>>()).Data!;
        Assert.Equal(SeedPlusBeyondDoublePrecision, mergeData["value"]);
        Assert.Equal("kept-by-merge", mergeData["untouched"]);
    }

    [Fact]
    public async Task increments_integer_field_values_beyond_double_precision_in_transaction_writes()
    {
        // The tuple and dictionary UpdateData overloads and SetData each enter value conversion through their
        // own helper overload, so each one carries an increment.
        var sut = CrossFirebaseFirestore.Current;
        var updateDocument = GetTestingDocument(sut, "integer-increment-transaction-update");
        var mergeDocument = GetTestingDocument(sut, "integer-increment-transaction-merge");
        await updateDocument.SetDataAsync(new Dictionary<object, object?> {
            { "tuple_overload", IncrementSeed },
            { "dictionary_overload", IncrementSeed }
        });
        await mergeDocument.SetDataAsync(new Dictionary<object, object?> {
            { "value", IncrementSeed },
            { "untouched", "kept-by-merge" }
        });

        await sut.RunTransactionAsync(transaction => {
            transaction.UpdateData(
                updateDocument, ("tuple_overload", FieldValue.IntegerIncrement(BeyondDoublePrecision)));
            transaction.UpdateData(updateDocument, new Dictionary<object, object?> {
                { "dictionary_overload", FieldValue.IntegerIncrement(BeyondDoublePrecision) }
            });
            transaction.SetData(
                mergeDocument,
                new Dictionary<object, object?> { { "value", FieldValue.IntegerIncrement(BeyondDoublePrecision) } },
                SetOptions.Merge());
            return true;
        });

        var updateData = (await updateDocument.GetDocumentSnapshotAsync<Dictionary<string, long>>()).Data!;
        Assert.Equal(SeedPlusBeyondDoublePrecision, updateData["tuple_overload"]);
        Assert.Equal(SeedPlusBeyondDoublePrecision, updateData["dictionary_overload"]);
        var mergeData = (await mergeDocument.GetDocumentSnapshotAsync<Dictionary<string, object>>()).Data!;
        Assert.Equal(SeedPlusBeyondDoublePrecision, mergeData["value"]);
        Assert.Equal("kept-by-merge", mergeData["untouched"]);
    }

    [Fact]
    public async Task increments_integer_field_values_beyond_double_precision_in_document_merge_set()
    {
        var sut = CrossFirebaseFirestore.Current;
        var document = GetTestingDocument(sut, "integer-increment-document-merge");
        await document.SetDataAsync(new Dictionary<object, object?> {
            { "value", IncrementSeed },
            { "untouched", "kept-by-merge" }
        });

        await document.SetDataAsync(
            new Dictionary<object, object?> { { "value", FieldValue.IntegerIncrement(BeyondDoublePrecision) } },
            SetOptions.Merge());

        var data = (await document.GetDocumentSnapshotAsync<Dictionary<string, object>>()).Data!;
        Assert.Equal(SeedPlusBeyondDoublePrecision, data["value"]);
        Assert.Equal("kept-by-merge", data["untouched"]);
    }
}