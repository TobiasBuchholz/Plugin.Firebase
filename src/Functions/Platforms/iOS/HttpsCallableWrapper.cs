using System.Text.Json;
using Firebase.CloudFunctions;
using Plugin.Firebase.Core.Exceptions;

namespace Plugin.Firebase.Functions.Platforms.iOS;

/// <summary>
/// iOS implementation of <see cref="IHttpsCallable"/> that wraps the native <see cref="HttpsCallable"/> type.
/// </summary>
public sealed class HttpsCallableWrapper : IHttpsCallable
{
    private readonly HttpsCallable _httpsCallable;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpsCallableWrapper"/> class.
    /// </summary>
    /// <param name="httpsCallable">The native iOS HTTPS callable to wrap.</param>
    public HttpsCallableWrapper(HttpsCallable httpsCallable)
    {
        _httpsCallable = httpsCallable;
    }

    /// <inheritdoc/>
    public Task CallAsync(string dataJson = null)
    {
        return dataJson == null
            ? _httpsCallable.CallAsync()
            : _httpsCallable.CallAsync(ConvertJsonToData(dataJson));
    }

    private static NSObject ConvertJsonToData(string dataJson)
    {
        var data = NSJsonSerialization.Deserialize(
            NSData.FromString(dataJson, NSStringEncoding.UTF8),
            0,
            out var error
        );
        if(error != null) {
            throw new FirebaseException(error.LocalizedDescription);
        }
        return data;
    }

    /// <inheritdoc/>
    public async Task<TResponse> CallAsync<TResponse>(string dataJson = null)
    {
        var result = await _httpsCallable.CallAsync(ConvertJsonToData(dataJson));
        return JsonSerializer.Deserialize<TResponse>(result.Data.ToString());
    }
}