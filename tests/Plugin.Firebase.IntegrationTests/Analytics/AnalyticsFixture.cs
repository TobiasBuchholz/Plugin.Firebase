using Plugin.Firebase.Analytics;

namespace Plugin.Firebase.IntegrationTests.Analytics
{
    [Collection("Sequential")]
    [TestLogging]
    [IntegrationTestFixture(IntegrationTestPackage.Analytics)]
    [Preserve(AllMembers = true)]
    public sealed class AnalyticsFixture
    {
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
                sut.SetDefaultEventParameters((IDictionary<string, object>?) null);
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
                sut.SetDefaultEventParameters((IDictionary<string, object>?) null);
            }
        }

        [RealFirebaseFact]
        public void does_not_throw_any_exception_when_clearing_default_event_parameters()
        {
            CrossFirebaseAnalytics.Current.SetDefaultEventParameters((IDictionary<string, object>?) null);
        }

        [RealFirebaseFact]
        public void does_not_throw_any_exception_when_setting_user_properties()
        {
            var sut = CrossFirebaseAnalytics.Current;
            sut.SetUserId("some_id");
            sut.SetUserProperty("some_name", "some_value");
        }

        [RealFirebaseFact]
        public void does_not_throw_any_exception_when_clearing_user_properties()
        {
            var sut = CrossFirebaseAnalytics.Current;
            sut.SetUserId(null);
            sut.SetUserProperty("some_name", null);
        }

        [RealFirebaseFact]
        public async Task does_not_throw_any_exception_when_getting_app_instance_id()
        {
            var sut = CrossFirebaseAnalytics.Current;
            Assert.NotNull(await sut.GetAppInstanceIdAsync());
        }

        [RealFirebaseFact]
        public async Task reset_analytics_data_keeps_instance_id_api_usable()
        {
            var sut = CrossFirebaseAnalytics.Current;

            sut.ResetAnalyticsData();

            Assert.NotNull(await sut.GetAppInstanceIdAsync());
        }

        [RealFirebaseFact]
        public void accepts_events_while_collection_is_disabled()
        {
            var sut = CrossFirebaseAnalytics.Current;

            try {
                sut.IsAnalyticsCollectionEnabled = false;
                sut.LogEvent("test_collection_disabled", ("some_parameter", "some_value"));
            }
            finally {
                sut.IsAnalyticsCollectionEnabled = true;
            }
        }

        [RealFirebaseFact]
        public void accepts_boundary_sized_parameters_and_user_properties()
        {
            var sut = CrossFirebaseAnalytics.Current;
            var parameterName = new string('p', 40);
            var userPropertyName = new string('u', 24);
            var userPropertyValue = new string('v', 36);

            sut.LogEvent("test_boundary_parameters", (parameterName, "value"));
            sut.SetUserProperty(userPropertyName, userPropertyValue);
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