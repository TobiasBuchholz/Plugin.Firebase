using Plugin.Firebase.Core.Utils;

namespace Plugin.Firebase.UnitTests.Utils;

public class FuncEqualityComparerTests
{
    [Fact]
    public void Equals_DelegatesToComparer()
    {
        var calls = 0;
        var comparer = new FuncEqualityComparer<string>((x, y) => {
            calls++;
            return string.Equals(x, y, StringComparison.OrdinalIgnoreCase);
        });

        Assert.True(comparer.Equals("a", "A"));
        Assert.Equal(1, calls);
    }

    [Fact]
    public void GetHashCode_DefaultCtor_ReturnsZero()
    {
        var comparer = new FuncEqualityComparer<string>((x, y) => x == y);

        Assert.Equal(0, comparer.GetHashCode("anything"));
    }

    [Fact]
    public void GetHashCode_DelegatesToHashFunction()
    {
        var comparer = new FuncEqualityComparer<string>((x, y) => x == y, s => s?.Length ?? -1);

        Assert.Equal(3, comparer.GetHashCode("abc"));
        Assert.Equal(-1, comparer.GetHashCode(null!));
    }
}