using System.Net.Http;
using Plugin.Firebase.PerformanceMonitoring;

namespace Plugin.Firebase.IntegrationTests.PerformanceMonitoring
{
    [Collection("Sequential")]
    [TestLogging]
    [Preserve(AllMembers = true)]
    public sealed class PerformanceMonitoringFixture
    {
        private const string AttributeName = "source";
        private const string AttributeValue = "integration_test";
        private const string MetricName = "items";

        public static TheoryData<HttpMethod> HttpMethods => new()
        {
            HttpMethod.Get,
            HttpMethod.Put,
            HttpMethod.Post,
            HttpMethod.Delete,
            HttpMethod.Head,
            HttpMethod.Patch,
            HttpMethod.Options,
            HttpMethod.Trace,
            HttpMethod.Connect
        };

        [Fact]
        public void round_trips_collection_enabled_state()
        {
            var sut = CrossFirebasePerformanceMonitoring.Current;
            var originalValue = sut.IsDataCollectionEnabled;

            try {
                sut.IsDataCollectionEnabled = false;
                Assert.False(sut.IsDataCollectionEnabled);

                sut.IsDataCollectionEnabled = true;
                Assert.True(sut.IsDataCollectionEnabled);
            }
            finally {
                sut.IsDataCollectionEnabled = originalValue;
                Assert.Equal(originalValue, sut.IsDataCollectionEnabled);
            }
        }

        [Fact]
        public void records_custom_trace_attributes_and_metrics()
        {
            var trace = CrossFirebasePerformanceMonitoring.Current.NewTrace("test_custom_trace_contract");
            var started = false;

            try {
                Assert.Equal("test_custom_trace_contract", trace.Name);

                trace.PutAttribute(AttributeName, AttributeValue);
                Assert.Equal(AttributeValue, trace.GetAttribute(AttributeName));
                Assert.True(trace.Attributes.ContainsKey(AttributeName));
                Assert.Equal(AttributeValue, trace.Attributes[AttributeName]);

                trace.Start();
                started = true;

                trace.PutMetric(MetricName, 1);
                trace.IncrementMetric(MetricName, 1);
                Assert.Equal(2, trace.GetLongMetric(MetricName));

                trace.RemoveAttribute(AttributeName);
                Assert.Null(trace.GetAttribute(AttributeName));
                Assert.False(trace.Attributes.ContainsKey(AttributeName));
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
                Assert.Equal("test_started_trace_contract", trace.Name);

                trace.PutMetric(MetricName, 3);
                trace.IncrementMetric(MetricName, 4);
                Assert.Equal(7, trace.GetLongMetric(MetricName));
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

            AssertHttpMetricContract(metric);
        }

        [Theory]
        [MemberData(nameof(HttpMethods))]
        public void records_uri_http_metric_for_each_method(HttpMethod method)
        {
            var metric = CrossFirebasePerformanceMonitoring.Current.NewHttpMetric(
                new Uri(GetHttpMetricUrl("uri", method)),
                method
            );

            AssertHttpMetricContract(metric);
        }

        [Fact]
        public void throws_for_null_http_metric_uri()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => CrossFirebasePerformanceMonitoring.Current.NewHttpMetric(
                    (Uri) null!,
                    HttpMethod.Get
                )
            );
            Assert.Equal("url", exception.ParamName);
        }

        [Fact]
        public void throws_for_unsupported_string_http_metric_method()
        {
            var exception = Assert.Throws<ArgumentOutOfRangeException>(
                () => CrossFirebasePerformanceMonitoring.Current.NewHttpMetric(
                    "https://example.com/performance-monitoring/unsupported-string-method",
                    new HttpMethod("BREW")
                )
            );
            Assert.Equal("httpMethod", exception.ParamName);
        }

        [Fact]
        public void throws_for_unsupported_uri_http_metric_method()
        {
            var exception = Assert.Throws<ArgumentOutOfRangeException>(
                () => CrossFirebasePerformanceMonitoring.Current.NewHttpMetric(
                    new Uri("https://example.com/performance-monitoring/unsupported-uri-method"),
                    new HttpMethod("BREW")
                )
            );
            Assert.Equal("httpMethod", exception.ParamName);
        }

        [RealFirebaseFact]
        public void real_backend_accepts_custom_trace()
        {
            var trace = CrossFirebasePerformanceMonitoring.Current.NewTrace("test_real_backend_trace");
            var started = false;

            try {
                trace.PutAttribute(AttributeName, "real_backend");
                trace.Start();
                started = true;
                trace.PutMetric(MetricName, 1);
                trace.IncrementMetric(MetricName, 1);
                Assert.Equal(2, trace.GetLongMetric(MetricName));
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

            AssertHttpMetricContract(metric);
        }

        private static void AssertHttpMetricContract(IFirebasePerformanceHttpMetric metric)
        {
            var started = false;

            try {
                metric.PutAttribute(AttributeName, AttributeValue);
                Assert.Equal(AttributeValue, metric.GetAttribute(AttributeName));
                Assert.True(metric.Attributes.ContainsKey(AttributeName));
                Assert.Equal(AttributeValue, metric.Attributes[AttributeName]);

                metric.Start();
                started = true;

                metric.SetHttpResponseCode(200);
                metric.SetRequestPayloadSize(10);
                metric.SetResponsePayloadSize(20);
                metric.SetResponseContentType("text/plain");

                metric.RemoveAttribute(AttributeName);
                Assert.Null(metric.GetAttribute(AttributeName));
                Assert.False(metric.Attributes.ContainsKey(AttributeName));
            }
            finally {
                if(started) {
                    metric.Stop();
                }
            }
        }

        private static string GetHttpMetricUrl(string overloadName, HttpMethod method)
        {
            return $"https://example.com/performance-monitoring/{overloadName}/{method.Method.ToLowerInvariant()}";
        }
    }
}
