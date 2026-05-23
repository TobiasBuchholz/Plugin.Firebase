using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore;

public sealed partial class FirestoreFixture
{
    [Fact]
    public async Task gets_data_with_simple_queries()
    {
        var sut = CrossFirebaseFirestore.Current;
        var collection = FirestoreAssertions.SeededPokemonCollection(sut);

        var firePokemons = await collection
            .WhereEqualsTo(Pokemon.PokeTypeField, PokeType.Fire)
            .GetDocumentsAsync<Pokemon>();

        var smallPokemons = await collection
            .WhereLessThanOrEqualsTo("height_in_cm", 100)
            .GetDocumentsAsync<Pokemon>();

        Assert.Equal(3, firePokemons.Documents.Count());
        Assert.Equal(5, smallPokemons.Documents.Count());
    }

    [Fact]
    public async Task gets_data_with_compound_query()
    {
        var sut = CrossFirebaseFirestore.Current;
        var collection = FirestoreAssertions.SeededPokemonCollection(sut);

        var smallWaterPokemons = await collection
            .WhereEqualsTo(Pokemon.PokeTypeField, PokeType.Water)
            .WhereGreaterThanOrEqualsTo("height_in_cm", 50)
            .WhereLessThan("height_in_cm", 100)
            .GetDocumentsAsync<Pokemon>();

        Assert.Single(smallWaterPokemons.Documents);
    }

    [Fact]
    public async Task gets_data_with_array_contains_queries()
    {
        var sut = CrossFirebaseFirestore.Current;

        var pokemonsByContains = await FirestoreAssertions
            .SeededPokemonCollection(sut)
            .WhereArrayContains(Pokemon.MovesField, "Razor-Wind")
            .GetDocumentsAsync<Pokemon>();

        var pokemonsByContainsAny = await FirestoreAssertions
            .SeededPokemonCollection(sut)
            .WhereArrayContainsAny(Pokemon.MovesField, ["Razor-Wind", "Fire-Punch"])
            .GetDocumentsAsync<Pokemon>();

        FirestoreAssertions.PokemonIds(pokemonsByContains, "1", "2", "3");
        FirestoreAssertions.PokemonIds(pokemonsByContainsAny, "1", "2", "3", "4", "5", "6");
    }

    [Fact]
    public async Task gets_data_using_in_query()
    {
        var sut = CrossFirebaseFirestore.Current;

        var pokemons = await FirestoreAssertions
            .SeededPokemonCollection(sut)
            .WhereFieldIn(FieldPath.DocumentId(), ["1", "2", "3"])
            .GetDocumentsAsync<Pokemon>();

        FirestoreAssertions.PokemonIds(pokemons, "1", "2", "3");
    }

    [Fact]
    public async Task uses_field_path_overloads()
    {
        var sut = CrossFirebaseFirestore.Current;
        var nestedFieldPath = FieldPath.Of([Pokemon.FirstSightingLocationField, "latitude"]);

        var nestedPathResults = await FirestoreAssertions
            .SeededPokemonCollection(sut)
            .WhereEqualsTo(nestedFieldPath, 52.5042112)
            .GetDocumentsAsync<Pokemon>();

        var documentIdResults = await FirestoreAssertions
            .SeededPokemonCollection(sut)
            .OrderBy(FieldPath.DocumentId())
            .StartingAt("2")
            .EndingAt("4")
            .GetDocumentsAsync<Pokemon>();

        Assert.Equal(9, nestedPathResults.Count);
        FirestoreAssertions.PokemonIds(documentIdResults, "2", "3", "4");
    }

    [Fact]
    public async Task orders_and_limits_data()
    {
        var sut = CrossFirebaseFirestore.Current;

        var pokemons = await FirestoreAssertions
            .SeededPokemonCollection(sut)
            .OrderBy(Pokemon.NameField, true)
            .LimitedTo(3)
            .GetDocumentsAsync<Pokemon>();

        FirestoreAssertions.PokemonNames(pokemons, "Wartortle", "Venusaur", "Squirtle");
    }

    [Fact]
    public async Task uses_limited_to_last()
    {
        var sut = CrossFirebaseFirestore.Current;

        var pokemons = await FirestoreAssertions
            .SeededPokemonCollection(sut)
            .OrderBy(Pokemon.NameField)
            .LimitedToLast(3)
            .GetDocumentsAsync<Pokemon>();

        FirestoreAssertions.PokemonNames(pokemons, "Squirtle", "Venusaur", "Wartortle");
    }

