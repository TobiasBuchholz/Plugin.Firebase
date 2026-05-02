using Plugin.Firebase.Analytics;

namespace Plugin.Firebase.IntegrationTests.Analytics
{
    [Collection("Sequential")]
    [TestLogging]
    [Preserve(AllMembers = true)]
    public sealed class AnalyticsFixture
    {
#if ANDROID
        [Fact]
        public void throws_actionable_exception_when_android_analytics_is_not_initialized()
        {
            var firebaseAnalyticsField = typeof(FirebaseAnalyticsImplementation).GetField(
                "_firebaseAnalytics",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static
            );
            Assert.NotNull(firebaseAnalyticsField);

            var originalFirebaseAnalytics = firebaseAnalyticsField.GetValue(null);
            try {
                firebaseAnalyticsField.SetValue(null, null);

                var logEventException = Assert.Throws<InvalidOperationException>(
                    () => CrossFirebaseAnalytics.Current.LogEvent("test_uninitialized_analytics_guard")
                );
                AssertAnalyticsNotInitializedException(logEventException);

                var setDefaultEventParametersException = Assert.Throws<InvalidOperationException>(
                    () => CrossFirebaseAnalytics.Current.SetDefaultEventParameters(new Dictionary<string, object> {
                        { "default_string", "some_value" }
                    })
                );
                AssertAnalyticsNotInitializedException(setDefaultEventParametersException);
            }
            finally {
                firebaseAnalyticsField.SetValue(null, originalFirebaseAnalytics);
            }
        }

        private static void AssertAnalyticsNotInitializedException(InvalidOperationException exception)
        {
            Assert.Contains("Firebase Analytics has not been initialized on Android", exception.Message);
            Assert.Contains("FirebaseAnalyticsImplementation.Initialize(activity)", exception.Message);
            Assert.Contains("isAnalyticsEnabled: true", exception.Message);
        }
#endif

        [RealFirebaseFact]
        public void does_not_throw_any_exception_when_logging_events()
        {
            var sut = CrossFirebaseAnalytics.Current;

            sut.LogEvent("test_without_parameters");
            sut.LogEvent("test_with_single_tuple_parameter", ("some_parameter", "some_value"));

            sut.LogEvent("test_with_multiple_tuple_parameters",
                ("some_string", "some_value"),
                ("some_int", 1337),
                ("some_long", 1337L),
                ("some_double", 13.37),
                ("some_float", 133.7f),
                ("some_bool", true),
                ("some_dictionary", new Dictionary<string, object> { { "some_string", "some_value" } }),
                ("some_dictionary_collection", new[] { new Dictionary<string, object> { { "dict_string", "dict_value" } } })
            );

            sut.LogEvent("test_with_dictionary", new Dictionary<string, object> {
                { "some_string", "some_value" },
                { "some_int", 1337 },
                { "some_long", 1337L },
                { "some_double", 13.37 },
                { "some_float", 133.7f },
                { "some_bool", true },
                { "some_dictionary", new Dictionary<string, object> { { "some_key", "some_value" } } },
                { "some_dictionary_collection", new [] { new Dictionary<string, object> { { "some_key", "some_value" } } }}
            });
        }

        [RealFirebaseFact]
        public void does_not_throw_any_exception_when_setting_default_event_parameters_via_dictionary()
        {
            var sut = CrossFirebaseAnalytics.Current;

            try {
                sut.SetDefaultEventParameters(new Dictionary<string, object> {
                    { "default_string", "some_value" },
                    { "default_long", 1337L },
                    { "default_double", 13.37 }
                });

                sut.LogEvent("test_with_default_dictionary_parameters");
            }
            finally {
                sut.SetDefaultEventParameters((IDictionary<string, object>) null);
            }
        }

        [RealFirebaseFact]
        public void does_not_throw_any_exception_when_setting_default_event_parameters_via_tuples()
        {
            var sut = CrossFirebaseAnalytics.Current;

            try {
                sut.SetDefaultEventParameters(
                    ("default_string", "some_value"),
                    ("default_long", 1337L),
                    ("default_double", 13.37));

                sut.LogEvent("test_with_default_tuple_parameters");
            }
            finally {
                sut.SetDefaultEventParameters((IDictionary<string, object>) null);
            }
        }

        [RealFirebaseFact]
        public void does_not_throw_any_exception_when_clearing_default_event_parameters()
        {
            CrossFirebaseAnalytics.Current.SetDefaultEventParameters((IDictionary<string, object>) null);
        }

        [RealFirebaseFact]
        public void does_not_throw_any_exception_when_setting_user_properties()
        {
            var sut = CrossFirebaseAnalytics.Current;
            sut.SetUserId("some_id");
            sut.SetUserProperty("some_name", "some_value");
        }

        [RealFirebaseFact]
        public async Task does_not_throw_any_exception_when_getting_app_instance_id()
        {
            var sut = CrossFirebaseAnalytics.Current;
            Assert.NotNull(await sut.GetAppInstanceIdAsync());
        }

        [RealFirebaseFact]
        public void does_not_throw_any_exception_at_other_methods()
        {
            var sut = CrossFirebaseAnalytics.Current;
            sut.IsAnalyticsCollectionEnabled = true;
            sut.SetSessionTimoutDuration(TimeSpan.FromSeconds(90));
            sut.ResetAnalyticsData();
        }
    }
}