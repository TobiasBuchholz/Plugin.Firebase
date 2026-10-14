using System.Runtime.InteropServices;
using CoreFoundation;
using ObjCRuntime;
using Plugin.Firebase.Auth.Internal;
using Plugin.Firebase.Core.Platforms.iOS.Extensions;

namespace Plugin.Firebase.Auth.Platforms.iOS.Extensions;

/// <summary>
/// Provides extension methods for converting native iOS NSObject types to .NET objects.
/// </summary>
public static class NSObjectExtensions
{
    private static readonly nint BooleanTypeId = CFBooleanGetTypeID();

    /// <summary>
    /// Converts an NSObject to a .NET object of the specified target type.
    /// </summary>
    /// <param name="this">The NSObject to convert.</param>
    /// <param name="targetType">The optional target type for the conversion.</param>
    /// <returns>The converted .NET object, or null for NSNull values.</returns>
    /// <exception cref="ArgumentException">Thrown when the NSObject type is not supported for conversion.</exception>
    /// <exception cref="OverflowException">Thrown when a number doesn't fit the numeric or enum target type.</exception>
    /// <exception cref="InvalidCastException">
    /// Thrown when a string or boolean is read as a numeric or enum type, or any value is read as <see cref="char"/>.
    /// </exception>
    public static object? ToObject(this NSObject @this, Type? targetType = null)
    {
        switch(@this) {
            case NSNumber x:
                return x.ToObject(targetType);
            case NSString x:
                return ClaimValueConverter.ToRequestedType(x.ToString(), targetType);
            case NSDate x:
                return x.ToDateTimeOffset();
            case NSDictionary x:
                return x.ToDictionaryObject(targetType);
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
    /// <remarks>
    /// A boolean NSNumber (a CFBoolean, which is what JSON <c>true</c> and <c>false</c> parse to) converts to
    /// <see cref="bool"/> and is never read as a number. Any other NSNumber converts to a numeric or enum target
    /// without wrapping: fractional values truncate toward zero, and values that don't fit throw
    /// <see cref="OverflowException"/>. For any other target the number is returned as a long, ulong or double, so a
    /// cast to an unrelated type such as <see cref="string"/> or <see cref="bool"/> fails.
    /// </remarks>
    /// <param name="this">The NSNumber to convert.</param>
    /// <param name="targetType">The optional target type for the conversion. Without one the number keeps its natural .NET type.</param>
    /// <returns>The converted .NET value.</returns>
    /// <exception cref="OverflowException">
    /// The value doesn't fit the numeric or enum target, or it is NaN or infinite and the target is integral, an enum
    /// or <see cref="decimal"/>.
    /// </exception>
    /// <exception cref="InvalidCastException">
    /// The NSNumber is a boolean and the target is numeric or an enum, or the target is <see cref="char"/>.
    /// </exception>
    public static object? ToObject(this NSNumber @this, Type? targetType = null)
    {
        if(@this.IsBoolean()) {
            return ClaimValueConverter.ToRequestedType(@this.BoolValue, targetType);
        }

        if(targetType == null || targetType == typeof(object)) {
            return @this.ToUntypedObject();
        }

        return ClaimValueConverter.ToRequestedType(@this.ToWidenedValue(), targetType);
    }

    private static bool IsBoolean(this NSNumber @this)
    {
        return @this.ObjCType == "B" || CFType.GetTypeID(@this.Handle) == BooleanTypeId;
    }

    [DllImport(Constants.CoreFoundationLibrary)]
    private static extern nint CFBooleanGetTypeID();

    /// <summary>
    /// Reads the stored value as long, ulong or double, which hold every value of the NSNumber's ObjCType, so the
    /// conversion to the requested type range checks it instead of a narrower NSNumber accessor wrapping it.
    /// </summary>
    private static object ToWidenedValue(this NSNumber @this)
    {
        switch(@this.ObjCType) {
            case "f":
            case "d":
                return @this.DoubleValue;
            case "C":
            case "S":
            case "I":
            case "L":
            case "Q":
                return @this.UInt64Value;
            default:
                return @this.Int64Value;
        }
    }

    private static object ToUntypedObject(this NSNumber @this)
    {
        switch(@this.ObjCType) {
            case "c":
                return @this.SByteValue;
            case "C":
                return @this.ByteValue;
            case "s":
                return @this.Int16Value;
            case "S":
                return @this.UInt16Value;
            case "i":
                return @this.Int32Value;
            case "I":
                return @this.UInt32Value;
            case "q":
            case "l":
                return @this.Int64Value;
            case "Q":
            case "L":
                return @this.UInt64Value;
            case "f":
                return @this.FloatValue;
            case "d":
                return @this.DoubleValue;
            default:
                return @this.Int64Value;
        }
    }

    private static Type GetGenericListType(Type? targetType)
    {
        return targetType?.GenericTypeArguments?.FirstOrDefault() ?? typeof(object);
    }
}