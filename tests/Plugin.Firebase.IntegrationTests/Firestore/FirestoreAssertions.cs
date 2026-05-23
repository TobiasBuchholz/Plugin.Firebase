using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore;

internal static class FirestoreAssertions
{
    public static ICollectionReference SeededPokemonCollection(IFirebaseFirestore firestore)
    {
        // The Pokemon collection is shared seed data for emulator and real-backend query coverage.
        return firestore.GetCollection("pokemons");
    }

    public static IDocumentReference SeededPokemonDocument(IFirebaseFirestore firestore, string id)
    {
        return firestore.GetDocument($"pokemons/{id}");
    }

    public static void PokemonIds(IQuerySnapshot<Pokemon> snapshot, params string[] expectedIds)
    {
        Assert.Equal(expectedIds, snapshot.Documents.Select(x => Require(x.Data).Id));
    }

    public static void PokemonNames(IQuerySnapshot<Pokemon> snapshot, params string[] expectedNames)
    {
        Assert.Equal(expectedNames, snapshot.Documents.Select(x => Require(x.Data).Name));
    }

    public static void QuerySnapshotProperties<T>(IQuerySnapshot<T> snapshot)
    {
        Assert.False(snapshot.IsEmpty);
        Assert.Equal(snapshot.Documents.Count(), snapshot.Count);
        Assert.NotNull(snapshot.Query);
        Assert.NotNull(snapshot.Metadata);
        Assert.NotEmpty(snapshot.DocumentChanges);
        Assert.NotEmpty(snapshot.GetDocumentChanges(includeMetadataChanges: false));
    }

    public static void WrittenDocument<T>(
        IDocumentSnapshot<T> snapshot,
        string expectedId,
        string expectedPath,
        T expectedData)
        where T : class
    {
        Assert.False(snapshot.Metadata.HasPendingWrites);
        Assert.Equal(expectedId, snapshot.Reference.Id);
        Assert.Equal(expectedPath, snapshot.Reference.Path);
        Assert.Equal(expectedData, Require(snapshot.Data));
    }

    public static void GeneratedSimpleItem(
        IDocumentReference document,
        IDocumentSnapshot<SimpleItem> snapshot,
        string expectedTitle)
    {
        Assert.False(string.IsNullOrWhiteSpace(document.Id));
        Assert.False(string.IsNullOrWhiteSpace(document.Path));
        Assert.Equal(document.Id, snapshot.Reference.Id);
        var data = Require(snapshot.Data);
        Assert.Equal(document.Id, data.Id);
        Assert.Equal(expectedTitle, data.Title);
    }

    public static void PokemonOtherProperties(Pokemon? pokemon, long expectedLegs, long expectedColors)
    {
        var properties = Require(pokemon).OtherProperties;
        Assert.NotNull(properties);
        Assert.Equal(expectedLegs, Convert.ToInt64(properties["legs"]));
        Assert.Equal(expectedColors, Convert.ToInt64(properties["colors"]));
    }

    public static void PokemonNestedMapAndDateValues(
        Pokemon? pokemon,
        DateTime expectedCreationDate,
        SightingLocation expectedLocation)
    {
        var data = Require(pokemon);
        Assert.InRange(Math.Abs(data.CreationDate.Ticks - expectedCreationDate.Ticks), 0, 10);
        Assert.Equal(expectedLocation, data.FirstSightingLocation);
        PokemonOtherProperties(data, expectedLegs: 4L, expectedColors: 3L);
    }

    public static async Task PokemonWriteOverloadResultsAsync(
        IDocumentReference mergedDictionaryDocument,
        string expectedMergedDictionaryName,
        IDocumentReference? tupleDocument,
        string? expectedTupleName,
        long? expectedTupleSightingCount,
        IDocumentReference mergedTupleDocument,
        string expectedMergedTupleName,
        long? expectedMergedTupleSightingCount = null)
    {
        var mergedDictionarySnapshot = await mergedDictionaryDocument.GetDocumentSnapshotAsync<Pokemon>();
        var mergedTupleSnapshot = await mergedTupleDocument.GetDocumentSnapshotAsync<Pokemon>();

        Assert.Equal(expectedMergedDictionaryName, mergedDictionarySnapshot.Data?.Name);
        Assert.Equal(60, mergedDictionarySnapshot.Data?.HeightInCm);
        Assert.Equal(expectedMergedTupleName, mergedTupleSnapshot.Data?.Name);
        Assert.Equal(50, mergedTupleSnapshot.Data?.HeightInCm);
        if(expectedMergedTupleSightingCount != null) {
            Assert.Equal(expectedMergedTupleSightingCount, mergedTupleSnapshot.Data?.SightingCount);
        }

        if(tupleDocument == null) {
            return;
        }

        var tupleSnapshot = await tupleDocument.GetDocumentSnapshotAsync<Pokemon>();
        Assert.Equal(expectedTupleName, tupleSnapshot.Data?.Name);
        Assert.Equal(expectedTupleSightingCount, tupleSnapshot.Data?.SightingCount);
    }

