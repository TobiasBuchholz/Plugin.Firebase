using Firebase.Perf.Metrics;

namespace Plugin.Firebase.PerformanceMonitoring;

public sealed class FirebasePerformanceTraceWrapper : IFirebasePerformanceTrace
{
    private readonly Trace _trace;

    public FirebasePerformanceTraceWrapper(Trace trace)
    {
        _trace = trace;
    }

    public string Name => _trace.Name;

    public IReadOnlyDictionary<string, string> Attributes {
        get {
            var attributes = _trace.Attributes;
            return attributes == null
                ? new Dictionary<string, string>()
                : new Dictionary<string, string>(attributes);
        }
    }

    public void Start()
    {
        _trace.Start();
    }

    public void Stop()
    {
        _trace.Stop();
    }

    public void IncrementMetric(string metricName, long incrementBy)
    {
        _trace.IncrementMetric(metricName, incrementBy);
    }

    public void PutMetric(string metricName, long value)
    {
        _trace.PutMetric(metricName, value);
    }

    public long GetLongMetric(string metricName)
    {
        return _trace.GetLongMetric(metricName);
    }

    public void PutAttribute(string attribute, string value)
    {
        _trace.PutAttribute(attribute, value);
    }

    public string? GetAttribute(string attribute)
    {
        return _trace.GetAttribute(attribute);
    }

    public void RemoveAttribute(string attribute)
    {
        _trace.RemoveAttribute(attribute);
    }
}
