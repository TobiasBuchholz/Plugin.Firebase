using Plugin.Firebase.Storage;

namespace Plugin.Firebase.UnitTests;

public class StorageListArgumentsTests
{
    [Theory]
    [InlineData(1L)]
    [InlineData(2L)]
    [InlineData(1000L)]
    public void accepts_max_results_in_native_range(long maxResults)
    {
        StorageListArguments.ThrowIfMaxResultsOutOfRange(maxResults);
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(-1L)]
    [InlineData(1001L)]
    [InlineData(4294967297L)] // Truncates to 1 when cast to int.
    [InlineData(long.MinValue)]
    [InlineData(long.MaxValue)]
    public void rejects_max_results_outside_native_range(long maxResults)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => StorageListArguments.ThrowIfMaxResultsOutOfRange(maxResults)
        );

        Assert.Equal("maxResults", exception.ParamName);
        Assert.Equal(maxResults, exception.ActualValue);
    }
}