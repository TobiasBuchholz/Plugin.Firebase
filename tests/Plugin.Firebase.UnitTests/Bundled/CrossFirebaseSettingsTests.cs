using Plugin.Firebase.AppCheck;
using Plugin.Firebase.Bundled.Shared;

namespace Plugin.Firebase.UnitTests.Bundled;

public class CrossFirebaseSettingsTests
{
    [Fact]
    public void preserves_legacy_constructor_signature()
    {
        var constructor = typeof(CrossFirebaseSettings).GetConstructor([
            typeof(bool),
            typeof(bool),
            typeof(bool),
            typeof(bool),
            typeof(bool),
            typeof(bool),
            typeof(bool),
            typeof(bool),
            typeof(bool),
            typeof(string),
            typeof(AppCheckOptions)
        ]);

        Assert.NotNull(constructor);
    }

    [Fact]
    public void enables_installations_via_initializer()
    {
        var settings = new CrossFirebaseSettings {
            IsInstallationsEnabled = true
        };

        Assert.True(settings.IsInstallationsEnabled);
    }

    [Fact]
    public void enables_performance_monitoring_via_initializer()
    {
        var settings = new CrossFirebaseSettings {
            IsPerformanceMonitoringEnabled = true
        };

        Assert.True(settings.IsPerformanceMonitoringEnabled);
    }

    [Fact]
    public void to_string_includes_performance_monitoring_flag()
    {
        var settings = new CrossFirebaseSettings {
            IsPerformanceMonitoringEnabled = true
        };

        Assert.Contains("IsPerformanceMonitoringEnabled=True", settings.ToString());
    }
}
