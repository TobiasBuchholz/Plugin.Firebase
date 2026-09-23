namespace Plugin.Firebase.Firestore;

/// <summary>
/// Sentinel values that can be used when writing document fields with <c>Set()</c> or <c>Update()</c>.
/// </summary>
///
public sealed class FieldValue
{
    /// <summary>
    /// Returns a special value that can be used with <c>Set()</c> or <c>Update()</c> that tells the server to union the given elements
    /// with any array value that already exists on the server. Each specified element that doesn't already exist in the array will be
    /// added to the end. If the field being modified is not already an array it will be overwritten with an array containing exactly
    /// the specified elements.
    /// </summary>
    /// <param name="elements">The elements to union into the array.</param>
    public static FieldValue ArrayUnion(params object?[] elements) =>
        new FieldValue(FieldValueType.ArrayUnion, elements: elements);

    /// <summary>
    /// Returns a special value that can be used with <c>Set()</c> or <c>Update()</c> that tells the server to remove the given elements
    /// from any array value that already exists on the server. All instances of each element specified will be removed from the array.
    /// If the field being modified is not already an array it will be overwritten with an empty array.
    /// </summary>
    /// <param name="elements">The elements to union into the array.</param>
    public static FieldValue ArrayRemove(params object?[] elements) =>
        new FieldValue(FieldValueType.ArrayRemove, elements: elements);

    /// <summary>
    /// Returns a special value that can be used with <c>Set()</c> or <c>Update()</c> that tells the server to increment the field's current value by
    /// the given value.
    /// </summary>
    /// <param name="incrementValue">The value to increment.</param>
    public static FieldValue IntegerIncrement(long incrementValue) =>
        new FieldValue(FieldValueType.IntegerIncrement, incrementValue, integerIncrementValue: incrementValue);

    /// <summary>
    /// Returns a special value that can be used with <c>Set()</c> or <c>Update()</c> that tells the server to increment the field's current value by
    /// the given value.
    /// </summary>
    /// <param name="incrementValue">The value to increment.</param>
    public static FieldValue DoubleIncrement(double incrementValue) =>
        new FieldValue(FieldValueType.DoubleIncrement, incrementValue);

    /// <summary>
    /// Returns a sentinel for use with <c>Update()</c> to mark a field for deletion.
    /// </summary>
    public static FieldValue Delete() => new FieldValue(FieldValueType.Delete);

    /// <summary>
    /// Returns a sentinel for use with <c>Set()</c> or <c>Update()</c> to include a server-generated timestamp in the written data.
    /// </summary>
    /// <returns></returns>
    public static FieldValue ServerTimestamp() => new FieldValue(FieldValueType.ServerTimestamp);

    private readonly long _integerIncrementValue;

    private FieldValue(
        FieldValueType type,
        double incrementValue = 0,
        long integerIncrementValue = 0,
        object?[]? elements = null)
    {
        Type = type;
        Elements = elements;
        IncrementValue = incrementValue;
        _integerIncrementValue = integerIncrementValue;
    }

    /// <summary>
    /// Gets the type of this <c>FieldValue</c> operation.
    /// </summary>
    public FieldValueType Type { get; }

    /// <summary>
    /// Gets the elements for array operations (ArrayUnion, ArrayRemove).
    /// </summary>
    public object?[]? Elements { get; }

    /// <summary>
    /// Gets the increment value for numeric increment operations.
    /// </summary>
    /// <remarks>
    /// For <see cref="FieldValueType.IntegerIncrement"/> this is the nearest <see cref="double"/> to the requested value, so it is
    /// approximate beyond ±2^53. The exact <see cref="long"/> value is what gets sent to Firestore.
    /// </remarks>
    public double IncrementValue { get; }

    // A double can't represent every long beyond ±2^53, so integer increments keep their exact value separately. The
    // native converters read operands through these checked accessors, so reading the wrong increment type fails loudly
    // instead of sending 0 or a rounded value.
    internal long IntegerIncrementValue => Type == FieldValueType.IntegerIncrement
        ? _integerIncrementValue
        : throw CreateOperandMismatchException(FieldValueType.IntegerIncrement);

    internal double DoubleIncrementValue => Type == FieldValueType.DoubleIncrement
        ? IncrementValue
        : throw CreateOperandMismatchException(FieldValueType.DoubleIncrement);

    private InvalidOperationException CreateOperandMismatchException(FieldValueType operandType) =>
        new InvalidOperationException($"The {operandType} operand isn't available on a {Type} field value.");
}