namespace Plugin.Firebase.Firestore
{
    public sealed class FieldValue
    {
        public static FieldValue ArrayUnion(params object[] elements) => 
            new FieldValue(FieldValueType.ArrayUnion, elements:elements);
        
        public static FieldValue ArrayRemove(params object[] elements) => 
            new FieldValue(FieldValueType.ArrayRemove, elements:elements);
        
        public static FieldValue IntegerIncrement(long incrementValue) => 
            new FieldValue(FieldValueType.IntegerIncrement, incrementValue);
        
        public static FieldValue DoubleIncrement(double incrementValue) => 
            new FieldValue(FieldValueType.IntegerIncrement, incrementValue);
        
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