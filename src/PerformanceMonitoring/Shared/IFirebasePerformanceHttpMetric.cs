namespace Plugin.Firebase.PerformanceMonitoring;

/// <summary>
/// A Firebase Performance Monitoring custom HTTP network request metric.
/// </summary>
public interface IFirebasePerformanceHttpMetric
{
    /// <summary>
    /// Gets the HTTP metric attributes.
    /// </summary>
    IReadOnlyDictionary<string, string> Attributes { get; }

    /// <summary>
    /// Starts the HTTP metric.
    /// </summary>
    void Start();

    /// <summary>
    /// Stops the HTTP metric.
    /// </summary>
    void Stop();

    /// <summary>
    /// Sets the HTTP response code.
    /// </summary>
    /// <param name="responseCode">The HTTP response code.</param>
    void SetHttpResponseCode(int responseCode);

    /// <summary>
    /// Sets the request payload size in bytes.
    /// </summary>
    /// <param name="bytes">The request payload size in bytes.</param>
    void SetRequestPayloadSize(long bytes);

    /// <summary>
    /// Sets the response payload size in bytes.
    /// </summary>
    /// <param name="bytes">The response payload size in bytes.</param>
    void SetResponsePayloadSize(long bytes);

    /// <summary>
    /// Sets the response content type.
    /// </summary>
    /// <param name="contentType">The response content type.</param>
    void SetResponseContentType(string contentType);

    /// <summary>
    /// Sets an HTTP metric attribute value.
    /// </summary>
    /// <param name="attribute">The attribute name.</param>
    /// <param name="value">The attribute value.</param>
    void PutAttribute(string attribute, string value);

    /// <summary>
    /// Gets an HTTP metric attribute value.
    /// </summary>
    /// <param name="attribute">The attribute name.</param>
    /// <returns>The attribute value.</returns>
    string GetAttribute(string attribute);

    /// <summary>
    /// Removes an HTTP metric attribute.
    /// </summary>
    /// <param name="attribute">The attribute name.</param>
    void RemoveAttribute(string attribute);
}
