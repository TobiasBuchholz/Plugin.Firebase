namespace Plugin.Firebase.IntegrationTests.Auth;

internal enum ByteBackedClaimLevel : byte
{
    None = 0,
    Max = 255
}

internal static class AuthTestPayloads
{
    public static Dictionary<string, object?> CreateNumericCustomClaims()
    {
        return new Dictionary<string, object?> {
            ["score"] = 7,
            ["byte_max"] = 255,
            ["above_byte"] = 300,
            ["short_max"] = 32767,
            ["above_short"] = 40000,
            ["int_max"] = int.MaxValue,
            ["above_int"] = 5_000_000_000,
            ["negative"] = -1,
            ["positive_fraction"] = 2.7,
            ["negative_fraction"] = -2.7,
            ["ratio"] = 1.5,
            ["flag"] = true,
            ["numeric_text"] = "123",
            ["levels"] = new[] { 7, 300 },
        };
    }

    public static Dictionary<string, object?> CreateNestedCustomClaims()
    {
        return new Dictionary<string, object?> {
            ["is_awesome"] = true,
            ["nested_object"] = new Dictionary<string, object?> {
                ["enabled"] = true,
                ["roles"] = new[] { "admin", "tester" },
                ["metadata"] = new Dictionary<string, object?> {
                    ["source"] = "emulator",
                    ["version"] = 2,
                },
                ["history"] = new[] {
                    new Dictionary<string, object?> {
                        ["action"] = "created",
                        ["count"] = 1,
                    },
                    new Dictionary<string, object?> {
                        ["action"] = "updated",
                        ["count"] = 2,
                    },
                },
                ["score"] = 7,
                ["ratio"] = 1.5,
                ["optional"] = null,
            },
            ["nested_array"] = new object[] {
                new Dictionary<string, object?> {
                    ["name"] = "first",
                    ["flags"] = new[] { true, false },
                },
                new Dictionary<string, object?> {
                    ["name"] = "second",
                    ["metadata"] = new Dictionary<string, object?> {
                        ["source"] = "emulator",
                    },
                },
            },
        };
    }
}