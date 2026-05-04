using System.Text.Json;
using Plugin.Firebase.Auth;
using Plugin.Firebase.Functions;

namespace Plugin.Firebase.IntegrationTests.Functions;

[Collection("Sequential")]
[TestLogging]
[IntegrationTestFixture(IntegrationTestPackage.Functions)]
[Preserve(AllMembers = true)]
public sealed partial class FunctionsFixture
{
    private const string RegionalFunctionsRegion = "southamerica-east1";
    private const string RegionalPingFunctionName = "regionalPing";
    private const long RegionalPingOutputValue = 541;
    private static readonly string[] Expected = ["alpha", "beta"];

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

    // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
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
        // ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local
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
        // ReSharper restore ParameterOnlyUsedForPreconditionCheck.Local
        Assert.Equal(Expected, response.Tags);
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
        Assert.Equal(["alpha", "beta"], response.GetProperty("tags").EnumerateArray().Select(x => x.GetString()!));
        Assert.Equal([3, 5, 8], response.GetProperty("scores").EnumerateArray().Select(x => x.GetInt64()));
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