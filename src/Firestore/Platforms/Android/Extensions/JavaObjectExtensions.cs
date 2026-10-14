using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Java.Util;
using Android.Runtime;
using AndroidX.Collection;
using Firebase.Firestore;
using IList = System.Collections.IList;
using NativeFirebase = Firebase;
using System.Diagnostics;
using Plugin.Firebase.Firestore.Internal;

namespace Plugin.Firebase.Firestore.Platforms.Android.Extensions;

public static class JavaObjectExtensions
{
    public static T Cast<T>(this IDictionary<string, Java.Lang.Object> @this, string? documentId = null)
    {
        return (T) ((IDictionary) @this).Cast(typeof(T), documentId);
    }

    /// <summary>
    /// Converts a .NET value to the Java object the native Firestore SDK writes. Numbers and enums follow the
    /// write rules shared with iOS: integral values become <see cref="Java.Lang.Long"/> and <see cref="float"/>
    /// and <see cref="double"/> become <see cref="Java.Lang.Double"/>.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// The value has no Firestore representation, for example a <see cref="decimal"/>, a <see cref="char"/> or a
    /// <see cref="ulong"/> above <see cref="long.MaxValue"/>.
    /// </exception>
    public static Java.Lang.Object? ToJavaObject(this object? @this)
    {
        if(TryToJavaNumber(@this, out var number)) {
            return number;
        }

        switch(@this) {
            case null:
                return null;
            case string x:
                return x;
            case bool x:
                return x;
            case Java.Lang.ICharSequence x:
                return x.ToString();
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
            case IDictionary<string, object?> x:
                return x.ToHashMap();
            case IDictionary<object, object?> x:
                return x.ToHashMap();
            case IDictionary x:
                return x.ToHashMapFromNonGenericDict();
            case IList x:
                return x.ToJavaList();
            case DocumentReferenceWrapper x:
                return x.Wrapped;
            case GeoPoint x:
                return new global::Firebase.Firestore.GeoPoint(x.Latitude, x.Longitude);
            case IFirestoreObject x:
                return x.ToJavaObject();
            default:
                throw new ArgumentException($"Could not convert object of type {@this.GetType()} to Java.Lang.Object. Does it extend {nameof(IFirestoreObject)}?");
        }
    }

    /// <summary>
    /// Converts a number or enum with <see cref="FirestoreNumericWrites"/>. <see cref="DictionaryExtensions"/> uses
    /// this as well, so set and update writes store the same values.
    /// </summary>
    internal static bool TryToJavaNumber(object? value, [NotNullWhen(true)] out Java.Lang.Object? number)
    {
        if(!FirestoreNumericWrites.TryWiden(value, out var widened)) {
            number = null;
            return false;
        }

        number = widened is long int64 ? Java.Lang.Long.ValueOf(int64) : Java.Lang.Double.ValueOf((double) widened);
        return true;
    }

    public static Java.Lang.Object ToJavaObject(this IFirestoreObject @this)
    {
        var javaObject = @this.ToHashMap().ToJavaObject();
        if(javaObject is null) {
            throw new InvalidOperationException("Could not convert Firestore object to Java.Lang.Object.");
        }

        return javaObject;
    }

    public static object? ToObject(this Java.Lang.Object? @this, Type? targetType = null)
    {
        switch(@this) {
            case null:
                return null;
            case Java.Lang.ICharSequence x:
                return x.ToString();
            case Java.Lang.Boolean x:
                return x.BooleanValue().ConvertToTargetType(targetType);
            case Java.Lang.Integer x:
                return x.IntValue().ConvertToTargetType(targetType);
            case Java.Lang.Double x:
                return x.DoubleValue().ConvertToTargetType(targetType);
            case Java.Lang.Float x:
                return x.FloatValue().ConvertToTargetType(targetType);
            case Java.Lang.Long x:
                return x.LongValue().ConvertToTargetType(targetType);
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
                return targetType is null ? x.ToList() : x.ToList(GetGenericListType(targetType));
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

    private static object ToDictionaryObject(this IDictionary @this, Type? targetType)
    {
        if(targetType == null) {
            return @this.ToDictionary();
        } else if(targetType == typeof(object)) {
            return @this.ToDictionary(typeof(string), typeof(object));
        } else if(IsDictionaryType(targetType)) {
            var types = targetType.GenericTypeArguments;
            return @this.ToDictionary(types[0], types[1]);
        } else {
            return @this.Cast(targetType);
        }
    }

    public static IDictionary ToDictionary(this IDictionary @this, Type keyType, Type valueType)
    {
        var dict = Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(keyType, valueType)) as IDictionary;
        if(dict is null) {
            throw new InvalidOperationException("Could not create dictionary of type " + valueType);
        }

        foreach(var rawKey in @this.Keys) {
            if(rawKey is null) {
                throw new ArgumentException("Dictionary contains a null key.");
            }

            var key = ConvertToObject(keyType, rawKey);
            if(key is null) {
                throw new ArgumentException("Dictionary contains a null key.");
            }

            var value = ConvertToObject(valueType, @this[rawKey]);
            dict[key] = value;
        }
        return dict;
    }

