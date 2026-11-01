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

    [Fact]
    public async Task surfaces_cross_platform_exception_when_write_is_rejected()
    {
        var sut = CrossFirebaseFirestore.Current;
        var document = GetTestingDocument(sut, "oversized-write");

        // A field value over the 1 MiB document limit is rejected by the backend when the write is committed.
        var oversized = new string('x', 1_048_789);
        var exception = await Record.ExceptionAsync(() => document.SetDataAsync(("oversized", oversized)));

        FirestoreErrorAssertions.InvalidArgumentFailure(exception);
    }

    [Fact]
    public async Task surfaces_cross_platform_exception_when_cache_read_misses()
    {
        var sut = CrossFirebaseFirestore.Current;
        var document = GetTestingDocument(sut, IntegrationTestData.UniqueId("uncached"));

        // A cache-only read of a document the local cache has never seen fails with UNAVAILABLE.
        var exception = await Record.ExceptionAsync(
            () => document.GetDocumentSnapshotAsync<SimpleItem>(Source.Cache));

        FirestoreErrorAssertions.UnavailableFailure(exception);
    }
}