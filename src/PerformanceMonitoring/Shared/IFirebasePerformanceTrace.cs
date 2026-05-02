namespace Plugin.Firebase.PerformanceMonitoring;

/// <summary>
/// A Firebase Performance Monitoring custom code trace.
/// </summary>
public interface IFirebasePerformanceTrace
{
    /// <summary>
    /// Gets the trace name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the trace attributes.
    /// </summary>
    IReadOnlyDictionary<string, string> Attributes { get; }

    /// <summary>
    /// Starts the trace.
    /// </summary>
    void Start();

    /// <summary>
    /// Stops the trace.
    /// </summary>
    void Stop();

    /// <summary>
    /// Increments a trace metric.
    /// </summary>
    /// <param name="metricName">The metric name.</param>
    /// <param name="incrementBy">The increment amount.</param>
    void IncrementMetric(string metricName, long incrementBy);

    /// <summary>
    /// Sets a trace metric value.
    /// </summary>
    /// <param name="metricName">The metric name.</param>
    /// <param name="value">The metric value.</param>
    void PutMetric(string metricName, long value);

    /// <summary>
    /// Gets a trace metric value.
    /// </summary>
    /// <param name="metricName">The metric name.</param>
    /// <returns>The metric value.</returns>
    long GetLongMetric(string metricName);

    /// <summary>
    /// Sets a trace attribute value.
    /// </summary>
    /// <param name="attribute">The attribute name.</param>
    /// <param name="value">The attribute value.</param>
    void PutAttribute(string attribute, string value);

    /// <summary>
    /// Gets a trace attribute value.
    /// </summary>
    /// <param name="attribute">The attribute name.</param>
    /// <returns>The attribute value.</returns>
    string? GetAttribute(string attribute);

    /// <summary>
    /// Removes a trace attribute.
    /// </summary>
    /// <param name="attribute">The attribute name.</param>
    void RemoveAttribute(string attribute);
}
