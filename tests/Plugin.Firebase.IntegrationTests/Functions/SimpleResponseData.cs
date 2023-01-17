using System.Text.Json.Serialization;

namespace Plugin.Firebase.IntegrationTests.Functions
{
    [Preserve(AllMembers = true)]
    public sealed class SimpleResponseData
    {
        public SimpleResponseData()
        {
        }

        [JsonPropertyName("input_value")]
        public long InputValue { get; private set; }

        [JsonPropertyName("output_value")]
        public long OutputValue { get; private set; }
    }
}