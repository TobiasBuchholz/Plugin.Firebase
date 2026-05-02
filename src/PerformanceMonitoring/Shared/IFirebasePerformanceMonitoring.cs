using System.Net.Http;

namespace Plugin.Firebase.PerformanceMonitoring;

/// <summary>
/// Firebase Performance Monitoring service for custom code traces and custom network request metrics.
/// </summary>
public interface IFirebasePerformanceMonitoring : IDisposable
{
    /// <summary>
    /// Gets or sets whether Performance Monitoring data collection is enabled for this app instance.
    /// </summary>
    bool IsDataCollectionEnabled { get; set; }

    /// <summary>
    /// Creates a new custom trace with the given name.
    /// </summary>
    /// <param name="traceName">The trace name.</param>
    /// <returns>A new custom trace.</returns>
    IFirebasePerformanceTrace NewTrace(string traceName);

    /// <summary>
    /// Creates and starts a new custom trace with the given name.
    /// </summary>
    /// <param name="traceName">The trace name.</param>
    /// <returns>A started custom trace.</returns>
    IFirebasePerformanceTrace StartTrace(string traceName);

    /// <summary>
    /// Creates a custom HTTP network request metric.
    /// </summary>
    /// <param name="url">The request URL.</param>
    /// <param name="httpMethod">The request HTTP method.</param>
    /// <returns>A new custom HTTP metric.</returns>
    IFirebasePerformanceHttpMetric NewHttpMetric(
        string url,
        HttpMethod httpMethod
    );

    /// <summary>
    /// Creates a custom HTTP network request metric.
    /// </summary>
    /// <param name="url">The request URL.</param>
    /// <param name="httpMethod">The request HTTP method.</param>
    /// <returns>A new custom HTTP metric.</returns>
    IFirebasePerformanceHttpMetric NewHttpMetric(
        Uri url,
        HttpMethod httpMethod
    );
}
