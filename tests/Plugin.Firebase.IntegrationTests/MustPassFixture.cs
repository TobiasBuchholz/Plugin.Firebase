using Plugin.Firebase.Analytics;
using Plugin.Firebase.AppCheck;
using Plugin.Firebase.Auth;
using Plugin.Firebase.CloudMessaging;
using Plugin.Firebase.Firestore;
using Plugin.Firebase.Functions;
using Plugin.Firebase.Installations;
using Plugin.Firebase.PerformanceMonitoring;
using Plugin.Firebase.RemoteConfig;
using Plugin.Firebase.Storage;

namespace Plugin.Firebase.IntegrationTests;

[Collection("Sequential")]
[TestLogging]
[IntegrationTestCoverageIgnore("Harness smoke test; package acceptance is tracked by package fixtures.")]
[Preserve(AllMembers = true)]
public class MustPassFixture
{
    [Fact]
    public void must_pass()
    {
        Assert.True(true);
        Assert.True(CrossFirebaseAnalytics.IsSupported);
        Assert.True(CrossFirebaseAuth.IsSupported);
        Assert.True(CrossFirebaseCloudMessaging.IsSupported);
        Assert.True(CrossFirebaseFirestore.IsSupported);
        Assert.True(CrossFirebaseFunctions.IsSupported);
        Assert.True(CrossFirebaseInstallations.IsSupported);
        Assert.True(CrossFirebasePerformanceMonitoring.IsSupported);
        Assert.True(CrossFirebaseStorage.IsSupported);
        Assert.True(CrossFirebaseRemoteConfig.IsSupported);
        Assert.True(CrossFirebaseAppCheck.IsSupported);

        if(OperatingSystem.IsAndroid()) {
            Assert.Throws<NotSupportedException>(() => CrossFirebaseAppCheck.Configure(AppCheckOptions.DeviceCheck));
            Assert.Throws<NotSupportedException>(() => CrossFirebaseAppCheck.Configure(AppCheckOptions.AppAttest));
            // Android cannot remove a provider factory, so finish on Debug for the rest of the run.
            CrossFirebaseAppCheck.Configure(AppCheckOptions.PlayIntegrity);
            CrossFirebaseAppCheck.Configure(AppCheckOptions.Debug);
        }

        if(OperatingSystem.IsIOS()) {
            CrossFirebaseAppCheck.Configure(AppCheckOptions.Disabled);
            CrossFirebaseAppCheck.Configure(AppCheckOptions.PlayIntegrity);
            CrossFirebaseAppCheck.Configure(AppCheckOptions.Debug);
            CrossFirebaseAppCheck.Configure(AppCheckOptions.DeviceCheck);
            CrossFirebaseAppCheck.Configure(AppCheckOptions.AppAttest);
        }
    }

    // CI passes TZ to the iOS app (SIMCTL_CHILD_TZ) so date tests run outside UTC. When TZ is set, check that .NET's
    // local time zone follows it, so a run that silently stayed in UTC fails here. Without TZ the test only logs the
    // time zone; the Android emulator's zone comes from -timezone instead, and the startup log shows it.
    [Fact]
    public void uses_the_time_zone_named_by_tz_when_it_is_set()
    {
        var tz = Environment.GetEnvironmentVariable("TZ");
        var now = DateTimeOffset.UtcNow;
        TestLog.Write($"[TIME ZONE] TZ={tz ?? "(unset)"}; local={TimeZoneInfo.Local.Id}; utcOffset={TimeZoneInfo.Local.GetUtcOffset(now)}");
        if(string.IsNullOrEmpty(tz)) {
            return;
        }

        Assert.True(
            TimeZoneInfo.TryFindSystemTimeZoneById(tz, out var expected),
            $"TZ={tz} doesn't name a time zone this runtime knows, so local time falls back to UTC.");
        Assert.Equal(expected.GetUtcOffset(now), TimeZoneInfo.Local.GetUtcOffset(now));
    }

    [RealFirebaseOptInFact(IntegrationTestEnvironment.RunAppCheckTokenTestsEnvironmentVariableName)]
    public async Task fetches_app_check_token_when_enabled_via_environment()
    {
        CrossFirebaseAppCheck.Configure(AppCheckOptions.Debug);
        var token = await CrossFirebaseAppCheck.GetTokenAsync(forceRefresh: true);

        Assert.False(string.IsNullOrWhiteSpace(token));
    }
}