    public static void RawDictionaryData(IDictionary<string, object?> data, IDocumentReference document)
    {
        Assert.NotNull(data);
        Assert.Equal("value", data["unknown_string"]);
        Assert.Equal(123L, Convert.ToInt64(data["unknown_long"]));
        Assert.Equal(12.5, Convert.ToDouble(data["unknown_double"]));
        Assert.True((bool) data["unknown_bool"]!);
        Assert.Null(data["unknown_null"]);

        var numbers = Assert.IsAssignableFrom<IList<object?>>(data["unknown_numbers"]);
        Assert.Equal([1L, 2L], numbers.Select(Convert.ToInt64));

        Assert.Empty(Assert.IsAssignableFrom<IList<object?>>(data["unknown_empty_array"]));
        Assert.Empty(Assert.IsAssignableFrom<IDictionary<string, object?>>(data["unknown_empty_map"]));

        var arrayWithNulls = Assert.IsAssignableFrom<IList<object?>>(data["unknown_array_with_nulls"]);
        Assert.Null(arrayWithNulls[0]);
        Assert.Equal("text", arrayWithNulls[1]);
        Assert.Equal(3L, Convert.ToInt64(arrayWithNulls[2]));

        var childMap = Assert.IsAssignableFrom<IDictionary<string, object?>>(arrayWithNulls[3]);
        Assert.Null(childMap["child_null"]);
        Assert.Equal("child", childMap["child_text"]);
        Assert.False((bool) arrayWithNulls[4]!);

        var mapArray = Assert.IsAssignableFrom<IList<object?>>(data["unknown_map_array"]);
        Assert.Equal(2, mapArray.Count);
        var firstMap = Assert.IsAssignableFrom<IDictionary<string, object?>>(mapArray[0]);
        Assert.Equal("first", firstMap["name"]);
        Assert.Equal(1L, Convert.ToInt64(firstMap["score"]));
        Assert.True((bool) firstMap["active"]!);
        var secondMap = Assert.IsAssignableFrom<IDictionary<string, object?>>(mapArray[1]);
        Assert.Equal("second", secondMap["name"]);
        Assert.Equal(2L, Convert.ToInt64(secondMap["score"]));
        Assert.False((bool) secondMap["active"]!);

        var nested = Assert.IsAssignableFrom<IDictionary<string, object?>>(data["nested"]);
        Assert.Equal(42L, Convert.ToInt64(nested["answer"]));
        Assert.Equal("nested value", nested["label"]);
        Assert.Null(nested["null_value"]);
        Assert.Empty(Assert.IsAssignableFrom<IList<object?>>(nested["empty_values"]));
        Assert.Empty(Assert.IsAssignableFrom<IDictionary<string, object?>>(nested["empty_map"]));

        var nestedValues = Assert.IsAssignableFrom<IList<object?>>(nested["values"]);
        Assert.Equal(["one", "two"], nestedValues.Select(x => x as string));

        var deepNested = Assert.IsAssignableFrom<IDictionary<string, object?>>(nested["deep"]);
        Assert.Equal(84L, Convert.ToInt64(deepNested["answer"]));
        Assert.Null(deepNested["null_value"]);

        var directMap = Assert.IsAssignableFrom<IDictionary<string, object?>>(nested["direct_map"]);
        Assert.Equal("direct", directMap["text"]);
        Assert.Equal(9L, Convert.ToInt64(directMap["count"]));
        Assert.Equal((short) 7, Convert.ToInt16(directMap["short_count"]));
        var flags = Assert.IsAssignableFrom<IList<object?>>(directMap["flags"]);
        Assert.Equal([true, false], flags.Select(x => (bool) x!));
        var innerMap = Assert.IsAssignableFrom<IDictionary<string, object?>>(directMap["inner"]);
        Assert.Equal("inside", innerMap["value"]);

        Assert.IsType<DateTimeOffset>(data["observed_at"]);
        Assert.IsType<DateTimeOffset>(data["created_at"]);
        Assert.IsType<DateTimeOffset>(data["generated_at"]);

        var reference = Assert.IsAssignableFrom<IDocumentReference>(data["original_reference"]);
        Assert.Equal(document.Path, reference.Path);
    }

