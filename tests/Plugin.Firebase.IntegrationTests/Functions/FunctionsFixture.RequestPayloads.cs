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
    public async Task sends_array_of_objects_json_request_payload()
    {
        var sut = CrossFirebaseFunctions.Current;

        var result = await sut.GetHttpsCallable("echoRequestData").CallAsync<JsonElement>(
            "[{\"title\":\"first\",\"value\":1},{\"title\":\"second\",\"value\":2}]");

        AssertArrayJsonElement(result);
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
    public async Task sends_escaped_string_json_request_payload()
    {
        var sut = CrossFirebaseFunctions.Current;
        const string payload = """
            "line\nbreak \"quoted\" \u00e9 \\ slash"
            """;

        var result = await sut.GetHttpsCallable("echoRequestData").CallAsync<JsonElement>(payload);

        Assert.Equal(JsonValueKind.String, result.ValueKind);
        Assert.Equal("line\nbreak \"quoted\" \u00e9 \\ slash", result.GetString());
    }


    [Fact]
    public async Task sends_integer_json_request_payloads()
    {
        var sut = CrossFirebaseFunctions.Current;
        var callable = sut.GetHttpsCallable("echoRequestData");

        // iOS reads integers as int64 and Android as double, so stay within the range a double holds exactly
        var smallResult = await callable.CallAsync<JsonElement>("42");
        Assert.Equal(42, smallResult.GetInt64());

        var maxSafeResult = await callable.CallAsync<JsonElement>("9007199254740991");
        Assert.Equal(9007199254740991, maxSafeResult.GetInt64());
    }


    [Fact]
    public async Task sends_null_json_request_payload()
    {
        var sut = CrossFirebaseFunctions.Current;

        // echoRequestData answers null, which deserializes to a default JsonElement
        var result = await sut.GetHttpsCallable("echoRequestData").CallAsync<JsonElement>("null");

        Assert.Equal(JsonValueKind.Undefined, result.ValueKind);
    }


    [Theory]
    [InlineData("abc123")]
    [InlineData("True")]
    [InlineData("NaN")]
    [InlineData("'x'")]
    [InlineData("{key:\"value\"}")]
    public async Task rejects_invalid_json_request_payload(string payload)
    {
        var sut = CrossFirebaseFunctions.Current;
        var callable = sut.GetHttpsCallable("echoRequestData");

        await AssertRejectsInvalidJson(() => callable.CallAsync<JsonElement>(payload));
    }

    // Both platforms reject the payload before sending it, each with its own parser failure.
    private static Task AssertRejectsInvalidJson(Func<Task> call)
    {
#if ANDROID
        return Assert.ThrowsAsync<global::GoogleGson.JsonSyntaxException>(call);
#elif IOS
        return Assert.ThrowsAsync<Plugin.Firebase.Core.Exceptions.FirebaseException>(call);
#else
        throw new PlatformNotSupportedException();
#endif
    }
}