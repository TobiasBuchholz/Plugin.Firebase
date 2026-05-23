using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore;

public sealed partial class FirestoreFixture
{
    [Fact]
    public async Task set_and_get_a_map()
    {
        var sut = CrossFirebaseFirestore.Current;
        var pokemon = PokemonFactory.CreateCharmeleon();
        var path = TestingDocumentPath(pokemon.Id);
        var document = GetTestingDocument(sut, pokemon.Id);

        await document.SetDataAsync(pokemon);

        var snapshot = await document.GetDocumentSnapshotAsync<Pokemon>();
        FirestoreAssertions.WrittenDocument(snapshot, pokemon.Id, path, pokemon);
        FirestoreAssertions.PokemonOtherProperties(snapshot.Data, expectedLegs: 4L, expectedColors: 3L);

        var updates = new Dictionary<object, object?> {
            { Pokemon.OtherPropertiesColorsPath, FieldValue.IntegerIncrement(1) }
        };

        await document.UpdateDataAsync(updates);

        snapshot = await document.GetDocumentSnapshotAsync<Pokemon>();
        FirestoreAssertions.PokemonOtherProperties(snapshot.Data, expectedLegs: 4L, expectedColors: 4L);
    }

    [Fact]
    public async Task covers_document_set_overloads_with_merge()
    {
        var sut = CrossFirebaseFirestore.Current;
        var mergedDictionaryDocument = GetTestingDocument(sut, "merge-dictionary");
        var tupleDocument = GetTestingDocument(sut, "tuple-set");
        var mergedTupleDocument = GetTestingDocument(sut, "merge-tuple");

        await mergedDictionaryDocument.SetDataAsync(PokemonFactory.CreateCharmander());
        await mergedDictionaryDocument.SetDataAsync(
            new Dictionary<object, object?> {
                { Pokemon.NameField, "Merged Charmander" }
            },
            SetOptions.Merge());

        await tupleDocument.SetDataAsync(
            (Pokemon.NameField, "Tuple Pokemon"),
            (Pokemon.SightingCountField, 12L));

        await mergedTupleDocument.SetDataAsync(PokemonFactory.CreateSquirtle());
        await mergedTupleDocument.SetDataAsync(
            SetOptions.Merge(),
            (Pokemon.NameField, "Merged Squirtle"));

        await FirestoreAssertions.PokemonWriteOverloadResultsAsync(
            mergedDictionaryDocument,
            "Merged Charmander",
            tupleDocument,
            "Tuple Pokemon",
            12L,
            mergedTupleDocument,
            "Merged Squirtle");
    }

    [Fact]
    public async Task covers_batch_set_overloads_and_commit_local()
    {
        var sut = CrossFirebaseFirestore.Current;
        var mergedDictionaryDocument = GetTestingDocument(sut, "batch-merge-dictionary");
        var tupleDocument = GetTestingDocument(sut, "batch-tuple");
        var mergedTupleDocument = GetTestingDocument(sut, "batch-merge-tuple");

        await mergedDictionaryDocument.SetDataAsync(PokemonFactory.CreateCharmander());
        await mergedTupleDocument.SetDataAsync(PokemonFactory.CreateSquirtle());

        var batch = sut.CreateBatch();
        batch.SetData(
            mergedDictionaryDocument,
            new Dictionary<object, object?> {
                { Pokemon.NameField, "Batch Merged Charmander" }
            },
            SetOptions.Merge());
        batch.SetData(
            tupleDocument,
            (Pokemon.NameField, "Batch Tuple Pokemon"),
            (Pokemon.SightingCountField, 33L));
        batch.SetData(
            mergedTupleDocument,
            SetOptions.Merge(),
            (Pokemon.NameField, "Batch Merged Squirtle"));
        batch.CommitLocal();

        await sut.WaitForPendingWritesAsync();

        await FirestoreAssertions.PokemonWriteOverloadResultsAsync(
            mergedDictionaryDocument,
            "Batch Merged Charmander",
            tupleDocument,
            "Batch Tuple Pokemon",
            33L,
            mergedTupleDocument,
            "Batch Merged Squirtle");
    }

    [Fact]
    public async Task covers_transaction_set_overloads()
    {
        var sut = CrossFirebaseFirestore.Current;
        var mergedDictionaryDocument = GetTestingDocument(sut, "transaction-merge-dictionary");
        var mergedTupleDocument = GetTestingDocument(sut, "transaction-merge-tuple");

        await mergedDictionaryDocument.SetDataAsync(PokemonFactory.CreateCharmander());
        await mergedTupleDocument.SetDataAsync(PokemonFactory.CreateSquirtle());

        await sut.RunTransactionAsync(transaction => {
            transaction.SetData(
                mergedDictionaryDocument,
                new Dictionary<object, object?> {
                    { Pokemon.NameField, "Transaction Merged Charmander" }
                },
                SetOptions.Merge());
            transaction.SetData(
                mergedTupleDocument,
                SetOptions.Merge(),
                (Pokemon.NameField, "Transaction Merged Squirtle"),
                (Pokemon.SightingCountField, 91L));
            return true;
        });

        await FirestoreAssertions.PokemonWriteOverloadResultsAsync(
            mergedDictionaryDocument,
            "Transaction Merged Charmander",
            null,
            null,
            null,
            mergedTupleDocument,
            "Transaction Merged Squirtle",
            expectedMergedTupleSightingCount: 91L);
    }

    [Fact]
    public async Task updates_nested_map_and_datetime_values()
    {
        var sut = CrossFirebaseFirestore.Current;
        var pokemon = PokemonFactory.CreateSquirtle();
        var document = GetTestingDocument(sut, pokemon.Id);
        var expectedCreationDate = new DateTime(2024, 1, 2, 3, 4, 5, 678, DateTimeKind.Utc);
        var expectedLocation = new SightingLocation(13.37, 42.24);

        await document.SetDataAsync(pokemon);
        await document.UpdateDataAsync(
            ("creation_date", expectedCreationDate),
            (Pokemon.FirstSightingLocationField, new Dictionary<object, object?> {
                { "latitude", expectedLocation.Latitude },
                { "longitude", expectedLocation.Longitude }
            }),
            ("other_properties", new Dictionary<object, object?> {
                { "legs", 4L },
                { "colors", 3L }
            })
        );

        var snapshot = await document.GetDocumentSnapshotAsync<Pokemon>();
        FirestoreAssertions.PokemonNestedMapAndDateValues(snapshot.Data, expectedCreationDate, expectedLocation);
    }

    [Fact]
    public async Task reads_issue_422_crew_check_in_document()
    {
        var sut = CrossFirebaseFirestore.Current;
        var document = GetTestingDocument(sut, "issue-422-crew-check-in");
        var scenario = CrewCheckInFactory.CreateIssue422Scenario();

        await document.SetDataAsync(scenario.CrewCheckIn);

        var snapshot = await document.GetDocumentSnapshotAsync<CrewCheckIn>();

        FirestoreAssertions.CrewCheckInDocument(snapshot.Data, scenario.Timestamp, scenario.LogTimestamp);
    }
}