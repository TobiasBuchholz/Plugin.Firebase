using Plugin.Firebase.Core.Platforms.iOS.Extensions;

namespace Plugin.Firebase.Auth.Platforms.iOS.Extensions;

/// <summary>
/// Provides extension methods for converting native iOS NSObject types to .NET objects.
/// </summary>
public static class NSObjectExtensions
{
    /// <summary>
    /// Converts an NSObject to a .NET object of the specified target type.
    /// </summary>
    /// <param name="this">The NSObject to convert.</param>
    /// <param name="targetType">The optional target type for the conversion.</param>
    /// <returns>The converted .NET object, or null for NSNull values.</returns>
    /// <exception cref="ArgumentException">Thrown when the NSObject type is not supported for conversion.</exception>
    public static object ToObject(this NSObject @this, Type targetType = null)
    {
        switch(@this) {
            case NSNumber x:
                return x.ToObject(targetType);
            case NSString x:
                return x.ToString();
            case NSDate x:
                return x.ToDateTimeOffset();
            case NSArray x:
                return x.ToList(GetGenericListType(targetType)!);
            case NSNull:
                return null;
            default:
                throw new ArgumentException(
                    $"Could not convert NSObject of type {@this.GetType()} to object"
                );
        }
    }

    /// <summary>
    /// Converts an NSNumber to a .NET object of the specified target type.
    /// </summary>
    /// <param name="this">The NSNumber to convert.</param>
    /// <param name="targetType">The optional target type for the conversion. Defaults to Int32 if not specified.</param>
    /// <returns>The converted .NET numeric value, or null if the type is not supported.</returns>
    public static object ToObject(this NSNumber @this, Type targetType = null)
    {
        if(targetType == null) {
            return @this.Int32Value;
        }

        switch(Type.GetTypeCode(targetType)) {
            case TypeCode.Boolean:
                return @this.BoolValue;
            case TypeCode.Char:
                return Convert.ToChar(@this.ByteValue);
            case TypeCode.SByte:
                return @this.SByteValue;
            case TypeCode.Byte:
                return @this.ByteValue;
            case TypeCode.Int16:
                return @this.Int16Value;
            case TypeCode.UInt16:
                return @this.UInt16Value;
            case TypeCode.Int32:
                return @this.Int32Value;
            case TypeCode.UInt32:
                return @this.UInt32Value;
            case TypeCode.Int64:
                return @this.Int64Value;
            case TypeCode.UInt64:
                return @this.UInt64Value;
            case TypeCode.Single:
                return @this.FloatValue;
            case TypeCode.Double:
                return @this.DoubleValue;
            default:
                return null;
        }
    }

    private static Type GetGenericListType(Type targetType)
    {
        var genericType = targetType.GenericTypeArguments?.FirstOrDefault();
        if(genericType == null) {
            throw new ArgumentException(
                $"Couldn't get generic list type of targetType {targetType}. Make sure to use a list IList<T> instead of an array T[] as type in your FirestoreObject."
            );
        }
        return genericType;
    }
}