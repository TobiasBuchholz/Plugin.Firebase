using Plugin.Firebase.PerformanceMonitoring.Platforms.iOS.Extensions;
using NativeHttpMetric = Firebase.PerformanceMonitoring.HttpMetric;

namespace Plugin.Firebase.PerformanceMonitoring;

/// <inheritdoc/>
public sealed class FirebasePerformanceHttpMetricWrapper : IFirebasePerformanceHttpMetric
{
    private readonly NativeHttpMetric _httpMetric;

    /// <summary>
    /// Initializes a new instance of the <see cref="FirebasePerformanceHttpMetricWrapper"/> class.
    /// </summary>
    /// <param name="httpMetric">The native Firebase HTTP metric.</param>
    public FirebasePerformanceHttpMetricWrapper(NativeHttpMetric httpMetric)
    {
        _httpMetric = httpMetric;
    }

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, string> Attributes => _httpMetric.Attributes.ToReadOnlyDictionary();

    /// <inheritdoc/>
    public void Start()
    {
        _httpMetric.Start();
    }

    /// <inheritdoc/>
    public void Stop()
    {
        _httpMetric.Stop();
    }

    /// <inheritdoc/>
    public void SetHttpResponseCode(int responseCode)
    {
        _httpMetric.ResponseCode = new IntPtr(responseCode);
    }

    /// <inheritdoc/>
    public void SetRequestPayloadSize(long bytes)
    {
        _httpMetric.RequestPayloadSize = new IntPtr(bytes);
    }

    /// <inheritdoc/>
    public void SetResponsePayloadSize(long bytes)
    {
        _httpMetric.ResponsePayloadSize = new IntPtr(bytes);
    }

    /// <inheritdoc/>
    public void SetResponseContentType(string contentType)
    {
        _httpMetric.ResponseContentType = contentType;
    }

    /// <inheritdoc/>
    public void PutAttribute(string attribute, string value)
    {
        _httpMetric.SetValue(value, attribute);
    }

    /// <inheritdoc/>
    public string? GetAttribute(string attribute)
    {
        return _httpMetric.GetValue(attribute);
    }

    /// <inheritdoc/>
    public void RemoveAttribute(string attribute)
    {
        _httpMetric.RemoveAttribute(attribute);
    }
}
