using Newtonsoft.Json;

namespace Plugin.Firebase.IntegrationTests.Functions
{
    public sealed class SimpleResponseData
    {
        [JsonProperty("input_value")]
        public long InputValue { get; private set; }
        
        [JsonProperty("output_value")]
        public long OutputValue { get; private set; }
    }
}