using System.Text.Json;
using System.Text.Json.Serialization;

namespace Plugin.Firebase.IntegrationTests.Functions
{
    [Preserve(AllMembers = true)]
    public sealed class SimpleRequestData
    {
        public SimpleRequestData()
        {
        }

        public SimpleRequestData(long inputValue)
        {
            InputValue = inputValue;
        }

        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }

        [JsonPropertyName("input_value")]
        public long InputValue { get; private set; }
    }
}