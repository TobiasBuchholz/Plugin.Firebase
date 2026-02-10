using Plugin.Firebase.AppCheck;

namespace Plugin.Firebase.UnitTests;

public class CrossFirebaseAppCheckReferenceAssemblyTests
{
    [Fact]
    public void is_supported_is_false_in_reference_assembly()
    {
        Assert.False(CrossFirebaseAppCheck.IsSupported);
    }

    [Fact]
    public void current_throws_in_reference_assembly()
    {
        var exception = Assert.Throws<NotImplementedException>(() => _ = CrossFirebaseAppCheck.Current);
        Assert.Contains("not implemented in the portable version", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void configure_throws_in_reference_assembly()
    {
        var exception = Assert.Throws<NotImplementedException>(() => CrossFirebaseAppCheck.Configure(AppCheckOptions.Debug));
        Assert.Contains("not implemented in the portable version", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task get_token_throws_in_reference_assembly()
    {
        var exception = await Assert.ThrowsAsync<NotImplementedException>(() => CrossFirebaseAppCheck.GetTokenAsync(forceRefresh: true));
        Assert.Contains("not implemented in the portable version", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}