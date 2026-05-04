using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Plugin.Firebase.IntegrationTests.Functions;

[Preserve(AllMembers = true)]
public sealed class AuthContextResponseData
{
    [JsonPropertyName("has_auth")]
    public bool HasAuth { get; [UsedImplicitly] set; }

    [JsonPropertyName("uid")]
    public string? Uid { get; [UsedImplicitly] set; }

    [JsonPropertyName("token_email")]
    public string? TokenEmail { get; [UsedImplicitly] set; }

    [JsonPropertyName("input_value")]
    public long? InputValue { get; [UsedImplicitly] set; }
}