    public static void RawObjectDictionaryData(IDictionary<object, object?> data, IDocumentReference document)
    {
        Assert.NotNull(data);
        Assert.All(data.Keys, key => Assert.IsType<string>(key));
        RawDictionaryData(data.ToDictionary(x => (string) x.Key, x => x.Value), document);
    }

    public static async Task NullableDocumentAsync(IDocumentReference document, string? expectedMarker)
    {
        var snapshot = await document.GetDocumentSnapshotAsync<NullableFirestoreItem>(Source.Server);
        var item = Require(snapshot.Data);
        Assert.Equal(expectedMarker, item.QueryMarker);
        Assert.Null(item.NullableString);
        Assert.Null(item.NullableNumber);

        var map = Require(item.NullableMap);
        Assert.True(map.ContainsKey("inner_null"));
        Assert.Null(map["inner_null"]);
        Assert.Equal("nested-value", map["inner_value"]);

        var list = Require(item.NullableList);
        Assert.Equal(["first", null, "last"], list);
    }

    public static async Task Issue482NestedMapAsync(IDocumentReference document, string expectedMarker)
    {
        var snapshot = await document.GetDocumentSnapshotAsync<NullableFirestoreItem>(Source.Server);
        var item = Require(snapshot.Data);
        Assert.Equal(expectedMarker, item.QueryMarker);

        var map = Require(item.NullableMap);
        Assert.True(map.ContainsKey("sub_field"));
        Assert.Equal($"{expectedMarker}-value", map["sub_field"]);
    }

    public static void CrewCheckInDocument(CrewCheckIn? crewCheckIn, DateTime timestamp, DateTime logTimestamp)
    {
        Assert.NotNull(crewCheckIn);
        Assert.True(crewCheckIn!.EmergencyCheckIn);
        Assert.Equal("07:30", crewCheckIn.ClockInTime);
        Assert.Equal("north yard", crewCheckIn.YardLocation);
        Assert.InRange(Math.Abs(crewCheckIn.Timestamp.Ticks - timestamp.Ticks), 0, 10);

        var employee = Assert.Single(crewCheckIn.Employees);
        Assert.Equal("Ada Lovelace", employee.Name);
        Assert.Equal("Foreman", employee.Clazz);
        Assert.Equal(7, employee.Crew);
        Assert.Equal(["en", "de"], employee.Languages);
        Assert.Equal(["1001", "1002"], employee.JobNumbers);
        Assert.Equal("yard", employee.WorkType);
        Assert.Equal("ready", employee.Notes);

        var equipment = Assert.Single(employee.AssignedEquipment);
        Assert.Equal("Bucket Attachment", equipment.Name);
        Assert.Equal("Alice", equipment.Operator);

        var vehicle = Assert.Single(employee.AssignedVehicles);
        Assert.Equal("Truck 12", vehicle.Name);
        Assert.Equal("Bob", vehicle.Operator);

        Assert.Equal(2, crewCheckIn.YardAssets.Count);
        Assert.Equal("Truck 12", crewCheckIn.YardAssets[0].Name);
        Assert.Equal("Air Compressor", crewCheckIn.YardAssets[1].Name);

        var removedAsset = Assert.Single(crewCheckIn.RemovedAssets);
        Assert.Equal("Spare Saw", removedAsset.AssetName);
        Assert.Equal("damaged chainsaw", removedAsset.AssetDescription);
        Assert.Equal("maintenance", removedAsset.Reason);

        var log = Assert.Single(crewCheckIn.LogEntries);
        Assert.Equal("created", log.Action);
        Assert.Equal("check-in created", log.Message);
        Assert.InRange(Math.Abs(log.Timestamp.Ticks - logTimestamp.Ticks), 0, 10);
    }

    public static T Require<T>(T? value) where T : class
    {
        return value ?? throw new InvalidOperationException("Expected a non-null value.");
    }
}