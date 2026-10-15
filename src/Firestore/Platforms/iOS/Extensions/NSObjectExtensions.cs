using System.Collections;
using System.Diagnostics;
using Firebase.CloudFirestore;
using Firebase.Core;
using Plugin.Firebase.Core.Platforms.iOS.Extensions;
using Plugin.Firebase.Firestore.Internal;

namespace Plugin.Firebase.Firestore.Platforms.iOS.Extensions;

/// <summary>
/// Extension methods for converting between .NET objects and native iOS NSObject types.
/// </summary>
public static class NSObjectExtensions
{
    // NSNumber.FromBoolean returns the kCFBooleanTrue and kCFBooleanFalse singletons, which are the
    // only CFBoolean instances and what Firestore returns for booleans.
    private static readonly NSNumber BooleanTrue = NSNumber.FromBoolean(true);
    private static readonly NSNumber BooleanFalse = NSNumber.FromBoolean(false);

    /// <summary>
    /// Casts a native iOS NSDictionary to a typed .NET object.
    /// </summary>
    /// <typeparam name="T">The target type to cast to.</typeparam>
    /// <param name="this">The NSDictionary to cast.</param>
    /// <param name="documentId">Optional document ID to set on the object.</param>
    /// <returns>The typed object.</returns>
    public static T? Cast<T>(this NSDictionary @this, string? documentId = null)
    {
        return (T?) @this.Cast(typeof(T), documentId);
    }

    /// <summary>
    /// Casts a native iOS NSDictionary to a .NET object of the specified type.
    /// </summary>
    /// <param name="this">The NSDictionary to cast.</param>
    /// <param name="targetType">The target type to cast to.</param>
    /// <param name="documentId">Optional document ID to set on the object.</param>
    /// <returns>The converted object.</returns>
    public static object? Cast(this NSDictionary @this, Type targetType, string? documentId = null)
    {
        if(targetType == typeof(object) || IsDictionaryType(targetType)) {
            return @this.ToDictionaryObject(targetType);
        }

        var instance = Activator.CreateInstance(targetType);
        var properties = targetType.GetProperties();
        foreach(var property in properties) {
            if(
                documentId != null
                && property.GetCustomAttributes(typeof(FirestoreDocumentIdAttribute), true).Any()
            ) {
                property.SetValue(instance, documentId);
                continue;
            }

            var attributes = property.GetCustomAttributes(typeof(FirestorePropertyAttribute), true);
            if(attributes.Any()) {
                var attribute = (FirestorePropertyAttribute) attributes[0];
                var value = @this[attribute.PropertyName];
                if(value == null) {
                    Debug.WriteLine(
                        $"[Plugin.Firebase] Couldn't cast property '{attribute.PropertyName}' of '{targetType}' because it's not contained in the NSDictionary."
                    );
                } else {
                    property.SetValue(instance, value.ToObject(property.PropertyType, attribute.PropertyName));
                }
            }

            var timestampAttributes = property.GetCustomAttributes(
                typeof(FirestoreServerTimestampAttribute),
                true
            );
            if(timestampAttributes.Any()) {
                var attribute = (FirestoreServerTimestampAttribute) timestampAttributes[0];
                var value = @this[attribute.PropertyName];
                if(value == null) {
                    Debug.WriteLine(
                        $"[Plugin.Firebase] Couldn't cast property '{attribute.PropertyName}' of '{targetType}' because value is null"
                    );
                } else if(
                      property.PropertyType == typeof(DateTimeOffset)
                      && value is Timestamp timestamp
                  ) {
                    property.SetValue(instance, timestamp.ToDateTimeOffset());
                } else {
                    Debug.WriteLine(
                        $"[Plugin.Firebase] Couldn't cast property '{attribute.PropertyName}' of '{targetType}' because properties that use the {nameof(FirestoreServerTimestampAttribute)} need to be of type {nameof(DateTimeOffset)} and value of type {nameof(Timestamp)}"
                    );
                }
            }
        }
        return instance;
    }