    [Fact]
    public async Task adds_simple_cursor_to_query()
    {
        var sut = CrossFirebaseFirestore.Current;

        var pokemonsByHeight = await FirestoreAssertions
            .SeededPokemonCollection(sut)
            .OrderBy("height_in_cm")
            .StartingAt(50)
            .EndingBefore(100)
            .GetDocumentsAsync<Pokemon>();

        var pokemonsByWeight = await FirestoreAssertions
            .SeededPokemonCollection(sut)
            .OrderBy("weight_in_kg")
            .StartingAfter(8.5)
            .EndingAt(85.5)
            .GetDocumentsAsync<Pokemon>();

        FirestoreAssertions.PokemonIds(pokemonsByHeight, "7", "4", "1");
        FirestoreAssertions.PokemonIds(pokemonsByWeight, "7", "2", "5", "8", "9");
    }

    [Fact]
    public async Task uses_document_snapshot_to_define_query_cursor()
    {
        var sut = CrossFirebaseFirestore.Current;

        var snapshot = await FirestoreAssertions
            .SeededPokemonDocument(sut, "2")
            .GetDocumentSnapshotAsync<Pokemon>();

        var pokemons = await FirestoreAssertions
            .SeededPokemonCollection(sut)
            .OrderBy(Pokemon.NameField)
            .StartingAt(snapshot)
            .GetDocumentsAsync<Pokemon>();

        FirestoreAssertions.PokemonNames(pokemons, "Ivysaur", "Squirtle", "Venusaur", "Wartortle");
    }

    [Fact]
    public async Task uses_snapshot_end_cursors()
    {
        var sut = CrossFirebaseFirestore.Current;
        var snapshot = await FirestoreAssertions
            .SeededPokemonDocument(sut, "7")
            .GetDocumentSnapshotAsync<Pokemon>();

        var endingAt = await FirestoreAssertions
            .SeededPokemonCollection(sut)
            .OrderBy(Pokemon.NameField)
            .EndingAt(snapshot)
            .GetDocumentsAsync<Pokemon>();

        var endingBefore = await FirestoreAssertions
            .SeededPokemonCollection(sut)
            .OrderBy(Pokemon.NameField)
            .EndingBefore(snapshot)
            .GetDocumentsAsync<Pokemon>();

        FirestoreAssertions.PokemonNames(
            endingAt,
            "Blastoise",
            "Bulbasaur",
            "Charizard",
            "Charmander",
            "Charmeleon",
            "Ivysaur",
            "Squirtle");
        FirestoreAssertions.PokemonNames(
            endingBefore,
            "Blastoise",
            "Bulbasaur",
            "Charizard",
            "Charmander",
            "Charmeleon",
            "Ivysaur");
    }

    [Fact]
    public async Task sets_multiple_cursor_conditions()
    {
        var sut = CrossFirebaseFirestore.Current;

        var pokemons = await FirestoreAssertions
            .SeededPokemonCollection(sut)
            .OrderBy(Pokemon.PokeTypeField)
            .OrderBy(Pokemon.NameField)
            .StartingAt(PokeType.Water, "Squirtle")
            .GetDocumentsAsync<Pokemon>();

        FirestoreAssertions.PokemonNames(pokemons, "Squirtle", "Wartortle", "Bulbasaur", "Ivysaur", "Venusaur");
    }

    [Fact]
    public async Task paginates_data()
    {
        var sut = CrossFirebaseFirestore.Current;
        var collection = FirestoreAssertions.SeededPokemonCollection(sut);

        var firstPageSnapshot = await collection
            .LimitedTo(5)
            .GetDocumentsAsync<Pokemon>();

        var nextPageSnapshot = await collection
            .LimitedTo(5)
            .StartingAfter(firstPageSnapshot.Documents.Last())
            .GetDocumentsAsync<Pokemon>();

        FirestoreAssertions.PokemonIds(firstPageSnapshot, "1", "2", "3", "4", "5");
        FirestoreAssertions.PokemonIds(nextPageSnapshot, "6", "7", "8", "9");
    }

    [Fact]
    public async Task covers_query_snapshot_properties()
    {
        var sut = CrossFirebaseFirestore.Current;
        var snapshot = await FirestoreAssertions
            .SeededPokemonCollection(sut)
            .WhereEqualsTo(Pokemon.PokeTypeField, PokeType.Fire)
            .GetDocumentsAsync<Pokemon>();

        FirestoreAssertions.QuerySnapshotProperties(snapshot);
    }

}