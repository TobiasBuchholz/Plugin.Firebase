using System.Text.Json;
using Plugin.Firebase.Functions;
using Firebase.CloudFunctions;
using Foundation;
using Plugin.Firebase.Common;

namespace Plugin.Firebase.iOS.Functions;

public sealed class HttpsCallableWrapper : IHttpsCallable
{
    private readonly HttpsCallable _httpsCallable;

    public HttpsCallableWrapper(HttpsCallable httpsCallable)
    {
        _httpsCallable = httpsCallable;
    }

    public Task CallAsync(string dataJson = null)
    {
        return _httpsCallable.CallAsync(ConvertJsonToData(dataJson));
    }

    private static NSObject ConvertJsonToData(string dataJson = null)
    {
        if(dataJson == null) {
            return null;
        } else {
            var data = NSJsonSerialization.Deserialize(NSData.FromString(dataJson, NSStringEncoding.UTF8), 0, out var error);
            if(error != null) {
                throw new FirebaseException(error.LocalizedDescription);
            }
            return data;
        }
    }

    public async Task<TResponse> CallAsync<TResponse>(string dataJson = null)
    {
        var result = await _httpsCallable.CallAsync(ConvertJsonToData(dataJson));
        return JsonSerializer.Deserialize<TResponse>(result.Data.ToString());
    }
}