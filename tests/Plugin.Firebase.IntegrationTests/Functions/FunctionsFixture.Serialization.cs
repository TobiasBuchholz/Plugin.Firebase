using System.Text.Json;
using Plugin.Firebase.Auth;
using Plugin.Firebase.Functions;

namespace Plugin.Firebase.IntegrationTests.Functions;

public sealed partial class FunctionsFixture
{
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

        // ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local
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
        // ReSharper restore ParameterOnlyUsedForPreconditionCheck.Local
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
        Assert.Equal(0, await sut.GetHttpsCallable("returnNullPayload").CallAsync<long>());
        Assert.Equal(JsonValueKind.Undefined, (await sut.GetHttpsCallable("returnNullPayload").CallAsync<JsonElement>()).ValueKind);
    }

}