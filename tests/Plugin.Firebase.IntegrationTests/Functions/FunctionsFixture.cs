using System.Text.Json;
using Plugin.Firebase.Functions;
using Xunit.Sdk;

namespace Plugin.Firebase.IntegrationTests.Functions
{
    [Collection("Sequential")]
    [TestLogging]
    [Preserve(AllMembers = true)]
    public sealed class FunctionsFixture
    {
        private const string RegionalFunctionsRegion = "southamerica-east1";
        private const string RegionalPingFunctionName = "regionalPing";
        private const long RegionalPingOutputValue = 541;

        [Fact]
        public async Task executes_simple_callable_function()
        {
            var sut = CrossFirebaseFunctions.Current;
            await sut.GetHttpsCallable("convertToLeet").CallAsync();
        }

        [Fact]
        public async Task executes_callable_function_with_json_body()
        {
            var sut = CrossFirebaseFunctions.Current;
            var json = new SimpleRequestData(123).ToJson();
            await sut.GetHttpsCallable("convertToLeet").CallAsync(json);
        }

        [Fact]
        public async Task executes_callable_function_with_json_body_and_response()
        {
            var sut = CrossFirebaseFunctions.Current;
            var json = new SimpleRequestData(123).ToJson();
            var response = await sut.GetHttpsCallable("convertToLeet").CallAsync<SimpleResponseData>(json);

            Assert.Equal(123, response.InputValue);
            Assert.Equal(1337, response.OutputValue);
        }

        [Fact]
        public async Task deserializes_callable_function_with_json_string_response_as_raw_string_and_json_value()
        {
            var sut = CrossFirebaseFunctions.Current;
            var json = new SimpleRequestData(123).ToJson();

            var responseJson = await sut.GetHttpsCallable("convertToLeet").CallAsync<string>(json);
            using var response = JsonDocument.Parse(responseJson);
            AssertSimpleResponse(response.RootElement, 123);

            var responseElement = await sut.GetHttpsCallable("convertToLeet").CallAsync<JsonElement>(json);
            AssertSimpleResponse(responseElement, 123);
        }

        [Fact]
        public async Task deserializes_callable_function_with_native_object_response()
        {
            var sut = CrossFirebaseFunctions.Current;
            var json = new SimpleRequestData(123).ToJson();
            var response = await sut.GetHttpsCallable("returnObjectPayload").CallAsync<CallableObjectResponseData>(json);

            AssertObjectResponse(response, 123);
        }

        [Fact]
        public async Task deserializes_callable_function_with_native_object_response_without_json_body()
        {
            var sut = CrossFirebaseFunctions.Current;
            var response = await sut.GetHttpsCallable("returnObjectPayload").CallAsync<CallableObjectResponseData>();

            AssertObjectResponse(response, 0);
        }

        [Fact]
        public async Task returns_callable_native_object_response_as_json_string()
        {
            var sut = CrossFirebaseFunctions.Current;
            var json = new SimpleRequestData(123).ToJson();
            var responseJson = await sut.GetHttpsCallable("returnObjectPayload").CallAsync<string>(json);

            using var response = JsonDocument.Parse(responseJson);
            var root = response.RootElement;
            Assert.Equal(123, root.GetProperty("input_value").GetInt64());
            Assert.Equal(1337, root.GetProperty("output_value").GetInt64());
            Assert.Equal("object response", root.GetProperty("message").GetString());
            Assert.True(root.GetProperty("is_valid").GetBoolean());
            Assert.Equal("nested response", root.GetProperty("nested").GetProperty("name").GetString());
            Assert.Equal(2, root.GetProperty("items").GetArrayLength());
        }

        [Fact]
        public async Task deserializes_callable_function_with_native_object_response_as_json_value()
        {
            var sut = CrossFirebaseFunctions.Current;
            var json = new SimpleRequestData(123).ToJson();
            var response = await sut.GetHttpsCallable("returnObjectPayload").CallAsync<JsonElement>(json);

            AssertObjectJsonElement(response, 123);
        }

        [Fact]
        public async Task deserializes_callable_function_with_native_array_response()
        {
            var sut = CrossFirebaseFunctions.Current;
            var response = await sut.GetHttpsCallable("returnArrayPayload").CallAsync<List<CallableArrayItemData>>();

            Assert.Collection(
                response,
                first => {
                    Assert.Equal("first", first.Title);
                    Assert.Equal(1, first.Value);
                },
                second => {
                    Assert.Equal("second", second.Title);
                    Assert.Equal(2, second.Value);
                });
        }