    private static IDictionary<string, object?> ToDictionary(this IDictionary @this)
    {
        var dict = new Dictionary<string, object?>();
        foreach(var rawKey in @this.Keys) {
            if(rawKey is null) {
                throw new ArgumentException("Dictionary contains a null key.");
            }

            var key = rawKey.ToString();
            if(key is null) {
                throw new ArgumentException("Dictionary contains a null key.");
            }

            dict[key] = ConvertToObject(typeof(object), @this[rawKey]);
        }
        return dict;
    }

    private static object Cast(this IDictionary @this, Type targetType, string? documentId = null)
    {
        if(targetType == typeof(object) || IsDictionaryType(targetType)) {
            return @this.ToDictionaryObject(targetType);
        }

        var instance = Activator.CreateInstance(targetType);
        if(instance is null) {
            throw new InvalidOperationException("Could not create instance of type " + targetType);
        }

        var properties = targetType.GetProperties();
        foreach(var property in properties) {
            if(documentId != null && property.GetCustomAttributes(typeof(FirestoreDocumentIdAttribute), true).Any()) {
                property.SetValue(instance, documentId);
                continue;
            }

            var attributes = property.GetCustomAttributes(typeof(FirestorePropertyAttribute), true);
            if(attributes.Any()) {
                var attribute = (FirestorePropertyAttribute) attributes[0];
                if(!@this.Contains(attribute.PropertyName)) {
                    Debug.WriteLine($"[Plugin.Firebase] Couldn't cast property '{attribute.PropertyName}' of '{targetType}' because it's not contained in the dictionary.");
                    continue;
                }

                // values that aren't java peers arrive as CTS-mapped CLR types (e.g. long, double), so they still need
                // to be narrowed to the declared property type; sharing the dictionary path's conversion keeps both identical
                property.SetValue(instance, ConvertToObject(property.PropertyType, @this[attribute.PropertyName]));
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

    private static object? ConvertToObject(Type? targetType, object? value)
    {
        var conversionType = targetType.GetConversionType();

        if(value == null) {
            return null;
        } else if(conversionType == typeof(string)) {
            return value is Java.Lang.ICharSequence charSequence
                ? charSequence.ToString()
                : Convert.ToString(value, CultureInfo.InvariantCulture);
        } else if(value is IDictionary dictionary) {
            return dictionary.ToDictionaryObject(targetType);
        } else if(value is Java.Lang.Object javaValue) {
            return javaValue.ToObject(targetType);
        }

        return value.ConvertToTargetType(targetType);
    }

    private static bool IsDictionaryType(Type targetType)
    {
        return targetType.IsGenericType
               && (
                   targetType.GetGenericTypeDefinition() == typeof(IDictionary<,>)
                   || targetType.GetGenericTypeDefinition() == typeof(Dictionary<,>)
               );
    }

    public static IDictionary<string, object?> ToDictionary(this ArrayMap @this)
    {
        var dict = new Dictionary<string, object?>();
        var keys = @this.KeySet();
        if(keys is null) {
            throw new InvalidOperationException("Could not read dictionary keys.");
        }

        foreach(var key in keys) {
            var keyString = key.ToString();
            if(keyString is null) {
                throw new ArgumentException("Dictionary contains a null key.");
            }

            dict[keyString] = @this.Get(keyString).ToObject();
        }
        return dict;
    }

    private static Type GetGenericListType(Type? targetType)
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
}