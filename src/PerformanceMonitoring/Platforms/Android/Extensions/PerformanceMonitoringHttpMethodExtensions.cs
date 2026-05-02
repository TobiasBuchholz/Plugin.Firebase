namespace Plugin.Firebase.PerformanceMonitoring.Platforms.Android.Extensions;

internal static class PerformanceMonitoringHttpMethodExtensions
{
    public static string ToNative(this PerformanceMonitoringHttpMethod method)
    {
        return method switch {
            PerformanceMonitoringHttpMethod.Get => "GET",
            PerformanceMonitoringHttpMethod.Put => "PUT",
            PerformanceMonitoringHttpMethod.Post => "POST",
            PerformanceMonitoringHttpMethod.Delete => "DELETE",
            PerformanceMonitoringHttpMethod.Head => "HEAD",
            PerformanceMonitoringHttpMethod.Patch => "PATCH",
            PerformanceMonitoringHttpMethod.Options => "OPTIONS",
            PerformanceMonitoringHttpMethod.Trace => "TRACE",
            PerformanceMonitoringHttpMethod.Connect => "CONNECT",
            _ => throw new ArgumentOutOfRangeException(
                nameof(method),
                method,
                "Unsupported Performance Monitoring HTTP method."
            )
        };
    }
}
