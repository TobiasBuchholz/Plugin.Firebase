using NativeHttpMethod = Firebase.PerformanceMonitoring.HttpMethod;

namespace Plugin.Firebase.PerformanceMonitoring.Platforms.iOS.Extensions;

internal static class PerformanceMonitoringHttpMethodExtensions
{
    public static NativeHttpMethod ToNative(this PerformanceMonitoringHttpMethod method)
    {
        return method switch {
            PerformanceMonitoringHttpMethod.Get => NativeHttpMethod.Get,
            PerformanceMonitoringHttpMethod.Put => NativeHttpMethod.Put,
            PerformanceMonitoringHttpMethod.Post => NativeHttpMethod.Post,
            PerformanceMonitoringHttpMethod.Delete => NativeHttpMethod.Delete,
            PerformanceMonitoringHttpMethod.Head => NativeHttpMethod.Head,
            PerformanceMonitoringHttpMethod.Patch => NativeHttpMethod.Patch,
            PerformanceMonitoringHttpMethod.Options => NativeHttpMethod.Options,
            PerformanceMonitoringHttpMethod.Trace => NativeHttpMethod.Trace,
            PerformanceMonitoringHttpMethod.Connect => NativeHttpMethod.Connect,
            _ => throw new ArgumentOutOfRangeException(
                nameof(method),
                method,
                "Unsupported Performance Monitoring HTTP method."
            )
        };
    }
}
