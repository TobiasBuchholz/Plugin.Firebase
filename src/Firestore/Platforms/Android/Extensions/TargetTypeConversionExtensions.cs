namespace Plugin.Firebase.Firestore.Platforms.Android.Extensions;

internal static class TargetTypeConversionExtensions
{
    public static Type? GetConversionType(this Type? targetType)
    {
        return targetType == null
            ? null
            : Nullable.GetUnderlyingType(targetType) ?? targetType;
    }

    public static object? ConvertToTargetType(this object? value, Type? targetType)
    {
        var conversionType = targetType.GetConversionType();
        if(value == null || conversionType == null || conversionType == typeof(object) || conversionType.IsInstanceOfType(value)) {
            return value;
        }

        if(conversionType.IsEnum) {
            return Enum.ToObject(conversionType, value);
        }

        return Convert.ChangeType(value, conversionType);
    }
}
