using System.Net.Http;
using NativeHttpMethod = Firebase.PerformanceMonitoring.HttpMethod;

namespace Plugin.Firebase.PerformanceMonitoring.Platforms.iOS.Extensions;

internal static class HttpMethodExtensions
{
    public static NativeHttpMethod ToNative(this HttpMethod httpMethod)
    {
        ArgumentNullException.ThrowIfNull(httpMethod);
        return httpMethod.Method switch {
            "GET" => NativeHttpMethod.Get,
            "PUT" => NativeHttpMethod.Put,
            "POST" => NativeHttpMethod.Post,
            "DELETE" => NativeHttpMethod.Delete,
            "HEAD" => NativeHttpMethod.Head,
            "PATCH" => NativeHttpMethod.Patch,
            "OPTIONS" => NativeHttpMethod.Options,
            "TRACE" => NativeHttpMethod.Trace,
            "CONNECT" => NativeHttpMethod.Connect,
            _ => throw new ArgumentOutOfRangeException(
                nameof(httpMethod),
                httpMethod,
                "Unsupported Performance Monitoring HTTP method."
            )
        };
    }
}
