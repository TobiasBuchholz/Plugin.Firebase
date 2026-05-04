using Plugin.Firebase.PerformanceMonitoring;

namespace Plugin.Firebase.IntegrationTests.PerformanceMonitoring;

[Collection("Sequential")]
[TestLogging]
[IntegrationTestFixture(IntegrationTestPackage.PerformanceMonitoring)]
[Preserve(AllMembers = true)]
public sealed class PerformanceMonitoringFixture
{
    public static TheoryData<HttpMethod> HttpMethods => [
        HttpMethod.Get,
        HttpMethod.Put,
        HttpMethod.Post,
        HttpMethod.Delete,
        HttpMethod.Head,
        HttpMethod.Patch,
        HttpMethod.Options,
        HttpMethod.Trace,
        HttpMethod.Connect
    ];

    [Fact]
    public void round_trips_collection_enabled_state()
    {
        var sut = CrossFirebasePerformanceMonitoring.Current;
        var originalValue = sut.IsDataCollectionEnabled;

        try {
            sut.IsDataCollectionEnabled = false;
            PerformanceAssertions.DataCollectionEnabled(sut, false);

            sut.IsDataCollectionEnabled = true;
            PerformanceAssertions.DataCollectionEnabled(sut, true);
        }
        finally {
            sut.IsDataCollectionEnabled = originalValue;
            PerformanceAssertions.DataCollectionEnabled(sut, originalValue);
        }
    }

    [Fact]
    public void records_custom_trace_attributes_and_metrics()
    {
        var trace = CrossFirebasePerformanceMonitoring.Current.NewTrace("test_custom_trace_contract");
        var started = false;

        try {
            PerformanceAssertions.TraceName(trace, "test_custom_trace_contract");

            trace.PutAttribute(PerformanceAssertions.AttributeName, PerformanceAssertions.AttributeValue);
            PerformanceAssertions.TraceAttribute(trace, PerformanceAssertions.AttributeValue);

            trace.Start();
            started = true;

            trace.PutMetric(PerformanceAssertions.MetricName, 1);
            trace.IncrementMetric(PerformanceAssertions.MetricName, 1);
            PerformanceAssertions.TraceMetric(trace, 2);

            trace.RemoveAttribute(PerformanceAssertions.AttributeName);
            PerformanceAssertions.TraceAttributeRemoved(trace);
        }
        finally {
            if(started) {
                trace.Stop();
            }
        }
    }

    [Fact]
    public void records_started_custom_trace_metrics()
    {
        var trace = CrossFirebasePerformanceMonitoring.Current.StartTrace("test_started_trace_contract");

        try {
            PerformanceAssertions.TraceName(trace, "test_started_trace_contract");

            trace.PutMetric(PerformanceAssertions.MetricName, 3);
            trace.IncrementMetric(PerformanceAssertions.MetricName, 4);
            PerformanceAssertions.TraceMetric(trace, 7);
        }
        finally {
            trace.Stop();
        }
    }

    [Theory]
    [MemberData(nameof(HttpMethods))]
    public void records_string_http_metric_for_each_method(HttpMethod method)
    {
        var metric = CrossFirebasePerformanceMonitoring.Current.NewHttpMetric(
            GetHttpMetricUrl("string", method),
            method
        );

        PerformanceAssertions.HttpMetricContract(metric);
    }

    [Theory]
    [MemberData(nameof(HttpMethods))]
    public void records_uri_http_metric_for_each_method(HttpMethod method)
    {
        var metric = CrossFirebasePerformanceMonitoring.Current.NewHttpMetric(
            new Uri(GetHttpMetricUrl("uri", method)),
            method
        );

        PerformanceAssertions.HttpMetricContract(metric);
    }

    [Fact]
    public void throws_for_null_http_metric_uri()
    {
        PerformanceAssertions.NullHttpMetricUriThrows(CrossFirebasePerformanceMonitoring.Current);
    }

    [Fact]
    public void throws_for_unsupported_string_http_metric_method()
    {
        PerformanceAssertions.UnsupportedStringHttpMetricMethodThrows(CrossFirebasePerformanceMonitoring.Current);
    }

    [Fact]
    public void throws_for_unsupported_uri_http_metric_method()
    {
        PerformanceAssertions.UnsupportedUriHttpMetricMethodThrows(CrossFirebasePerformanceMonitoring.Current);
    }

    [RealFirebaseFact]
    public void real_backend_accepts_custom_trace()
    {
        var trace = CrossFirebasePerformanceMonitoring.Current.NewTrace("test_real_backend_trace");
        var started = false;

        try {
            trace.PutAttribute(PerformanceAssertions.AttributeName, "real_backend");
            trace.Start();
            started = true;
            trace.PutMetric(PerformanceAssertions.MetricName, 1);
            trace.IncrementMetric(PerformanceAssertions.MetricName, 1);
            PerformanceAssertions.TraceMetric(trace, 2);
        }
        finally {
            if(started) {
                trace.Stop();
            }
        }
    }

    [RealFirebaseFact]
    public void real_backend_accepts_custom_http_metric()
    {
        var metric = CrossFirebasePerformanceMonitoring.Current.NewHttpMetric(
            "https://example.com/performance-monitoring/real-backend",
            HttpMethod.Get
        );

        PerformanceAssertions.HttpMetricContract(metric);
    }

    private static string GetHttpMetricUrl(string overloadName, HttpMethod method)
    {
        return $"https://example.com/performance-monitoring/{overloadName}/{method.Method.ToLowerInvariant()}";
    }
}