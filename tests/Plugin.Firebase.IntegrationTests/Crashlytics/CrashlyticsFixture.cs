using Plugin.Firebase.Crashlytics;

namespace Plugin.Firebase.IntegrationTests.Crashlytics
{
    [Collection("Sequential")]
    [TestLogging]
    [Preserve(AllMembers = true)]
    public sealed class CrashlyticsFixture
    {
        [Fact]
        public void configures_collection_and_custom_keys()
        {
            var sut = CrossFirebaseCrashlytics.Current;

            sut.SetCrashlyticsCollectionEnabled(false);
            sut.SetCustomKey("test_bool", true);
            sut.SetCustomKey("test_int", 1);
            sut.SetCustomKey("test_long", 2L);
            sut.SetCustomKey("test_float", 3.5f);
            sut.SetCustomKey("test_double", 4.5d);
            sut.SetCustomKey("test_string", "value");
            sut.SetCustomKeys(new Dictionary<string, object> {
                { "bulk_bool", false },
                { "bulk_int", 7 },
                { "bulk_string", "bulk-value" }
            });
            sut.SetUserId($"integration-test-{Guid.NewGuid():N}");
            sut.Log("Crashlytics integration smoke test");
            sut.SetCrashlyticsCollectionEnabled(true);
        }

        [Fact]
        public async Task records_exception_and_queries_unsent_reports()
        {
            if(OperatingSystem.IsIOS() && DeviceInfo.DeviceType == DeviceType.Virtual) {
                return;
            }

            var sut = CrossFirebaseCrashlytics.Current;

            sut.SetCrashlyticsCollectionEnabled(false);
            sut.RecordException(new InvalidOperationException("Crashlytics integration smoke test"));
            await sut.CheckForUnsentReportsAsync().WaitAsync(TimeSpan.FromSeconds(10));
            sut.SetCrashlyticsCollectionEnabled(true);
        }

        [Fact]
        public void handles_unsent_report_controls()
        {
            var sut = CrossFirebaseCrashlytics.Current;

            sut.SetCrashlyticsCollectionEnabled(false);
            sut.SendUnsentReports();
            sut.DeleteUnsentReports();
            sut.SetCrashlyticsCollectionEnabled(true);
        }
    }
}
