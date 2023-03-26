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
        return JsonSerializer.Deserialize<TResponse>(result.Data.ToString());
    }
}