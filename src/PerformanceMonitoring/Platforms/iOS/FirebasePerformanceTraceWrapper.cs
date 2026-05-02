using Plugin.Firebase.PerformanceMonitoring.Platforms.iOS.Extensions;
using NativeTrace = Firebase.PerformanceMonitoring.Trace;

namespace Plugin.Firebase.PerformanceMonitoring;

/// <inheritdoc/>
public sealed class FirebasePerformanceTraceWrapper : IFirebasePerformanceTrace
{
    private readonly NativeTrace _trace;

    /// <summary>
    /// Initializes a new instance of the <see cref="FirebasePerformanceTraceWrapper"/> class.
    /// </summary>
    /// <param name="trace">The native Firebase trace.</param>
    public FirebasePerformanceTraceWrapper(NativeTrace trace)
    {
        _trace = trace;
    }

    /// <inheritdoc/>
    public string Name => _trace.Name;

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, string> Attributes => _trace.Attributes.ToReadOnlyDictionary();

    /// <inheritdoc/>
    public void Start()
    {
        _trace.Start();
    }

    /// <inheritdoc/>
    public void Stop()
    {
        _trace.Stop();
    }

    /// <inheritdoc/>
    public void IncrementMetric(string metricName, long incrementBy)
    {
        _trace.IncrementMetric(metricName, incrementBy);
    }

    /// <inheritdoc/>
    public void PutMetric(string metricName, long value)
    {
        _trace.SetIntValue(value, metricName);
    }

    /// <inheritdoc/>
    public long GetLongMetric(string metricName)
    {
        return _trace.GetIntValue(metricName);
    }

    /// <inheritdoc/>
    public void PutAttribute(string attribute, string value)
    {
        _trace.SetValue(value, attribute);
    }

    /// <inheritdoc/>
    public string GetAttribute(string attribute)
    {
        return _trace.GetValue(attribute);
    }

    /// <inheritdoc/>
    public void RemoveAttribute(string attribute)
    {
        _trace.RemoveAttribute(attribute);
    }
}
