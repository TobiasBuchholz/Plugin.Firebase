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
        var result = dataJson == null
            ? await _httpsCallable.CallAsync()
            : await _httpsCallable.CallAsync(ConvertJsonToData(dataJson));
        return DeserializeResponse<TResponse>(result.Data);
    }

    private static TResponse DeserializeResponse<TResponse>(NSObject data)
    {
        if(data == null || data == NSNull.Null) {
            return default;
        }

        if(data is NSString stringData) {
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

    private static string ConvertDataToJson(NSObject data)
    {
        if(data == null || data == NSNull.Null) {
            return null;
        }

        var jsonData = NSJsonSerialization.Serialize(
            data,
            NSJsonWritingOptions.FragmentsAllowed,
            out var error);
        if(error != null) {
            throw new FirebaseException(error.LocalizedDescription);
        }

        return jsonData == null
            ? null
            : NSString.FromData(jsonData, NSStringEncoding.UTF8)?.ToString();
    }
}
