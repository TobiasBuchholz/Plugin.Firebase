namespace Plugin.Firebase.IntegrationTests.Auth;

internal static class AuthTestPayloads
{
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