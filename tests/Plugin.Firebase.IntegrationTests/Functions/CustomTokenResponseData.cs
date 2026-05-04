using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Plugin.Firebase.IntegrationTests.Functions;

[Preserve(AllMembers = true)]
public sealed class CustomTokenResponseData
{
    [JsonPropertyName("uid")]
    public string Uid { get; [UsedImplicitly] set; } = string.Empty;

    [JsonPropertyName("token")]
    public string Token { get; [UsedImplicitly] set; } = string.Empty;
}