using Newtonsoft.Json;

namespace Plugin.Firebase.IntegrationTests.Functions
{
    public sealed class SimpleRequestData
    {
        public SimpleRequestData(long inputValue)
        {
            InputValue = inputValue;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        [JsonProperty("input_value")]
        public long InputValue { get; private set; }
    }
}