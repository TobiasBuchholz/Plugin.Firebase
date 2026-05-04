using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore;

public sealed partial class FirestoreFixture
{
    [Fact]
    public async Task adds_document_to_collection()
    {
        var sut = CrossFirebaseFirestore.Current;
        var pokemon = PokemonFactory.CreateBulbasur();
        var path = TestingDocumentPath(pokemon.Id);
        var document = GetTestingDocument(sut, pokemon.Id);

        await document.SetDataAsync(pokemon);

        var snapshot = await document.GetDocumentSnapshotAsync<Pokemon>();
        FirestoreAssertions.WrittenDocument(snapshot, pokemon.Id, path, pokemon);
    }

    [Fact]
    public async Task creates_document_with_auto_generated_reference()
    {
        var sut = CrossFirebaseFirestore.Current;
        var collection = GetTestingCollection(sut);
        var document = collection.CreateDocument();
        var item = new SimpleItem("generated-item");

        await document.SetDataAsync(item);

        var snapshot = await document.GetDocumentSnapshotAsync<SimpleItem>();
        FirestoreAssertions.GeneratedSimpleItem(document, snapshot, "generated-item");
    }

    [Fact]
    public async Task adds_document_with_auto_generated_id()
    {
        var sut = CrossFirebaseFirestore.Current;
        var collection = GetTestingCollection(sut);

        var document = await collection.AddDocumentAsync(new SimpleItem("added-item"));

        var snapshot = await document.GetDocumentSnapshotAsync<SimpleItem>();
        FirestoreAssertions.GeneratedSimpleItem(document, snapshot, "added-item");
    }

    [Fact]
    public async Task sets_server_timestamp_via_property_attribute()
    {
        var sut = CrossFirebaseFirestore.Current;
        var pokemon = PokemonFactory.CreateBulbasur();

        var document = GetTestingDocument(sut, pokemon.Id);
        await document.SetDataAsync(pokemon);

        var snapshot = await GetTestingDocument(sut, pokemon.Id)
            .GetDocumentSnapshotAsync<Pokemon>(Source.Server);
        Assert.NotEqual(snapshot.Data!.ServerTimestamp, DateTimeOffset.MinValue);
        Assert.NotEqual(snapshot.Data!.ServerTimestamp, DateTimeOffset.Now);
    }

    [Fact]
    public async Task updates_existing_document()
    {
        var sut = CrossFirebaseFirestore.Current;
        var pokemon = PokemonFactory.CreateSquirtle();
        var document = GetTestingDocument(sut, pokemon.Id);

        await document.SetDataAsync(pokemon);
        Assert.Equal(pokemon, (await document.GetDocumentSnapshotAsync<Pokemon>()).Data!);

        var update = new Dictionary<object, object?> {
            { Pokemon.NameField, "Cool Squirtle" },
            { Pokemon.MovesField, FieldValue.ArrayUnion("Bubble-Blast") },
            { $"{Pokemon.FirstSightingLocationField}.latitude", 13.37 },
            { "original_reference", document }
        };

        await document.UpdateDataAsync(update);
        var snapshot = await document.GetDocumentSnapshotAsync<Pokemon>();
        var data = snapshot.Data!;
        Assert.Equal("Cool Squirtle", data.Name);
        Assert.NotNull(data.Moves);
        Assert.Contains("Bubble-Blast", data.Moves);
        Assert.NotNull(data.FirstSightingLocation);
        Assert.Equal(13.37, data.FirstSightingLocation.Latitude);
    }

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
    public async Task writes_data_as_batch()
    {
        var sut = CrossFirebaseFirestore.Current;
        var bulbasur = PokemonFactory.CreateBulbasur();
        var charmander = PokemonFactory.CreateCharmander();
        var squirtle = PokemonFactory.CreateSquirtle();
        var documentBulbasur = GetTestingDocument(sut, "1");
        var documentCharmander = GetTestingDocument(sut, "4");
        var documentSquirtle = GetTestingDocument(sut, "7");
        await documentBulbasur.SetDataAsync(bulbasur);
        await documentCharmander.SetDataAsync(charmander);

        var batch = sut.CreateBatch();
        batch.SetData(documentSquirtle, squirtle);
        batch.UpdateData(documentCharmander, (Pokemon.SightingCountField, 1337));
        batch.DeleteDocument(documentBulbasur);
        await batch.CommitAsync();

        Assert.Equal(squirtle, (await documentSquirtle.GetDocumentSnapshotAsync<Pokemon>()).Data!);
        Assert.Equal(1337, (await documentCharmander.GetDocumentSnapshotAsync<Pokemon>()).Data!.SightingCount);
        Assert.Null((await documentBulbasur.GetDocumentSnapshotAsync<Pokemon>()).Data);
    }
}