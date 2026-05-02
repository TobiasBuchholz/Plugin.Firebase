using System.Text.Json.Serialization;

namespace Plugin.Firebase.IntegrationTests.Functions
{
    [Preserve(AllMembers = true)]
    public sealed class CallableObjectResponseData
    {
        public CallableObjectResponseData()
        {
        }

        [JsonPropertyName("input_value")]
        public long InputValue { get; set; }

        [JsonPropertyName("output_value")]
        public long OutputValue { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("is_valid")]
        public bool IsValid { get; set; }

        [JsonPropertyName("nested")]
        public CallableNestedResponseData Nested { get; set; }

        [JsonPropertyName("items")]
        public List<CallableArrayItemData> Items { get; set; }

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; }

        [JsonPropertyName("scores")]
        public List<long> Scores { get; set; }
    }

    [Preserve(AllMembers = true)]
    public sealed class CallableNestedResponseData
    {
        public CallableNestedResponseData()
        {
        }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("count")]
        public long Count { get; set; }
    }

    [Preserve(AllMembers = true)]
    public sealed class CallableArrayItemData
    {
        public CallableArrayItemData()
        {
        }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("value")]
        public long Value { get; set; }
    }
}