using Plugin.Firebase.PerformanceMonitoring;

namespace Plugin.Firebase.IntegrationTests.PerformanceMonitoring;

internal static class PerformanceAssertions
{
    public const string AttributeName = "source";
    public const string AttributeValue = "integration_test";
    public const string MetricName = "items";

    public static void DataCollectionEnabled(IFirebasePerformanceMonitoring monitoring, bool expectedValue)
    {
        Assert.Equal(expectedValue, monitoring.IsDataCollectionEnabled);
    }

    public static void TraceName(IFirebasePerformanceTrace trace, string expectedName)
    {
        Assert.Equal(expectedName, trace.Name);
    }

    public static void TraceAttribute(IFirebasePerformanceTrace trace, string attributeValue)
    {
        Assert.Equal(attributeValue, trace.GetAttribute(AttributeName));
        Assert.True(trace.Attributes.ContainsKey(AttributeName));
        Assert.Equal(attributeValue, trace.Attributes[AttributeName]);
    }

    public static void TraceMetric(IFirebasePerformanceTrace trace, long expectedValue)
    {
        Assert.Equal(expectedValue, trace.GetLongMetric(MetricName));
    }

    public static void TraceAttributeRemoved(IFirebasePerformanceTrace trace)
    {
        Assert.Null(trace.GetAttribute(AttributeName));
        Assert.False(trace.Attributes.ContainsKey(AttributeName));
    }

    public static void HttpMetricContract(IFirebasePerformanceHttpMetric metric)
    {
        var started = false;

        try {
            metric.PutAttribute(AttributeName, AttributeValue);
            HttpMetricAttribute(metric);

            metric.Start();
            started = true;

            metric.SetHttpResponseCode(200);
            metric.SetRequestPayloadSize(10);
            metric.SetResponsePayloadSize(20);
            metric.SetResponseContentType("text/plain");

            metric.RemoveAttribute(AttributeName);
            HttpMetricAttributeRemoved(metric);
        }
        finally {
            if(started) {
                metric.Stop();
            }
        }
    }

    public static void NullHttpMetricUriThrows(IFirebasePerformanceMonitoring monitoring)
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => monitoring.NewHttpMetric(
                ((Uri) null!)!,
                HttpMethod.Get
            )
        );
        Assert.Equal("url", exception.ParamName);
    }

    public static void UnsupportedStringHttpMetricMethodThrows(IFirebasePerformanceMonitoring monitoring)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => monitoring.NewHttpMetric(
                "https://example.com/performance-monitoring/unsupported-string-method",
                new HttpMethod("BREW")
            )
        );
        Assert.Equal("httpMethod", exception.ParamName);
    }

    public static void UnsupportedUriHttpMetricMethodThrows(IFirebasePerformanceMonitoring monitoring)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => monitoring.NewHttpMetric(
                new Uri("https://example.com/performance-monitoring/unsupported-uri-method"),
                new HttpMethod("BREW")
            )
        );
        Assert.Equal("httpMethod", exception.ParamName);
    }

    private static void HttpMetricAttribute(IFirebasePerformanceHttpMetric metric)
    {
        Assert.Equal(AttributeValue, metric.GetAttribute(AttributeName));
        Assert.True(metric.Attributes.ContainsKey(AttributeName));
        Assert.Equal(AttributeValue, metric.Attributes[AttributeName]);
    }

    private static void HttpMetricAttributeRemoved(IFirebasePerformanceHttpMetric metric)
    {
        Assert.Null(metric.GetAttribute(AttributeName));
        Assert.False(metric.Attributes.ContainsKey(AttributeName));
    }
}