    /// <summary>
    /// Converts a native iOS NSObject to a .NET object.
    /// </summary>
    /// <remarks>
    /// With a <paramref name="targetType"/>, the rules are the same as on Android: the stored value has
    /// to match the target type, except that a number read into a numeric or enum target is range
    /// checked and, if it is fractional and the target is integral or an enum, truncated toward
    /// zero. Any other mismatch throws, for example a number read into a <see cref="string"/>, text
    /// read into a number, a boolean read into a number or anything read into a <see cref="char"/>.
    /// </remarks>
    /// <param name="this">The NSObject to convert.</param>
    /// <param name="targetType">Optional target type for the conversion.</param>
    /// <returns>The converted .NET object.</returns>
    /// <exception cref="ArgumentException">Thrown if the NSObject type cannot be converted.</exception>
    /// <exception cref="InvalidCastException">
    /// Thrown if the stored value's type doesn't match <paramref name="targetType"/>.
    /// </exception>
    /// <exception cref="OverflowException">
    /// Thrown if a stored number doesn't fit into <paramref name="targetType"/>, or if it is NaN or
    /// infinity and <paramref name="targetType"/> is an integral, enum or <see cref="decimal"/> type.
    /// </exception>
    public static object? ToObject(this NSObject @this, Type? targetType = null)
    {
        return @this.ToObject(targetType, field: null);
    }

    internal static object? ToObject(this NSObject @this, Type? targetType, string? field)
    {
        // the rules are shared with Android: the value has to match the target type, except that
        // numbers convert between numeric and enum types, so a mismatch throws InvalidCastException
        return FirestoreValueConversion.ToTargetType(@this.Unwrap(targetType, field), targetType, field);
    }

    private static object? Unwrap(this NSObject @this, Type? targetType, string? field)
    {
        switch(@this) {
            case NSNumber x:
                return targetType == null || targetType == typeof(object)
                    ? x.ToUntypedObject()
                    : x.ToConversionSource();
            case NSString x:
                return x.ToString();
            case NSDate x:
                return x.ToDateTimeOffset();
            case NSDictionary x:
                FirestoreValueConversion.EnsureMapTarget(targetType, field);
                return x.ToDictionaryObject(targetType);
            case NSArray x:
                if(targetType is null) {
                    return x.ToList();
                }
                FirestoreValueConversion.EnsureArrayTarget(targetType, field);
                return x.ToList(GetGenericListType(targetType), field);
            case global::Firebase.CloudFirestore.GeoPoint x:
                return new GeoPoint(x.Latitude, x.Longitude);
            case Timestamp x:
                if(targetType == typeof(DateTime))
                    return x.ToDateTime();
                else
                    return x.ToDateTimeOffset();
            case DocumentReference x:
                return new DocumentReferenceWrapper(x);
            case NSNull:
                return null;
            default:
                throw new ArgumentException(
                    $"Could not convert NSObject of type {@this.GetType()} to object"
                );
        }
    }

    /// <summary>
    /// Converts a native iOS NSNumber, which holds a Firestore number or boolean, to a .NET object.
    /// </summary>
    /// <remarks>
    /// Without a <paramref name="targetType"/>, or with <see cref="object"/>, the value keeps the type
    /// it is stored as. With a target type, the rules are the same as on Android: a boolean only reads
    /// into a <see cref="bool"/>, and a number only reads into a numeric or enum type. A fractional
    /// number read into an integral or enum target is range checked as stored and then truncated
    /// toward zero, and an enum is range checked against its underlying type.
    /// </remarks>
    /// <param name="this">The NSNumber to convert.</param>
    /// <param name="targetType">Optional target type for the conversion.</param>
    /// <returns>The stored boolean or number, converted to <paramref name="targetType"/> if one is given.</returns>
    /// <exception cref="OverflowException">
    /// Thrown if the stored number doesn't fit into <paramref name="targetType"/>, or if it is NaN or
    /// infinity and <paramref name="targetType"/> is an integral, enum or <see cref="decimal"/> type.
    /// </exception>
    /// <exception cref="InvalidCastException">
    /// Thrown if a number is read into a type that isn't numeric or an enum, such as
    /// <see cref="string"/>, <see cref="bool"/>, <see cref="char"/> or <see cref="DateTime"/>, or if
    /// a boolean is read into a type other than <see cref="bool"/>.
    /// </exception>
    public static object? ToObject(this NSNumber @this, Type? targetType = null)
    {
        return @this.ToObject(targetType, field: null);
    }

    /// <summary>
    /// Gets the stored value in its widest .NET representation, so that
    /// <see cref="FirestoreNumericConversion"/> range checks the target conversion instead of the
    /// NSNumber accessors silently truncating it.
    /// </summary>
    /// <remarks>
    /// This deliberately widens where <see cref="ToUntypedObject"/> preserves the stored width:
    /// an untyped read hands the caller the value's own type, while a typed read only needs a
    /// source wide enough for the target's range check. Booleans are the two CFBoolean singletons,
    /// which report the same "c" type encoding as a signed char, so they are told apart by identity
    /// and are never read as numbers.
    /// </remarks>
    private static object ToConversionSource(this NSNumber @this)
    {
        if(@this.Handle == BooleanTrue.Handle || @this.Handle == BooleanFalse.Handle) {
            return @this.BoolValue;
        }

