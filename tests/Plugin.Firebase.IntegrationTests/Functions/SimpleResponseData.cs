using Android.Runtime;
using Newtonsoft.Json;

namespace Plugin.Firebase.IntegrationTests.Functions
{
    [Preserve(AllMembers = true)]
    public sealed class SimpleResponseData
    {
        public SimpleResponseData()
        {
        }

        [JsonProperty("input_value")]
        public long InputValue { get; private set; }

        [JsonProperty("output_value")]
        public long OutputValue { get; private set; }
    }
}