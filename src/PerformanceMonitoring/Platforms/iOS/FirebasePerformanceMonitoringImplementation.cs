using Plugin.Firebase.Core;
using Plugin.Firebase.PerformanceMonitoring.Platforms.iOS.Extensions;
using NativeHttpMetric = Firebase.PerformanceMonitoring.HttpMetric;
using NativePerformance = Firebase.PerformanceMonitoring.Performance;

namespace Plugin.Firebase.PerformanceMonitoring;

/// <inheritdoc/>
public sealed class FirebasePerformanceMonitoringImplementation : DisposableBase, IFirebasePerformanceMonitoring
{
    private readonly NativePerformance _instance;

    /// <summary>
    /// Initializes a new instance of the <see cref="FirebasePerformanceMonitoringImplementation"/> class.
    /// </summary>
    public FirebasePerformanceMonitoringImplementation()
    {
        _instance = NativePerformance.SharedInstance;
    }

    /// <inheritdoc/>
    public bool IsDataCollectionEnabled {
        get => _instance.DataCollectionEnabled;
        set => _instance.DataCollectionEnabled = value;
    }

    /// <inheritdoc/>
    public IFirebasePerformanceTrace NewTrace(string traceName)
    {
        return new FirebasePerformanceTraceWrapper(_instance.GetTrace(traceName)!);
    }

    /// <inheritdoc/>
    public IFirebasePerformanceTrace StartTrace(string traceName)
    {
        return new FirebasePerformanceTraceWrapper(NativePerformance.StartTrace(traceName)!);
    }

    /// <inheritdoc/>
    public IFirebasePerformanceHttpMetric NewHttpMetric(
        string url,
        PerformanceMonitoringHttpMethod httpMethod
    )
    {
        return new FirebasePerformanceHttpMetricWrapper(
            new NativeHttpMetric(url, httpMethod.ToNative())
        );
    }

    /// <inheritdoc/>
    public IFirebasePerformanceHttpMetric NewHttpMetric(
        Uri url,
        PerformanceMonitoringHttpMethod httpMethod
    )
    {
        ArgumentNullException.ThrowIfNull(url);
        return NewHttpMetric(url.AbsoluteUri, httpMethod);
    }
}
