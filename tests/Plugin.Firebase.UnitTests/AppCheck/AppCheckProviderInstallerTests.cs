using Plugin.Firebase.AppCheck;
using Plugin.Firebase.Core;

namespace Plugin.Firebase.UnitTests;

[Collection(FirebaseInitializationHooksTests.CollectionName)]
public class AppCheckProviderInstallerTests
{
    private readonly List<AppCheckProviderType> _installedProviders = new();
    private readonly AppCheckProviderInstaller _sut;
    private bool _installSucceeds = true;

    public AppCheckProviderInstallerTests()
    {
        FirebaseInitializationHooks.Reset();
        _sut = new AppCheckProviderInstaller(provider => {
            _installedProviders.Add(provider);
            return _installSucceeds;
        });
    }

    [Fact]
    public void configure_before_initialize_installs_latest_provider_on_initialize()
    {
        _sut.Configure(AppCheckOptions.Debug);
        _sut.Configure(AppCheckOptions.PlayIntegrity);

        Assert.Empty(_installedProviders);

        FirebaseInitializationHooks.InvokeAfterInitialize();

        Assert.Equal(new[] { AppCheckProviderType.PlayIntegrity }, _installedProviders);
    }

    [Fact]
    public void configure_after_initialize_installs_every_provider_change()
    {
        FirebaseInitializationHooks.InvokeAfterInitialize();

        _sut.Configure(AppCheckOptions.Debug);
        _sut.Configure(AppCheckOptions.PlayIntegrity);
        _sut.Configure(AppCheckOptions.Debug);

        Assert.Equal(
            new[] { AppCheckProviderType.Debug, AppCheckProviderType.PlayIntegrity, AppCheckProviderType.Debug },
            _installedProviders);
    }

    [Fact]
    public void configure_after_initialize_replaces_provider_installed_on_initialize()
    {
        _sut.Configure(AppCheckOptions.Debug);
        FirebaseInitializationHooks.InvokeAfterInitialize();

        _sut.Configure(AppCheckOptions.PlayIntegrity);

        Assert.Equal(new[] { AppCheckProviderType.Debug, AppCheckProviderType.PlayIntegrity }, _installedProviders);
    }

    [Fact]
    public void disabling_before_initialize_cancels_pending_installation()
    {
        _sut.Configure(AppCheckOptions.Debug);
        _sut.Configure(AppCheckOptions.Disabled);

        FirebaseInitializationHooks.InvokeAfterInitialize();

        Assert.Empty(_installedProviders);
    }

    [Fact]
    public void disabling_after_initialize_is_accepted_when_no_provider_was_installed()
    {
        FirebaseInitializationHooks.InvokeAfterInitialize();

        _sut.Configure(AppCheckOptions.Disabled);

        Assert.Empty(_installedProviders);
    }

    [Fact]
    public void disabling_after_a_provider_is_installed_throws()
    {
        FirebaseInitializationHooks.InvokeAfterInitialize();
        _sut.Configure(AppCheckOptions.Debug);

        var exception = Assert.Throws<InvalidOperationException>(() => _sut.Configure(AppCheckOptions.Disabled));

        Assert.Contains("'Debug'", exception.Message);
        Assert.Equal(new[] { AppCheckProviderType.Debug }, _installedProviders);
    }

    [Fact]
    public void rejected_disable_keeps_the_provider_configured()
    {
        _sut.Configure(AppCheckOptions.Debug);
        FirebaseInitializationHooks.InvokeAfterInitialize();

        Assert.Throws<InvalidOperationException>(() => _sut.Configure(AppCheckOptions.Disabled));
        FirebaseInitializationHooks.InvokeAfterInitialize();

        Assert.Equal(new[] { AppCheckProviderType.Debug, AppCheckProviderType.Debug }, _installedProviders);
    }

    [Fact]
    public void provider_that_failed_to_install_does_not_block_disabling()
    {
        _installSucceeds = false;
        FirebaseInitializationHooks.InvokeAfterInitialize();
        _sut.Configure(AppCheckOptions.Debug);

        _sut.Configure(AppCheckOptions.Disabled);

        Assert.Equal(new[] { AppCheckProviderType.Debug }, _installedProviders);
    }

    [Fact]
    public void cancel_pending_installation_skips_install_on_initialize()
    {
        _sut.Configure(AppCheckOptions.Debug);
        _sut.CancelPendingInstallation();

        FirebaseInitializationHooks.InvokeAfterInitialize();

        Assert.Empty(_installedProviders);
    }
}