        switch(@this.ObjCType) {
            case "f":
                return @this.FloatValue;
            case "d":
                return @this.DoubleValue;
            case "Q":
            case "L":
                return @this.UInt64Value;
            default:
                return @this.Int64Value;
        }
    }

    private static object ToUntypedObject(this NSNumber @this)
    {
        switch(@this.ObjCType) {
            case "B":
            case "c":
                return @this.BoolValue;
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

    private static Type GetGenericListType(Type targetType)
    {
        if(targetType == null || targetType == typeof(object)) {
            return typeof(object);
        }

        var genericType = targetType.GenericTypeArguments.FirstOrDefault();
        if(genericType == null) {
            throw new ArgumentException(
                $"Couldn't get generic list type of targetType {targetType}. Make sure to use a list IList<T> instead of an array T[] as type in your FirestoreObject."
            );
        }
        return genericType;
    }

    private static bool IsDictionaryType(Type targetType)
    {
        return targetType.IsGenericType
               && (
                   targetType.GetGenericTypeDefinition() == typeof(IDictionary<,>)
                   || targetType.GetGenericTypeDefinition() == typeof(Dictionary<,>)
               );
    }

    /// <summary>
    /// Converts a .NET object to a native iOS NSObject.
    /// </summary>
    /// <param name="this">The object to convert.</param>
    /// <returns>The native iOS NSObject representation.</returns>
    /// <exception cref="ArgumentException">Thrown if the object type cannot be converted.</exception>
    public static NSObject ToNSObject(this object? @this)
    {
        switch(@this) {
            case null:
                return new NSNull();
            case NSObject x:
                return x;
            case string x:
                return new NSString(x);
            case int x:
                return new NSNumber(x);
            case long x:
                return new NSNumber(x);
            case short x:
                return new NSNumber(x);
            case float x:
                return new NSNumber(x);
            case double x:
                return new NSNumber(x);
            case bool x:
                return new NSNumber(x);
            case DateTime x:
                return x.ToNSDate();
            case DateTimeOffset x:
                return x.ToNSDate();
            case FieldValue x:
                return x.ToNative();
            case Plugin.Firebase.Firestore.GeoPoint x:
                return new global::Firebase.CloudFirestore.GeoPoint(x.Latitude, x.Longitude);
            case IList x:
                return x.ToNSArray();
            case IDictionary x:
                return x.ToNSDictionaryFromNonGeneric();
            case DocumentReferenceWrapper x:
                return x.Wrapped;
            case IFirestoreObject x:
                return x.ToNSObject();
            default:
                if(@this is Enum) {
                    var enumType = Enum.GetUnderlyingType(@this.GetType());
                    if(enumType == typeof(int)) {
                        return new NSNumber(Convert.ToInt32(@this));
                    } else if(enumType == typeof(uint)) {
                        return new NSNumber(Convert.ToUInt32(@this));
                    } else if(enumType == typeof(long)) {
                        return new NSNumber(Convert.ToInt64(@this));
                    } else if(enumType == typeof(ulong)) {
                        return new NSNumber(Convert.ToUInt64(@this));
                    } else if(enumType == typeof(sbyte)) {
                        return new NSNumber(Convert.ToSByte(@this));
                    } else if(enumType == typeof(byte)) {
                        return new NSNumber(Convert.ToByte(@this));
                    } else if(enumType == typeof(short)) {
                        return new NSNumber(Convert.ToInt16(@this));
                    } else if(enumType == typeof(ushort)) {
                        return new NSNumber(Convert.ToUInt16(@this));
                    }
                }
                throw new ArgumentException(
                    $"Could not convert object of type {@this.GetType()} to NSObject. Does it extend {nameof(IFirestoreObject)}?"
                );
        }
    }

    /// <summary>
    /// Converts an <see cref="IFirestoreObject"/> to a native iOS NSObject.
    /// </summary>
    /// <param name="this">The Firestore object to convert.</param>
    /// <returns>The native iOS NSObject representation.</returns>
    public static NSObject ToNSObject(this IFirestoreObject @this)
    {
        return @this.ToDictionary().ToNSObject();
    }
}