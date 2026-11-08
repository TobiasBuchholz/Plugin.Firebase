using Plugin.Firebase.AppCheck;

namespace Plugin.Firebase.UnitTests;

public class AppCheckStartupProviderInstallerTests
{
    private readonly List<AppCheckProviderType> _factoriesSet = new();
    private readonly AppCheckStartupProviderInstaller _sut;
    private bool _defaultAppConfigured;
    private bool _setFails;

    public AppCheckStartupProviderInstallerTests()
    {
        _sut = new AppCheckStartupProviderInstaller(
            () => _defaultAppConfigured,
            provider => {
                if(_setFails) {
                    throw new InvalidOperationException("native failure");
                }

                _factoriesSet.Add(provider);
            });
    }

    [Fact]
    public void configure_before_the_default_app_exists_sets_every_provider_right_away()
    {
        _sut.Configure(AppCheckOptions.Debug);
        _sut.Configure(AppCheckOptions.Disabled);
        _sut.Configure(AppCheckOptions.AppAttest);

        Assert.Equal(
            new[] { AppCheckProviderType.Debug, AppCheckProviderType.Disabled, AppCheckProviderType.AppAttest },
            _factoriesSet);
    }

    [Theory]
    [InlineData(AppCheckProviderType.Disabled)]
    [InlineData(AppCheckProviderType.Debug)]
    [InlineData(AppCheckProviderType.DeviceCheck)]
    [InlineData(AppCheckProviderType.AppAttest)]
    public void configuring_the_installed_provider_after_the_default_app_exists_does_nothing(AppCheckProviderType provider)
    {
        _sut.Configure(new AppCheckOptions(provider));
        _defaultAppConfigured = true;

        _sut.Configure(new AppCheckOptions(provider));

        Assert.Equal(new[] { provider }, _factoriesSet);
    }

    [Theory]
    [InlineData(AppCheckProviderType.Disabled, AppCheckProviderType.Debug)]
    [InlineData(AppCheckProviderType.Debug, AppCheckProviderType.Disabled)]
    [InlineData(AppCheckProviderType.DeviceCheck, AppCheckProviderType.AppAttest)]
    public void changing_the_installed_provider_after_the_default_app_exists_throws(
        AppCheckProviderType installed,
        AppCheckProviderType requested)
    {
        _sut.Configure(new AppCheckOptions(installed));
        _defaultAppConfigured = true;

        var exception = Assert.Throws<InvalidOperationException>(
            () => _sut.Configure(new AppCheckOptions(requested)));

        Assert.Contains($"from the '{installed}' provider to '{requested}'", exception.Message);
        Assert.Equal(new[] { installed }, _factoriesSet);
    }

    // The native SDK installs a DeviceCheck factory by default, and the app may set one natively, so when the plugin
    // set none it can't tell which provider is in effect, not even Disabled or DeviceCheck.
    [Theory]
    [InlineData(AppCheckProviderType.Disabled)]
    [InlineData(AppCheckProviderType.Debug)]
    [InlineData(AppCheckProviderType.DeviceCheck)]
    [InlineData(AppCheckProviderType.AppAttest)]
    public void every_provider_throws_when_the_plugin_set_none(AppCheckProviderType requested)
    {
        _defaultAppConfigured = true;

        var exception = Assert.Throws<InvalidOperationException>(
            () => _sut.Configure(new AppCheckOptions(requested)));

        Assert.Contains($"'{requested}'", exception.Message);
        Assert.Contains("didn't install the provider in effect", exception.Message);
        Assert.Empty(_factoriesSet);
    }

    // A failed initialization leaves no default app, so the provider can still change, and the record follows the
    // factory actually set, whichever way the default app is configured afterwards.
    [Fact]
    public void the_provider_can_change_until_the_default_app_exists()
    {
        _sut.Configure(AppCheckOptions.Debug);
        _sut.Configure(AppCheckOptions.AppAttest);
        _defaultAppConfigured = true;

        _sut.Configure(AppCheckOptions.AppAttest);
        Assert.Throws<InvalidOperationException>(() => _sut.Configure(AppCheckOptions.Debug));
    }

    [Fact]
    public void the_provider_can_change_again_after_the_default_app_is_deleted()
    {
        _sut.Configure(AppCheckOptions.Debug);
        _defaultAppConfigured = true;
        Assert.Throws<InvalidOperationException>(() => _sut.Configure(AppCheckOptions.AppAttest));

        _defaultAppConfigured = false;
        _sut.Configure(AppCheckOptions.AppAttest);
        _defaultAppConfigured = true;

        _sut.Configure(AppCheckOptions.AppAttest);
        Assert.Equal(new[] { AppCheckProviderType.Debug, AppCheckProviderType.AppAttest }, _factoriesSet);
    }

    [Fact]
    public void a_failed_native_call_keeps_the_previous_record()
    {
        _sut.Configure(AppCheckOptions.Debug);

        _setFails = true;
        Assert.Throws<InvalidOperationException>(() => _sut.Configure(AppCheckOptions.AppAttest));
        _setFails = false;
        _defaultAppConfigured = true;

        _sut.Configure(AppCheckOptions.Debug);
        Assert.Throws<InvalidOperationException>(() => _sut.Configure(AppCheckOptions.AppAttest));
    }
}
