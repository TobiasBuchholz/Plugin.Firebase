using Firebase.Perf.Metrics;

namespace Plugin.Firebase.PerformanceMonitoring;

public sealed class FirebasePerformanceHttpMetricWrapper : IFirebasePerformanceHttpMetric
{
    private readonly HttpMetric _httpMetric;

    public FirebasePerformanceHttpMetricWrapper(HttpMetric httpMetric)
    {
        _httpMetric = httpMetric;
    }

    public IReadOnlyDictionary<string, string> Attributes {
        get {
            var attributes = _httpMetric.Attributes;
            return attributes == null
                ? new Dictionary<string, string>()
                : new Dictionary<string, string>(attributes);
        }
    }

    public void Start()
    {
        _httpMetric.Start();
    }

    public void Stop()
    {
        _httpMetric.Stop();
    }

    public void SetHttpResponseCode(int responseCode)
    {
        _httpMetric.SetHttpResponseCode(responseCode);
    }

    public void SetRequestPayloadSize(long bytes)
    {
        _httpMetric.SetRequestPayloadSize(bytes);
    }

    public void SetResponsePayloadSize(long bytes)
    {
        _httpMetric.SetResponsePayloadSize(bytes);
    }

    public void SetResponseContentType(string contentType)
    {
        _httpMetric.SetResponseContentType(contentType);
    }

    public void PutAttribute(string attribute, string value)
    {
        _httpMetric.PutAttribute(attribute, value);
    }

    public string? GetAttribute(string attribute)
    {
        return _httpMetric.GetAttribute(attribute);
    }

    public void RemoveAttribute(string attribute)
    {
        _httpMetric.RemoveAttribute(attribute);
    }
}
