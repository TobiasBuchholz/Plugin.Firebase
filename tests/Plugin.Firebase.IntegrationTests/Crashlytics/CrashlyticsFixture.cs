using Plugin.Firebase.Crashlytics;

namespace Plugin.Firebase.IntegrationTests.Crashlytics
{
    [Collection("Sequential")]
    [TestLogging]
    [IntegrationTestFixture(IntegrationTestPackage.Crashlytics)]
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
            sut.SetUserId(IntegrationTestData.UniqueId("integration-test"));
            sut.Log("Crashlytics integration smoke test");
            sut.SetCrashlyticsCollectionEnabled(true);
        }

        [NonIosSimulatorFact]
        public async Task records_exception_and_queries_unsent_reports()
        {
            var sut = CrossFirebaseCrashlytics.Current;

            sut.SetCrashlyticsCollectionEnabled(false);
            sut.RecordException(new InvalidOperationException("Crashlytics integration smoke test"));
            var hasUnsentReports = await sut.CheckForUnsentReportsAsync().WaitForTestAsync(
                IntegrationTestTimeouts.Callback,
                "Crashlytics unsent report check");
            Assert.IsType<bool>(hasUnsentReports);
            sut.SetCrashlyticsCollectionEnabled(true);
        }

        [Fact]
        public void exposes_previous_crash_state()
        {
            var didCrash = CrossFirebaseCrashlytics.Current.DidCrashOnPreviousExecution();
            Assert.IsType<bool>(didCrash);
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

        [OptInFact(IntegrationTestOptions.ExpectPreviousCrashEnvironmentVariableName)]
        public void detects_previous_forced_crash_when_enabled()
        {
            Assert.True(CrossFirebaseCrashlytics.Current.DidCrashOnPreviousExecution());
        }

        [OptInFact(IntegrationTestOptions.ForceCrashlyticsCrashEnvironmentVariableName)]
        public void forces_process_crash_when_enabled()
        {
            var sut = CrossFirebaseCrashlytics.Current;
            sut.SetCrashlyticsCollectionEnabled(true);
            sut.Log("Forcing Crashlytics acceptance-test crash.");

            Environment.FailFast("Forced Crashlytics acceptance-test crash.");
        }
    }
}