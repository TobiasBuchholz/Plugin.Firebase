using Plugin.Firebase.Installations;

namespace Plugin.Firebase.UnitTests;

public class CrossFirebaseInstallationsReferenceAssemblyTests
{
    [Fact]
    public void is_supported_is_false_in_reference_assembly()
    {
        Assert.False(CrossFirebaseInstallations.IsSupported);
    }

    [Fact]
    public void current_throws_in_reference_assembly()
    {
        var exception = Assert.Throws<NotImplementedException>(() => _ = CrossFirebaseInstallations.Current);
        Assert.Contains("not implemented in the portable version", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task get_id_throws_in_reference_assembly()
    {
        var exception = await Assert.ThrowsAsync<NotImplementedException>(() => CrossFirebaseInstallations.GetIdAsync());
        Assert.Contains("not implemented in the portable version", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task get_token_throws_in_reference_assembly()
    {
        var exception = await Assert.ThrowsAsync<NotImplementedException>(() => CrossFirebaseInstallations.GetTokenAsync(forceRefresh: true));
        Assert.Contains("not implemented in the portable version", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task delete_throws_in_reference_assembly()
    {
        var exception = await Assert.ThrowsAsync<NotImplementedException>(() => CrossFirebaseInstallations.DeleteAsync());
        Assert.Contains("not implemented in the portable version", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}