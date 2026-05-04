using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Plugin.Firebase.IntegrationTests.Functions;

[Preserve(AllMembers = true)]
public sealed class CallableObjectResponseData
{
    [JsonPropertyName("input_value")]
    public long InputValue { get; [UsedImplicitly] set; }

    [JsonPropertyName("output_value")]
    public long OutputValue { get; [UsedImplicitly] set; }

    [JsonPropertyName("message")]
    public string Message { get; [UsedImplicitly] set; } = string.Empty;

    [JsonPropertyName("is_valid")]
    public bool IsValid { get; [UsedImplicitly] set; }

    [JsonPropertyName("nested")]
    public CallableNestedResponseData Nested { get; [UsedImplicitly] set; } = new();

    [JsonPropertyName("items")]
    // ReSharper disable CollectionNeverUpdated.Global
    public List<CallableArrayItemData> Items { get; [UsedImplicitly] set; } = [];

    [JsonPropertyName("tags")]
    public List<string> Tags { get; [UsedImplicitly] set; } = [];

    [JsonPropertyName("scores")]
    public List<long> Scores { get; [UsedImplicitly] set; } = [];
    // ReSharper restore CollectionNeverUpdated.Global
}

[Preserve(AllMembers = true)]
public sealed class CallableNestedResponseData
{
    [JsonPropertyName("name")]
    public string Name { get; [UsedImplicitly] set; } = string.Empty;

    [JsonPropertyName("count")]
    public long Count { get; [UsedImplicitly] set; }
}

[Preserve(AllMembers = true)]
public sealed class CallableArrayItemData
{
    [JsonPropertyName("title")]
    public string Title { get; [UsedImplicitly] set; } = string.Empty;

    [JsonPropertyName("value")]
    public long Value { get; [UsedImplicitly] set; }
}