        [Fact]
        public async Task deserializes_callable_function_with_native_array_response_as_json_string_and_json_value()
        {
            var sut = CrossFirebaseFunctions.Current;

            var responseJson = await sut.GetHttpsCallable("returnArrayPayload").CallAsync<string>();
            using var response = JsonDocument.Parse(responseJson);
            AssertArrayJsonElement(response.RootElement);

            var responseElement = await sut.GetHttpsCallable("returnArrayPayload").CallAsync<JsonElement>();
            AssertArrayJsonElement(responseElement);
        }

        [Fact]
        public async Task deserializes_callable_function_with_native_string_response()
        {
            var sut = CrossFirebaseFunctions.Current;
            var response = await sut.GetHttpsCallable("returnStringPayload").CallAsync<string>();

            Assert.Equal("callable-string", response);
        }

        [Fact]
        public async Task deserializes_callable_function_with_native_string_response_as_json_value()
        {
            var sut = CrossFirebaseFunctions.Current;
            var response = await sut.GetHttpsCallable("returnStringPayload").CallAsync<JsonElement>();

            Assert.Equal(JsonValueKind.String, response.ValueKind);
            Assert.Equal("callable-string", response.GetString());
        }

        [Fact]
        public async Task deserializes_callable_function_with_native_escaped_string_response()
        {
            var sut = CrossFirebaseFunctions.Current;
            const string expected = "escaped \"quote\" and backslash \\\\ path";

            var response = await sut.GetHttpsCallable("returnEscapedStringPayload").CallAsync<string>();
            Assert.Equal(expected, response);

            var responseElement = await sut.GetHttpsCallable("returnEscapedStringPayload").CallAsync<JsonElement>();
            Assert.Equal(JsonValueKind.String, responseElement.ValueKind);
            Assert.Equal(expected, responseElement.GetString());
        }

        [Fact]
        public async Task deserializes_callable_function_with_native_number_response()
        {
            var sut = CrossFirebaseFunctions.Current;
            var response = await sut.GetHttpsCallable("returnNumberPayload").CallAsync<long>();

            Assert.Equal(42, response);
        }

        [Fact]
        public async Task deserializes_callable_function_with_native_number_response_as_json_string_and_json_value()
        {
            var sut = CrossFirebaseFunctions.Current;

            Assert.Equal("42", await sut.GetHttpsCallable("returnNumberPayload").CallAsync<string>());

            var response = await sut.GetHttpsCallable("returnNumberPayload").CallAsync<JsonElement>();
            Assert.Equal(JsonValueKind.Number, response.ValueKind);
            Assert.Equal(42, response.GetInt64());
        }

        [Fact]
        public async Task deserializes_callable_function_with_native_boolean_response()
        {
            var sut = CrossFirebaseFunctions.Current;
            var response = await sut.GetHttpsCallable("returnBooleanPayload").CallAsync<bool>();

            Assert.True(response);
        }

        [Fact]
        public async Task deserializes_callable_function_with_native_boolean_response_as_json_string_and_json_value()
        {
            var sut = CrossFirebaseFunctions.Current;

            Assert.Equal("true", await sut.GetHttpsCallable("returnBooleanPayload").CallAsync<string>());

            var response = await sut.GetHttpsCallable("returnBooleanPayload").CallAsync<JsonElement>();
            Assert.Equal(JsonValueKind.True, response.ValueKind);
            Assert.True(response.GetBoolean());
        }

        [Fact]
        public async Task returns_default_for_callable_function_with_native_null_response()
        {
            var sut = CrossFirebaseFunctions.Current;

            Assert.Null(await sut.GetHttpsCallable("returnNullPayload").CallAsync<CallableObjectResponseData>());
            Assert.Null(await sut.GetHttpsCallable("returnNullPayload").CallAsync<string>());
            Assert.Equal(default, await sut.GetHttpsCallable("returnNullPayload").CallAsync<long>());
            Assert.Equal(JsonValueKind.Undefined, (await sut.GetHttpsCallable("returnNullPayload").CallAsync<JsonElement>()).ValueKind);
        }

        [Fact]
        public async Task throws_exception_when_function_does_not_exist()
        {
            var sut = CrossFirebaseFunctions.Current;
            await Assert.ThrowsAnyAsync<Exception>(() => sut.GetHttpsCallable("doesNotExist").CallAsync());
        }

        [Fact]
        public async Task uses_configured_region_when_initialize_runs_after_emulator_configuration()
        {
            SkipIfRealBackend();

            try {
                ResetFunctionsToDefaultRegion();
                ConfigureFunctionsEmulator();

                CrossFirebaseFunctions.Initialize(RegionalFunctionsRegion);

                var response = await CrossFirebaseFunctions.Current
                    .GetHttpsCallable(RegionalPingFunctionName)
                    .CallAsync<SimpleResponseData>("{}");

                Assert.Equal(RegionalPingOutputValue, response.OutputValue);
            } finally {
                RestoreDefaultFunctionsConfiguration();
            }
        }

