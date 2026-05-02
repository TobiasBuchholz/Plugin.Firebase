using System.Net.Http;

namespace Plugin.Firebase.PerformanceMonitoring.Platforms.Android.Extensions;

internal static class HttpMethodExtensions
{
    public static string ToNative(this HttpMethod httpMethod)
    {
        ArgumentNullException.ThrowIfNull(httpMethod);
        return httpMethod.Method switch {
            "GET" => "GET",
            "PUT" => "PUT",
            "POST" => "POST",
            "DELETE" => "DELETE",
            "HEAD" => "HEAD",
            "PATCH" => "PATCH",
            "OPTIONS" => "OPTIONS",
            "TRACE" => "TRACE",
            "CONNECT" => "CONNECT",
            _ => throw new ArgumentOutOfRangeException(
                nameof(httpMethod),
                httpMethod,
                "Unsupported Performance Monitoring HTTP method."
            )
        };
    }
}
