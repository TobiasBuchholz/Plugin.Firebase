using Plugin.Firebase.Core.Extensions;

namespace Plugin.Firebase.UnitTests.Extensions;

public class EnumerableExtensionsTests
{
    [Fact]
    public void SequenceEqualSafe_BothNull_ReturnsTrue()
    {
        IEnumerable<int> left = null!;
        IEnumerable<int> right = null!;

        Assert.True(left.SequenceEqualSafe(right));
    }

    [Fact]
    public void SequenceEqualSafe_LeftNullRightNotNull_ReturnsFalse()
    {
        IEnumerable<int> left = null!;
        IEnumerable<int> right = new[] { 1 };

        Assert.False(left.SequenceEqualSafe(right));
    }

    [Fact]
    public void SequenceEqualSafe_LeftNotNullRightNull_ReturnsFalse()
    {
        IEnumerable<int> left = new[] { 1 };
        IEnumerable<int> right = null!;

        Assert.False(left.SequenceEqualSafe(right));
    }

    [Fact]
    public void SequenceEqualSafe_EqualSequences_DefaultComparer_ReturnsTrue()
    {
        var left = new[] { 1, 2, 3 };
        var right = new[] { 1, 2, 3 };

        Assert.True(left.SequenceEqualSafe(right));
    }

    [Fact]
    public void SequenceEqualSafe_DifferentSequences_DefaultComparer_ReturnsFalse()
    {
        var left = new[] { 1, 2, 3 };
        var right = new[] { 1, 2, 4 };

        Assert.False(left.SequenceEqualSafe(right));
    }

    [Fact]
    public void SequenceEqualSafe_CustomComparer_IsUsed()
    {
        var left = new[] { "a", "B" };
        var right = new[] { "A", "b" };

        Assert.True(left.SequenceEqualSafe(right, (a, b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public void SequenceEqual_CustomComparer_IsUsed()
    {
        var left = new[] { new Person("a"), new Person("B") };
        var right = new[] { new Person("A"), new Person("b") };

        Assert.True(left.SequenceEqual(right, (a, b) => string.Equals(a.Name, b.Name, StringComparison.OrdinalIgnoreCase)));
    }

    private sealed record Person(string Name);
}