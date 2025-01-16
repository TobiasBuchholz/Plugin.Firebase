using System.Collections;
using System.Diagnostics;
using Firebase.CloudFirestore;
using Firebase.Core;
using Foundation;
using Plugin.Firebase.Core.Platforms.iOS.Extensions;

namespace Plugin.Firebase.Firestore.Platforms.iOS.Extensions;

public static class NSObjectExtensions
{
    public static T Cast<T>(this NSDictionary @this, string documentId = null)
    {
        return (T) @this.Cast(typeof(T), documentId);
    }

    public static object Cast(this NSDictionary @this, Type targetType, string documentId = null)
    {
        var instance = Activator.CreateInstance(targetType);
        var properties = targetType.GetProperties();
        foreach(var property in properties) {
            if(documentId != null && property.GetCustomAttributes(typeof(FirestoreDocumentIdAttribute), true).Any()) {
                property.SetValue(instance, documentId);
                continue;
            }

            var attributes = property.GetCustomAttributes(typeof(FirestorePropertyAttribute), true);
            if(attributes.Any()) {
                var attribute = (FirestorePropertyAttribute) attributes[0];
                var value = @this[attribute.PropertyName];
                if(value == null) {
                    Debug.WriteLine($"[Plugin.Firebase] Couldn't cast property '{attribute.PropertyName}' of '{targetType}' because it's not contained in the NSDictionary.");
                } else {
                    property.SetValue(instance, value.ToObject(property.PropertyType));
                }
            }

            var timestampAttributes = property.GetCustomAttributes(typeof(FirestoreServerTimestampAttribute), true);
            if(timestampAttributes.Any()) {
                var attribute = (FirestoreServerTimestampAttribute) timestampAttributes[0];
                var value = @this[attribute.PropertyName];
                if(value == null) {
                    Debug.WriteLine($"[Plugin.Firebase] Couldn't cast property '{attribute.PropertyName}' of '{targetType}' because value is null");
                } else if(property.PropertyType == typeof(DateTimeOffset) && value is Timestamp timestamp) {
                    property.SetValue(instance, timestamp.ToDateTimeOffset());
                } else {
                    Debug.WriteLine($"[Plugin.Firebase] Couldn't cast property '{attribute.PropertyName}' of '{targetType}' because properties that use the {nameof(FirestoreServerTimestampAttribute)} need to be of type {nameof(DateTimeOffset)} and value of type {nameof(Timestamp)}");
                }
            }
        }
        return instance;
    }

    public static object ToObject(this NSObject @this, Type targetType = null)
    {
        switch(@this) {
            case NSNumber x:
                return x.ToObject(targetType);
            case NSString x:
                return x.ToString();
            case NSDate x:
                return x.ToDateTimeOffset();
            case NSDictionary x:
                return x.ToDictionaryObject(targetType);
            case NSArray x:
                return x.ToList(GetGenericListType(targetType));
            case global::Firebase.CloudFirestore.GeoPoint x:
                return new GeoPoint(x.Latitude, x.Longitude);
            case Timestamp x:
                if(targetType == typeof(DateTime))
                    return x.ToDateTime();
                else
                    return x.ToDateTimeOffset();
            case DocumentReference x:
                return new DocumentReferenceWrapper(x);
            case NSNull x:
                return null;
            default:
                throw new ArgumentException($"Could not convert NSObject of type {@this.GetType()} to object");
        }
    }

    public static object ToObject(this NSNumber @this, Type targetType = null)
    {
        if(targetType == null) {
            return @this.Int32Value;
        }

        switch(Type.GetTypeCode(Nullable.GetUnderlyingType(targetType) ?? targetType)) {
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
            throw new ArgumentException($"Couldn't get generic list type of targetType {targetType}. Make sure to use a list IList<T> instead of an array T[] as type in your FirestoreObject.");
        }
        return genericType;
    }

    public static NSObject ToNSObject(this object @this)
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
                throw new ArgumentException($"Could not convert object of type {@this.GetType()} to NSObject. Does it extend {nameof(IFirestoreObject)}?");
        }
    }

    public static NSObject ToNSObject(this IFirestoreObject @this)
    {
        return @this.ToDictionary().ToNSObject();
    }
}
