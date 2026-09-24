using Plugin.Firebase.AppCheck;
using Plugin.Firebase.Core;

namespace Plugin.Firebase.UnitTests;

[Collection(FirebaseInitializationHooksTests.CollectionName)]
public class AppCheckProviderInstallerTests
{
    private readonly object _firstApp = new();
    private readonly object _secondApp = new();
    private readonly List<(object App, AppCheckProviderType Provider)> _installs = new();
    private readonly AppCheckProviderInstaller<object> _sut;
    private object? _defaultApp;
    private bool _installFails;

    public AppCheckProviderInstallerTests()
    {
        FirebaseInitializationHooks.Reset();
        _defaultApp = _firstApp;
        _sut = CreateInstaller();
    }

    [Fact]
    public void configure_before_initialize_installs_latest_provider_on_initialize()
    {
        _sut.Configure(AppCheckOptions.Debug);
        _sut.Configure(AppCheckOptions.PlayIntegrity);

        Assert.Empty(_installs);

        FirebaseInitializationHooks.InvokeAfterInitialize();

        Assert.Equal(new[] { (_firstApp, AppCheckProviderType.PlayIntegrity) }, _installs);
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
            _installs.Select(x => x.Provider));
    }

    [Fact]
    public void configure_after_initialize_replaces_provider_installed_on_initialize()
    {
        _sut.Configure(AppCheckOptions.Debug);
        FirebaseInitializationHooks.InvokeAfterInitialize();

        _sut.Configure(AppCheckOptions.PlayIntegrity);

        Assert.Equal(
            new[] { AppCheckProviderType.Debug, AppCheckProviderType.PlayIntegrity },
            _installs.Select(x => x.Provider));
    }

    [Fact]
    public void configuring_the_installed_provider_again_does_not_reinstall_it()
    {
        _sut.Configure(AppCheckOptions.Debug);
        FirebaseInitializationHooks.InvokeAfterInitialize();

        _sut.Configure(AppCheckOptions.Debug);
        FirebaseInitializationHooks.InvokeAfterInitialize();

        Assert.Equal(new[] { (_firstApp, AppCheckProviderType.Debug) }, _installs);
    }

    [Fact]
    public void installer_first_used_after_initialize_installs_on_configure()
    {
        FirebaseInitializationHooks.InvokeAfterInitialize();

        _sut.Configure(AppCheckOptions.Debug);

        Assert.Equal(new[] { (_firstApp, AppCheckProviderType.Debug) }, _installs);
    }

    [Fact]
    public void disabling_before_initialize_installs_nothing()
    {
        _sut.Configure(AppCheckOptions.Debug);
        _sut.Configure(AppCheckOptions.Disabled);

        FirebaseInitializationHooks.InvokeAfterInitialize();

        Assert.Empty(_installs);
    }

    [Fact]
    public void disabling_after_initialize_is_accepted_when_no_provider_was_installed()
    {
        FirebaseInitializationHooks.InvokeAfterInitialize();

        _sut.Configure(AppCheckOptions.Disabled);

        Assert.Empty(_installs);
    }

    [Fact]
    public void disabling_after_a_provider_is_installed_throws()
    {
        FirebaseInitializationHooks.InvokeAfterInitialize();
        _sut.Configure(AppCheckOptions.Debug);

        var exception = Assert.Throws<InvalidOperationException>(() => _sut.Configure(AppCheckOptions.Disabled));

        Assert.Contains("'Debug'", exception.Message);
        Assert.Equal(new[] { (_firstApp, AppCheckProviderType.Debug) }, _installs);
    }

    [Fact]
    public void rejected_disable_keeps_the_provider_for_a_recreated_app()
    {
        _sut.Configure(AppCheckOptions.Debug);
        FirebaseInitializationHooks.InvokeAfterInitialize();
        Assert.Throws<InvalidOperationException>(() => _sut.Configure(AppCheckOptions.Disabled));

        _defaultApp = _secondApp;
        FirebaseInitializationHooks.InvokeAfterInitialize();

        Assert.Equal(
            new[] { (_firstApp, AppCheckProviderType.Debug), (_secondApp, AppCheckProviderType.Debug) },
            _installs);
    }

    [Fact]
    public void disabling_is_accepted_once_the_app_with_the_factory_is_deleted()
    {
        FirebaseInitializationHooks.InvokeAfterInitialize();
        _sut.Configure(AppCheckOptions.Debug);

        _defaultApp = null;
        _sut.Configure(AppCheckOptions.Disabled);
        _defaultApp = _secondApp;
        FirebaseInitializationHooks.InvokeAfterInitialize();
        _sut.Configure(AppCheckOptions.Disabled);

        Assert.Equal(new[] { (_firstApp, AppCheckProviderType.Debug) }, _installs);
    }

    [Fact]
    public void provider_configured_while_the_default_app_is_missing_is_installed_on_the_next_initialize()
    {
        _sut.Configure(AppCheckOptions.Disabled);
        FirebaseInitializationHooks.InvokeAfterInitialize();
        _defaultApp = null;

        _sut.Configure(AppCheckOptions.PlayIntegrity);
        Assert.Empty(_installs);

        _defaultApp = _secondApp;
        FirebaseInitializationHooks.InvokeAfterInitialize();

        Assert.Equal(new[] { (_secondApp, AppCheckProviderType.PlayIntegrity) }, _installs);
    }

    [Fact]
    public void failed_install_is_not_installed_by_a_later_initialize()
    {
        _sut.Configure(AppCheckOptions.Disabled);
        FirebaseInitializationHooks.InvokeAfterInitialize();
        _installFails = true;

        Assert.Throws<ApplicationException>(() => _sut.Configure(AppCheckOptions.PlayIntegrity));
        _installFails = false;
        _defaultApp = _secondApp;
        FirebaseInitializationHooks.InvokeAfterInitialize();

        Assert.Empty(_installs);
    }

    [Fact]
    public void failed_native_install_is_not_recorded()
    {
        FirebaseInitializationHooks.InvokeAfterInitialize();
        _installFails = true;

        Assert.Throws<ApplicationException>(() => _sut.Configure(AppCheckOptions.Debug));
        _sut.Configure(AppCheckOptions.Disabled);
        _installFails = false;
        _sut.Configure(AppCheckOptions.Debug);

        Assert.Equal(new[] { (_firstApp, AppCheckProviderType.Debug) }, _installs);
    }

    private AppCheckProviderInstaller<object> CreateInstaller()
    {
        return new AppCheckProviderInstaller<object>(
            () => _defaultApp,
            (app, provider) => {
                if(_installFails) {
                    throw new ApplicationException("Native install failed.");
                }

                _installs.Add((app, provider));
            },
            ReferenceEquals);
    }
}