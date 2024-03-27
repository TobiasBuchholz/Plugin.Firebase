using System.Collections;
using Java.Util;
using Android.Runtime;
using AndroidX.Collection;
using Firebase.Firestore;
using IList = System.Collections.IList;
using NativeFirebase = Firebase;
using System.Diagnostics;

namespace Plugin.Firebase.Firestore.Platforms.Android.Extensions;

public static class JavaObjectExtensions
{
    public static T Cast<T>(this IDictionary<string, Java.Lang.Object> @this, string documentId = null)
    {
        return (T) ((IDictionary) @this).Cast(typeof(T), documentId);
    }

    public static Java.Lang.Object ToJavaObject(this object @this)
    {
        switch(@this) {
            case string x:
                return x;
            case int x:
                return x;
            case long x:
                return x;
            case float x:
                return x;
            case double x:
                return x;
            case bool x:
                return x;
            case DateTime x:
                return x.ToJavaDate();
            case DateTimeOffset x:
                return x.ToJavaDate();
            case FieldValue x:
                return x.ToNative();
            case JavaDictionary x:
                return x;
            case HashMap x:
                return x;
            case IList x:
                return x.ToJavaList();
            case IDictionary<string, object> x:
                return x.ToHashMap();
            case DocumentReferenceWrapper x:
                return x.Wrapped;
            case IFirestoreObject x:
                return x.ToJavaObject();
            default:
                if(@this is Enum) {
                    var enumType = Enum.GetUnderlyingType(@this.GetType());
                    if(enumType == typeof(int)) {
                        return Convert.ToInt32(@this);
                    } else if(enumType == typeof(uint)) {
                        return (int) Convert.ToUInt32(@this);
                    } else if(enumType == typeof(long)) {
                        return Convert.ToInt64(@this);
                    } else if(enumType == typeof(ulong)) {
                        return (long) Convert.ToUInt64(@this);
                    } else if(enumType == typeof(sbyte)) {
                        return Convert.ToSByte(@this);
                    } else if(enumType == typeof(byte)) {
                        return (sbyte) Convert.ToByte(@this);
                    } else if(enumType == typeof(short)) {
                        return Convert.ToInt16(@this);
                    } else if(enumType == typeof(ushort)) {
                        return (short) Convert.ToUInt16(@this);
                    }
                }
                throw new ArgumentException($"Could not convert object of type {@this.GetType()} to Java.Lang.Object. Does it extend {nameof(IFirestoreObject)}?");
        }
    }

    public static Java.Lang.Object ToJavaObject(this IFirestoreObject @this)
    {
        return @this.ToHashMap().ToJavaObject();
    }

    public static object ToObject(this Java.Lang.Object @this, Type targetType = null)
    {
        switch(@this) {
            case Java.Lang.ICharSequence x:
                return x.ToString();
            case Java.Lang.Boolean x:
                return x.BooleanValue();
            case Java.Lang.Integer x:
                return x.IntValue();
            case Java.Lang.Double x:
                return x.DoubleValue();
            case Java.Lang.Float x:
                return x.FloatValue();
            case Java.Lang.Long x:
                return x.LongValue();
            case Date x:
                return x.ToDateTimeOffset();
            case NativeFirebase.Timestamp x:
                if(targetType == typeof(DateTime))
                    return x.ToDate().ToDateTime();
                else
                    return x.ToDate().ToDateTimeOffset();
            case IDictionary x:
                return x.ToDictionaryObject(targetType);
            case JavaList x:
                return x.ToList(targetType?.GenericTypeArguments[0]);
            case global::Firebase.Firestore.GeoPoint x:
                return new GeoPoint(x.Latitude, x.Longitude);
            case DocumentReference x:
                return new DocumentReferenceWrapper(x);
            case ArrayMap x:
                return x.ToDictionary();
            default:
                throw new ArgumentException($"Could not convert Java.Lang.Object of type {@this.GetType()} to object");
        }
    }

    private static object ToDictionaryObject(this IDictionary @this, Type targetType)
    {
        if(targetType != null && targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Dictionary<,>)) {
            var types = targetType.GenericTypeArguments;
            return @this.ToDictionary(types[0], types[1]);
        } else {
            return @this.Cast(targetType);
        }
    }

    public static IDictionary ToDictionary(this IDictionary @this, Type keyType, Type valueType)
    {
        var dict = (IDictionary) Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(keyType, valueType));
        foreach(DictionaryEntry pair in @this) {
            var key = pair.Key.ToJavaObject().ToObject(keyType);
            var value = pair.Value.ToJavaObject().ToObject(valueType);
            dict[key] = value;
        }
        return dict;
    }

    private static object Cast(this IDictionary @this, Type targetType, string documentId = null)
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
                    Debug.WriteLine($"[Plugin.Firebase] Couldn't cast property '{attribute.PropertyName}' of '{targetType}' because it's not contained in the dictionary.");
                } else if(value is Java.Lang.Object javaValue) {
                    property.SetValue(instance, javaValue.ToObject(property.PropertyType));
                } else if(property.PropertyType == typeof(float)) {
                    property.SetValue(instance, Convert.ToSingle(value));
                } else if((Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType) == typeof(int)) {
                    property.SetValue(instance, Convert.ToInt32(value));
                } else {
                    property.SetValue(instance, value);
                }
            }

            var timestampAttributes = property.GetCustomAttributes(typeof(FirestoreServerTimestampAttribute), true);
            if(timestampAttributes.Any()) {
                var attribute = (FirestoreServerTimestampAttribute) timestampAttributes[0];
                var value = @this[attribute.PropertyName];
                if(value == null) {
                    Debug.WriteLine($"[Plugin.Firebase] Couldn't cast property '{attribute.PropertyName}' of '{targetType}' because value is null");
                } else if(property.PropertyType == typeof(DateTimeOffset) && value is NativeFirebase.Timestamp timestamp) {
                    property.SetValue(instance, timestamp.ToDate().ToDateTimeOffset());
                } else {
                    Debug.WriteLine($"[Plugin.Firebase] Couldn't cast property '{attribute.PropertyName}' of '{targetType}' because properties that use the {nameof(FirestoreServerTimestampAttribute)} need to be of type {nameof(DateTimeOffset)} and value of type {nameof(NativeFirebase.Timestamp)}");
                }
            }
        }
        return instance;
    }

    public static IDictionary<string, object> ToDictionary(this ArrayMap @this)
    {
        var dict = new Dictionary<string, object>();
        var keys = @this.KeySet()!;
        foreach(var key in keys) {
            dict[key.ToString()] = @this.Get(key.ToString()).ToObject();
        }
        return dict;
    }
}
