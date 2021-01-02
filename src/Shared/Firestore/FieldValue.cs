namespace Plugin.Firebase.Firestore
{
    /// <summary>
    /// Sentinel values that can be used when writing document fields with <c>Set()</c> or <c>Update()</c>.
    /// </summary>
    public sealed class FieldValue
    {
        /// <summary>
        /// Returns a special value that can be used with <c>Set()</c> or <c>Update()</c> that tells the server to union the given elements
        /// with any array value that already exists on the server. Each specified element that doesn't already exist in the array will be
        /// added to the end. If the field being modified is not already an array it will be overwritten with an array containing exactly
        /// the specified elements.
        /// </summary>
        /// <param name="elements">The elements to union into the array.</param>
        public static FieldValue ArrayUnion(params object[] elements) => 
            new FieldValue(FieldValueType.ArrayUnion, elements:elements);
        
        /// <summary>
        /// Returns a special value that can be used with <c>Set()</c> or <c>Update()</c> that tells the server to remove the given elements
        /// from any array value that already exists on the server. All instances of each element specified will be removed from the array.
        /// If the field being modified is not already an array it will be overwritten with an empty array.
        /// </summary>
        /// <param name="elements">The elements to union into the array.</param>
        public static FieldValue ArrayRemove(params object[] elements) => 
            new FieldValue(FieldValueType.ArrayRemove, elements:elements);
        
        /// <summary>
        /// Returns a special value that can be used with <c>Set()</c> or <c>Update()</c> that tells the server to increment the field's current value by
        /// the given value.
        /// </summary>
        /// <param name="incrementValue">The value to increment.</param>
        public static FieldValue IntegerIncrement(long incrementValue) => 
            new FieldValue(FieldValueType.IntegerIncrement, incrementValue);
        
        /// <summary>
        /// Returns a special value that can be used with <c>Set()</c> or <c>Update()</c> that tells the server to increment the field's current value by
        /// the given value.
        /// </summary>
        /// <param name="incrementValue">The value to increment.</param>
        public static FieldValue DoubleIncrement(double incrementValue) => 
            new FieldValue(FieldValueType.IntegerIncrement, incrementValue);
        
        /// <summary>
        /// Returns a sentinel for use with <c>Update()</c> to mark a field for deletion.
        /// </summary>
        public static FieldValue Delete() =>
            new FieldValue(FieldValueType.Delete);
        
        /// <summary>
        /// Returns a sentinel for use with <c>Set()</c> or <c>Update()</c> to include a server-generated timestamp in the written data.
        /// </summary>
        /// <returns></returns>
        public static FieldValue ServerTimestamp() =>
            new FieldValue(FieldValueType.ServerTimestamp);
        
        private FieldValue(FieldValueType type, double incrementValue = 0, object[] elements = null)
        {
            Type = type;
            Elements = elements;
            IncrementValue = incrementValue;
        }
        
        public FieldValueType Type { get; }
        public object[] Elements { get; }
        public double IncrementValue { get; }
    }
}