using Plugin.Firebase.Storage;

namespace Plugin.Firebase.UnitTests;

public class CrossFirebaseStorageReferenceAssemblyTests
{
    [Fact]
    public void is_supported_is_false_in_reference_assembly()
    {
        Assert.False(CrossFirebaseStorage.IsSupported);
    }

    [Fact]
    public void current_throws_in_reference_assembly()
    {
        var exception = Assert.Throws<NotImplementedException>(() => _ = CrossFirebaseStorage.Current);
        Assert.Contains("not implemented in the portable version", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void dispose_safely_resets_reference_assembly_state()
    {
        Assert.False(CrossFirebaseStorage.IsSupported);

        CrossFirebaseStorage.Dispose();

        Assert.False(CrossFirebaseStorage.IsSupported);
    }
}