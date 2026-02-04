using Plugin.Firebase.AppCheck;

namespace Plugin.Firebase.UnitTests;

public class AppCheckOptionsTests
{
    [Theory]
    [InlineData(AppCheckProviderType.Disabled)]
    [InlineData(AppCheckProviderType.Debug)]
    [InlineData(AppCheckProviderType.DeviceCheck)]
    [InlineData(AppCheckProviderType.AppAttest)]
    [InlineData(AppCheckProviderType.PlayIntegrity)]
    public void constructor_sets_provider(AppCheckProviderType provider)
    {
        var sut = new AppCheckOptions(provider);

        Assert.Equal(provider, sut.Provider);
    }

    [Fact]
    public void static_presets_map_to_expected_provider()
    {
        Assert.Equal(AppCheckProviderType.Disabled, AppCheckOptions.Disabled.Provider);
        Assert.Equal(AppCheckProviderType.Debug, AppCheckOptions.Debug.Provider);
        Assert.Equal(AppCheckProviderType.DeviceCheck, AppCheckOptions.DeviceCheck.Provider);
        Assert.Equal(AppCheckProviderType.AppAttest, AppCheckOptions.AppAttest.Provider);
        Assert.Equal(AppCheckProviderType.PlayIntegrity, AppCheckOptions.PlayIntegrity.Provider);
    }
}