        [Fact]
        public async Task uses_configured_region_after_is_supported_was_checked()
        {
            SkipIfRealBackend();

            try {
                ResetFunctionsToDefaultRegion();
                Assert.True(CrossFirebaseFunctions.IsSupported);

                CrossFirebaseFunctions.Initialize(RegionalFunctionsRegion);
                ConfigureFunctionsEmulator();

                var response = await CrossFirebaseFunctions.Current
                    .GetHttpsCallable(RegionalPingFunctionName)
                    .CallAsync<SimpleResponseData>("{}");

                Assert.Equal(RegionalPingOutputValue, response.OutputValue);
            } finally {
                RestoreDefaultFunctionsConfiguration();
            }
        }

        private static void SkipIfRealBackend()
        {
            if(IntegrationTestEnvironment.UsesRealBackend) {
                throw SkipException.ForSkip(
                    "This test uses the emulator-only regional function fixture.");
            }
        }

        private static void RestoreDefaultFunctionsConfiguration()
        {
            ResetFunctionsToDefaultRegion();
            ConfigureFunctionsEmulator();
        }

        private static void ResetFunctionsToDefaultRegion()
        {
            CrossFirebaseFunctions.Initialize(null);
            CrossFirebaseFunctions.Dispose();
        }

        private static void ConfigureFunctionsEmulator()
        {
            var functions = IntegrationTestEnvironment.FunctionsEmulatorEndpoint;
            CrossFirebaseFunctions.Current.UseEmulator(functions.Host, functions.Port);
        }

        private static void AssertSimpleResponse(JsonElement response, long expectedInputValue)
        {
            Assert.Equal(JsonValueKind.Object, response.ValueKind);
            Assert.Equal(expectedInputValue, response.GetProperty("input_value").GetInt64());
            Assert.Equal(1337, response.GetProperty("output_value").GetInt64());
        }

        private static void AssertObjectResponse(CallableObjectResponseData response, long expectedInputValue)
        {
            Assert.NotNull(response);
            Assert.Equal(expectedInputValue, response.InputValue);
            Assert.Equal(1337, response.OutputValue);
            Assert.Equal("object response", response.Message);
            Assert.True(response.IsValid);
            Assert.NotNull(response.Nested);
            Assert.Equal("nested response", response.Nested.Name);
            Assert.Equal(2, response.Nested.Count);
            Assert.Collection(
                response.Items,
                first => {
                    Assert.Equal("first", first.Title);
                    Assert.Equal(1, first.Value);
                },
                second => {
                    Assert.Equal("second", second.Title);
                    Assert.Equal(2, second.Value);
                });
            Assert.Equal(new[] { "alpha", "beta" }, response.Tags);
            Assert.Equal(new long[] { 3, 5, 8 }, response.Scores);
        }

        private static void AssertObjectJsonElement(JsonElement response, long expectedInputValue)
        {
            Assert.Equal(JsonValueKind.Object, response.ValueKind);
            Assert.Equal(expectedInputValue, response.GetProperty("input_value").GetInt64());
            Assert.Equal(1337, response.GetProperty("output_value").GetInt64());
            Assert.Equal("object response", response.GetProperty("message").GetString());
            Assert.True(response.GetProperty("is_valid").GetBoolean());
            Assert.Equal("nested response", response.GetProperty("nested").GetProperty("name").GetString());
            Assert.Equal(2, response.GetProperty("nested").GetProperty("count").GetInt64());
            AssertArrayJsonElement(response.GetProperty("items"));
            Assert.Equal(new[] { "alpha", "beta" }, response.GetProperty("tags").EnumerateArray().Select(x => x.GetString()));
            Assert.Equal(new long[] { 3, 5, 8 }, response.GetProperty("scores").EnumerateArray().Select(x => x.GetInt64()));
        }

        private static void AssertArrayJsonElement(JsonElement response)
        {
            Assert.Equal(JsonValueKind.Array, response.ValueKind);
            Assert.Equal(2, response.GetArrayLength());
            Assert.Equal("first", response[0].GetProperty("title").GetString());
            Assert.Equal(1, response[0].GetProperty("value").GetInt64());
            Assert.Equal("second", response[1].GetProperty("title").GetString());
            Assert.Equal(2, response[1].GetProperty("value").GetInt64());
        }
    }
}
