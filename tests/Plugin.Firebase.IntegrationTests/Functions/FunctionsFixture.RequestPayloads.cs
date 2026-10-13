using System.Text.Json;
using Plugin.Firebase.Functions;

namespace Plugin.Firebase.IntegrationTests.Functions;

public sealed partial class FunctionsFixture
{
    [Fact]
    public async Task sends_object_and_array_json_request_payloads()
    {
        var sut = CrossFirebaseFunctions.Current;
        var callable = sut.GetHttpsCallable("echoRequestData");

        var objectResult = await callable.CallAsync<JsonElement>("{\"key\":\"value\",\"n\":3}");
        Assert.Equal("value", objectResult.GetProperty("key").GetString());
        Assert.Equal(3, objectResult.GetProperty("n").GetInt32());

        var arrayResult = await callable.CallAsync<JsonElement>("[1,\"two\",true]");
        Assert.Equal(JsonValueKind.Array, arrayResult.ValueKind);
        Assert.Equal(3, arrayResult.GetArrayLength());
        Assert.Equal(1, arrayResult[0].GetInt32());
        Assert.Equal("two", arrayResult[1].GetString());
        Assert.True(arrayResult[2].GetBoolean());
    }


    [Fact]
    public async Task sends_scalar_json_request_payloads()
    {
        var sut = CrossFirebaseFunctions.Current;
        var callable = sut.GetHttpsCallable("echoRequestData");

        var stringResult = await callable.CallAsync<JsonElement>("\"hello\"");
        Assert.Equal("hello", stringResult.GetString());

        var numberResult = await callable.CallAsync<JsonElement>("42.5");
        Assert.Equal(42.5, numberResult.GetDouble());

        var boolResult = await callable.CallAsync<JsonElement>("true");
        Assert.True(boolResult.GetBoolean());
    }


    [Fact]
    public async Task sends_null_json_request_payload()
    {
        var sut = CrossFirebaseFunctions.Current;

        // echoRequestData answers null, which deserializes to a default JsonElement
        var result = await sut.GetHttpsCallable("echoRequestData").CallAsync<JsonElement>("null");

        Assert.Equal(JsonValueKind.Undefined, result.ValueKind);
    }
}