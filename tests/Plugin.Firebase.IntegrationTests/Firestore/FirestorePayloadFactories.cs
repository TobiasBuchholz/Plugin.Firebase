namespace Plugin.Firebase.IntegrationTests.Firestore;

internal static class NullableFirestoreItemFactory
{
    public static NullableFirestoreItem CreateNullableItem(string? queryMarker)
    {
        return new NullableFirestoreItem(
            nullableString: null,
            nullableNumber: null,
            nullableMap: CreateNestedMap(),
            nullableList: CreateNullableList(),
            queryMarker: queryMarker
        );
    }

    public static NullableFirestoreItem CreateNonNullItem(string queryMarker)
    {
        return new NullableFirestoreItem(
            nullableString: "seed",
            nullableNumber: 42,
            nullableMap: new Dictionary<string, object?> {
                { "inner_null", "seed" },
                { "inner_value", "seed" }
            },
            nullableList: ["seed"],
            queryMarker: queryMarker
        );
    }

    public static Dictionary<object, object?> CreateNullableDictionary(string? queryMarker)
    {
        return new Dictionary<object, object?> {
            { NullableFirestoreItem.NullableStringField, null },
            { NullableFirestoreItem.NullableNumberField, null },
            { NullableFirestoreItem.NullableMapField, CreateNestedMap() },
            { NullableFirestoreItem.NullableListField, CreateNullableList() },
            { NullableFirestoreItem.QueryMarkerField, queryMarker }
        };
    }

    public static (object Key, object? Value)[] CreateNullableTuples(string? queryMarker)
    {
        return [
            (NullableFirestoreItem.NullableStringField, null),
            (NullableFirestoreItem.NullableNumberField, null),
            (NullableFirestoreItem.NullableMapField, CreateNestedMap()),
            (NullableFirestoreItem.NullableListField, CreateNullableList()),
            (NullableFirestoreItem.QueryMarkerField, queryMarker)
        ];
    }

    public static Dictionary<object, object?> CreateNullUpdate(string? queryMarker)
    {
        return new Dictionary<object, object?> {
            { NullableFirestoreItem.NullableStringField, null },
            { NullableFirestoreItem.NullableNumberField, null },
            { $"{NullableFirestoreItem.NullableMapField}.inner_null", null },
            { $"{NullableFirestoreItem.NullableMapField}.inner_value", "nested-value" },
            { NullableFirestoreItem.NullableListField, CreateNullableList() },
            { NullableFirestoreItem.QueryMarkerField, queryMarker }
        };
    }

    public static (string Key, object? Value)[] CreateNullUpdateTuples(string? queryMarker)
    {
        return [
            (NullableFirestoreItem.NullableStringField, null),
            (NullableFirestoreItem.NullableNumberField, null),
            ($"{NullableFirestoreItem.NullableMapField}.inner_null", null),
            ($"{NullableFirestoreItem.NullableMapField}.inner_value", "nested-value"),
            (NullableFirestoreItem.NullableListField, CreateNullableList()),
            (NullableFirestoreItem.QueryMarkerField, queryMarker)
        ];
    }

    public static Dictionary<object, object?> CreateIssue482NestedMapUpdate(string marker)
    {
        return new Dictionary<object, object?> {
            {
                NullableFirestoreItem.NullableMapField,
                new Dictionary<object, object?> {
                    { "sub_field", $"{marker}-value" }
                }
            },
            { NullableFirestoreItem.QueryMarkerField, marker }
        };
    }

    public static Dictionary<string, object?> CreateNestedMap()
    {
        return new Dictionary<string, object?> {
            { "inner_null", null },
            { "inner_value", "nested-value" }
        };
    }

    public static List<object?> CreateNullableList()
    {
        return ["first", null, "last"];
    }
}

internal static class DictionaryContainerFactory
{
    public static DictionaryContainer CreateDefault()
    {
        return new DictionaryContainer(
            metadata: new Dictionary<string, object?> {
                { "title", "container" },
                { "count", 5L },
                { "nullable", null },
                {
                    "details",
                    new Dictionary<object, object?> {
                        { "enabled", true },
                        { "label", "nested" }
                    }
                }
            },
            scores: new Dictionary<string, long> {
                { "first", 10L },
                { "second", 20L }
            },
            flags: new Dictionary<string, bool> {
                { "active", true },
                { "archived", false }
            },
            mixedLists: new Dictionary<string, IList<object?>> {
                { "values", ["first", null, 3L] },
                { "empty", Array.Empty<object?>() }
            },
            nested: new Dictionary<string, Dictionary<string, object?>> {
                {
                    "outer",
                    new Dictionary<string, object?> {
                        { "name", "outer" },
                        { "count", 2L }
                    }
                }
            });
    }
}

internal sealed record CrewCheckInScenario(
    CrewCheckIn CrewCheckIn,
    DateTime Timestamp,
    DateTime LogTimestamp);

internal static class CrewCheckInFactory
{
    public static CrewCheckInScenario CreateIssue422Scenario()
    {
        var timestamp = new DateTime(2025, 2, 27, 14, 48, 2, 625, DateTimeKind.Utc);
        var logTimestamp = new DateTime(2025, 2, 27, 14, 49, 3, 123, DateTimeKind.Utc);
        var assignedEquipment = new List<CrewCheckInAsset> {
            new("bucket truck attachment", "Bucket Attachment", "Alice", "equipment")
        };
        var assignedVehicles = new List<CrewCheckInAsset> {
            new("crew truck", "Truck 12", "Bob", "vehicle")
        };
        var yardAssets = new List<CrewCheckInAsset> {
            new("crew truck", "Truck 12", "Bob", "vehicle"),
            new("compressor", "Air Compressor", "Charlie", "equipment")
        };
        var crewCheckIn = new CrewCheckIn(
            employees: [
                new(
                    "Ada Lovelace",
                    "Foreman",
                    7,
                    ["en", "de"],
                    assignedEquipment,
                    assignedVehicles,
                    "07:30",
                    "checked-in",
                    "yard",
                    ["1001", "1002"],
                    "ready")
            ],
            yardAssets: yardAssets,
            clockInTime: "07:30",
            yardLocation: "north yard",
            emergencyCheckIn: true,
            removedAssets: [new("Spare Saw", "damaged chainsaw", "maintenance")],
            logEntries: [new(logTimestamp, "created", "check-in created")],
            timestamp: timestamp);

        return new CrewCheckInScenario(crewCheckIn, timestamp, logTimestamp);
    }
}