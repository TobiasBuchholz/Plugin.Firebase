using Plugin.Firebase.AppCheck;

namespace Plugin.Firebase.UnitTests;

public class AppCheckLateConfigurationRulesTests
{
    [Theory]
    [InlineData(AppCheckProviderType.Disabled)]
    [InlineData(AppCheckProviderType.Debug)]
    [InlineData(AppCheckProviderType.DeviceCheck)]
    [InlineData(AppCheckProviderType.AppAttest)]
    public void configuring_the_installed_provider_does_nothing(AppCheckProviderType provider)
    {
        AppCheckLateConfigurationRules.EnsureProviderUnchanged(provider, provider);
    }

    [Theory]
    [InlineData(AppCheckProviderType.Disabled, AppCheckProviderType.Debug)]
    [InlineData(AppCheckProviderType.Debug, AppCheckProviderType.Disabled)]
    [InlineData(AppCheckProviderType.DeviceCheck, AppCheckProviderType.AppAttest)]
    public void changing_the_installed_provider_throws(AppCheckProviderType installed, AppCheckProviderType requested)
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => AppCheckLateConfigurationRules.EnsureProviderUnchanged(installed, requested));

        Assert.Contains($"from the '{installed}' provider to '{requested}'", exception.Message);
    }

    // The native SDK installs a DeviceCheck factory by default, and the app may set one natively, so when the plugin
    // installed none it can't tell which provider is in effect, not even Disabled or DeviceCheck.
    [Theory]
    [InlineData(AppCheckProviderType.Disabled)]
    [InlineData(AppCheckProviderType.Debug)]
    [InlineData(AppCheckProviderType.DeviceCheck)]
    [InlineData(AppCheckProviderType.AppAttest)]
    public void every_provider_throws_when_the_plugin_installed_none(AppCheckProviderType requested)
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => AppCheckLateConfigurationRules.EnsureProviderUnchanged(null, requested));

        Assert.Contains($"'{requested}'", exception.Message);
        Assert.Contains("didn't install the provider in effect", exception.Message);
    }
}