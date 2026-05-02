namespace Plugin.Firebase.PerformanceMonitoring;

/// <summary>
/// HTTP methods supported by Firebase Performance Monitoring custom HTTP metrics.
/// </summary>
public enum PerformanceMonitoringHttpMethod
{
    /// <summary>
    /// HTTP GET.
    /// </summary>
    Get,

    /// <summary>
    /// HTTP PUT.
    /// </summary>
    Put,

    /// <summary>
    /// HTTP POST.
    /// </summary>
    Post,

    /// <summary>
    /// HTTP DELETE.
    /// </summary>
    Delete,

    /// <summary>
    /// HTTP HEAD.
    /// </summary>
    Head,

    /// <summary>
    /// HTTP PATCH.
    /// </summary>
    Patch,

    /// <summary>
    /// HTTP OPTIONS.
    /// </summary>
    Options,

    /// <summary>
    /// HTTP TRACE.
    /// </summary>
    Trace,

    /// <summary>
    /// HTTP CONNECT.
    /// </summary>
    Connect
}
