using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.UnitTests;

public class FieldValueTests
{
    [Theory]
    [InlineData(1L)]
    [InlineData(9_007_199_254_740_993L)]
    [InlineData(-9_007_199_254_740_993L)]
    [InlineData(long.MaxValue - 1)]
    [InlineData(long.MaxValue)]
    [InlineData(long.MinValue)]
    public void integer_increment_keeps_the_exact_operand(long operand)
    {
        var fieldValue = FieldValue.IntegerIncrement(operand);

        Assert.Equal(FieldValueType.IntegerIncrement, fieldValue.Type);
        Assert.Equal(operand, fieldValue.IntegerIncrementValue);
        // the public double stays the nearest double to the operand for compatibility
        Assert.Equal((double) operand, fieldValue.IncrementValue);
    }

    [Fact]
    public void double_increment_exposes_its_operand()
    {
        var fieldValue = FieldValue.DoubleIncrement(0.25);

        Assert.Equal(FieldValueType.DoubleIncrement, fieldValue.Type);
        Assert.Equal(0.25, fieldValue.DoubleIncrementValue);
        Assert.Equal(0.25, fieldValue.IncrementValue);
    }

    [Fact]
    public void increment_operands_are_only_available_for_their_own_increment_type()
    {
        Assert.Throws<InvalidOperationException>(() => FieldValue.DoubleIncrement(0.25).IntegerIncrementValue);
        Assert.Throws<InvalidOperationException>(() => FieldValue.IntegerIncrement(1).DoubleIncrementValue);
        Assert.Throws<InvalidOperationException>(() => FieldValue.Delete().IntegerIncrementValue);
        Assert.Throws<InvalidOperationException>(() => FieldValue.ServerTimestamp().DoubleIncrementValue);
        Assert.Throws<InvalidOperationException>(() => FieldValue.ArrayUnion(1L).IntegerIncrementValue);
    }
}