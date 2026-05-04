using Plugin.Firebase.Analytics;

namespace Plugin.Firebase.IntegrationTests.Analytics;

[Collection("Sequential")]
[TestLogging]
[IntegrationTestFixture(IntegrationTestPackage.Analytics)]
[Preserve(AllMembers = true)]
public sealed class AnalyticsAndroidFixture
{
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
}