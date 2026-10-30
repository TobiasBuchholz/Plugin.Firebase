using Firebase.Perf;
using Plugin.Firebase.Core;
using Plugin.Firebase.PerformanceMonitoring.Platforms.Android.Extensions;
using System.Net.Http;

namespace Plugin.Firebase.PerformanceMonitoring;

public sealed class FirebasePerformanceMonitoringImplementation : DisposableBase, IFirebasePerformanceMonitoring
{
    private const string InstrumentationNotSupportedMessage =
        "Performance Monitoring instrumentation can't be toggled at runtime on Android. Use IsDataCollectionEnabled, "
        + "or the firebase_performance_collection_enabled / firebase_performance_collection_deactivated manifest keys.";

    private readonly FirebasePerformance _instance;

    public FirebasePerformanceMonitoringImplementation()
    {
        _instance = FirebasePerformance.Instance;
    }

    public bool IsDataCollectionEnabled {
        get => _instance.PerformanceCollectionEnabled;
        set => _instance.PerformanceCollectionEnabled = value;
    }

    // The Android SDK instruments at build time and exposes no runtime switch, so there is nothing to map this to.
    public bool IsInstrumentationEnabled {
        get => throw new NotSupportedException(InstrumentationNotSupportedMessage);
        set => throw new NotSupportedException(InstrumentationNotSupportedMessage);
    }

    public IFirebasePerformanceTrace NewTrace(string traceName)
    {
        return new FirebasePerformanceTraceWrapper(_instance.NewTrace(traceName));
    }

    public IFirebasePerformanceTrace StartTrace(string traceName)
    {
        return new FirebasePerformanceTraceWrapper(FirebasePerformance.StartTrace(traceName));
    }

    public IFirebasePerformanceHttpMetric NewHttpMetric(
        string url,
        HttpMethod httpMethod
    )
    {
        return new FirebasePerformanceHttpMetricWrapper(
            _instance.NewHttpMetric(url, httpMethod.ToNative())
        );
    }

    public IFirebasePerformanceHttpMetric NewHttpMetric(
        Uri url,
        HttpMethod httpMethod
    )
    {
        ArgumentNullException.ThrowIfNull(url);
        return NewHttpMetric(url.AbsoluteUri, httpMethod);
    }
}
