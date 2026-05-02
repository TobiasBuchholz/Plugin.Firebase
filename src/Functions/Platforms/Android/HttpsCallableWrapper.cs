using System.Text.Json;
using Android.Gms.Extensions;
using Firebase.Functions;
using GoogleGson;
using GoogleGson.Reflect;
using Java.Lang;
using Java.Util;

namespace Plugin.Firebase.Functions.Platforms.Android;

public sealed class HttpsCallableWrapper : IHttpsCallable
{
    private readonly HttpsCallableReference _httpsCallable;

    public HttpsCallableWrapper(HttpsCallableReference httpsCallable)
    {
        _httpsCallable = httpsCallable;
    }

    public Task CallAsync(string dataJson = null)
    {
        return _httpsCallable.Call(ConvertJsonToData(dataJson)).AsAsync();
    }

    private static Java.Lang.Object ConvertJsonToData(string dataJson = null)
    {
        return new Gson().FromJson(dataJson, TypeToken.GetParameterized(
                TypeToken.Get(Class.FromType(typeof(HashMap))).Type,
                TypeToken.Get(Class.FromType(typeof(Java.Lang.String))).Type,
                TypeToken.Get(Class.FromType(typeof(Java.Lang.Object))).Type)
            .Type);
    }

    public async Task<TResponse> CallAsync<TResponse>(string dataJson = null)
    {
        var result = await _httpsCallable.Call(ConvertJsonToData(dataJson)).AsAsync<HttpsCallableResult>();
        return DeserializeResponse<TResponse>(result.Data);
    }

    private static TResponse DeserializeResponse<TResponse>(Java.Lang.Object data)
    {
        if(data == null) {
            return default;
        }

        if(data is Java.Lang.String stringData) {
            return DeserializeStringResponse<TResponse>(stringData.ToString());
        }

        var json = ConvertDataToJson(data);
        if(json == null) {
            return default;
        }

        if(typeof(TResponse) == typeof(string)) {
            return (TResponse) (object) json;
        }

        return JsonSerializer.Deserialize<TResponse>(json);
    }

    private static TResponse DeserializeStringResponse<TResponse>(string value)
    {
        if(value == null) {
            return default;
        }

        if(typeof(TResponse) == typeof(string)) {
            return (TResponse) (object) value;
        }

        var json = IsJson(value)
            ? value
            : JsonSerializer.Serialize(value);
        return JsonSerializer.Deserialize<TResponse>(json);
    }

    private static bool IsJson(string value)
    {
        try {
            using var _ = JsonDocument.Parse(value);
            return true;
        } catch(JsonException) {
            return false;
        }
    }

    private static string ConvertDataToJson(Java.Lang.Object data)
    {
        return data switch {
            null => null,
            _ => new Gson().ToJson(data)
        };
    }
}