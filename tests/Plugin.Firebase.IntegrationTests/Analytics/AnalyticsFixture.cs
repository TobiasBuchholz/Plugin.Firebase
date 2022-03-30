using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Runtime;
using Plugin.Firebase.Analytics;
using Xunit;

namespace Plugin.Firebase.IntegrationTests.Analytics
{
    [Preserve(AllMembers = true)]
    public sealed class AnalyticsFixture
    {
        [Fact]
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
                ("some_bool", true));

            sut.LogEvent("test_with_dictionary", new Dictionary<string, object> {
                { "some_string", "some_value" },
                { "some_int", 1337 },
                { "some_long", 1337L },
                { "some_double", 13.37 },
                { "some_float", 133.7f },
                { "some_bool", true }
            });
        }

        [Fact]
        public void does_not_throw_any_exception_when_setting_user_properties()
        {
            var sut = CrossFirebaseAnalytics.Current;
            sut.SetUserId("some_id");
            sut.SetUserProperty("some_name", "some_value");
        }

        [Fact]
        public async Task does_not_throw_any_exception_when_getting_app_instance_id()
        {
            var sut = CrossFirebaseAnalytics.Current;
            Assert.NotNull(await sut.GetAppInstanceIdAsync());
        }

        [Fact]
        public void does_not_throw_any_exception_at_other_methods()
        {
            var sut = CrossFirebaseAnalytics.Current;
            sut.IsAnalyticsCollectionEnabled = true;
            sut.SetSessionTimoutDuration(TimeSpan.FromSeconds(90));
            sut.ResetAnalyticsData();
        }
    }
}