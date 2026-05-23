using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Plugin.Firebase.IntegrationTests.Functions;

[Preserve(AllMembers = true)]
public sealed class SimpleResponseData
{
    [JsonPropertyName("input_value")]
    public long InputValue { get; [UsedImplicitly] set; }

    [JsonPropertyName("output_value")]
    public long OutputValue { get; [UsedImplicitly] set; }
}