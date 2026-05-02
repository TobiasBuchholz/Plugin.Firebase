using System.Collections;
using AndroidX.Collection;
using Java.Util;

namespace Plugin.Firebase.Auth.Platforms.Android.Extensions;

public static class DictionaryExtensions
{
    public static IDictionary<string, object> ToDictionary(this IDictionary<string, Java.Lang.Object> @this)
    {
        var dict = new Dictionary<string, object>();
        foreach(var (key, value) in @this) {
            dict[key] = ConvertToObject(typeof(object), value)!;
        }
        return dict;
    }

    internal static object ToDictionaryObject(this IDictionary @this, Type? targetType)
    {
        if(targetType == null || targetType == typeof(object)) {
            return @this.ToDictionary();
        }

        if(targetType.IsGenericType && (
            targetType.GetGenericTypeDefinition() == typeof(IDictionary<,>) ||
            targetType.GetGenericTypeDefinition() == typeof(Dictionary<,>)
        )) {
            var types = targetType.GenericTypeArguments;
            return @this.ToDictionary(types[0], types[1]);
        }

        return @this.ToDictionary();
    }

    internal static object ToDictionaryObject(this IMap @this, Type? targetType)
    {
        if(targetType == null || targetType == typeof(object)) {
            return @this.ToDictionary();
        }

        if(targetType.IsGenericType && (
            targetType.GetGenericTypeDefinition() == typeof(IDictionary<,>) ||
            targetType.GetGenericTypeDefinition() == typeof(Dictionary<,>)
        )) {
            var types = targetType.GenericTypeArguments;
            return @this.ToDictionary(types[0], types[1]);
        }

        return @this.ToDictionary();
    }

    internal static object ToDictionaryObject(this ArrayMap @this, Type? targetType)
    {
        if(targetType == null || targetType == typeof(object)) {
            return @this.ToDictionary();
        }

        if(targetType.IsGenericType && (
            targetType.GetGenericTypeDefinition() == typeof(IDictionary<,>) ||
            targetType.GetGenericTypeDefinition() == typeof(Dictionary<,>)
        )) {
            var types = targetType.GenericTypeArguments;
            return @this.ToDictionary(types[0], types[1]);
        }

        return @this.ToDictionary();
    }

    public static IDictionary ToDictionary(this IDictionary @this, Type keyType, Type valueType)
    {
        var dict = (IDictionary)
            Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(keyType, valueType))!;
        foreach(DictionaryEntry pair in @this) {
            dict![ConvertToObject(keyType, pair.Key)!] = ConvertToObject(valueType, pair.Value);
        }
        return dict!;
    }

    public static IDictionary<string, object> ToDictionary(this IDictionary @this)
    {
        var dict = new Dictionary<string, object>();
        foreach(DictionaryEntry pair in @this) {
            var key = pair.Key?.ToString();
            if(key is null) {
                throw new ArgumentException("Dictionary contains a null key.");
            }

            dict[key] = ConvertToObject(typeof(object), pair.Value)!;
        }
        return dict;
    }

    public static IDictionary ToDictionary(this IMap @this, Type keyType, Type valueType)
    {
        var dict = (IDictionary)
            Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(keyType, valueType))!;
        foreach(var key in @this.KeySet()!) {
            var javaKey = ConvertToJavaMapKey(key);
            dict![ConvertToObject(keyType, key)!] = ConvertToObject(
                valueType,
                @this.Get(javaKey)
            );
        }
        return dict!;
    }

    public static IDictionary<string, object> ToDictionary(this IMap @this)
    {
        var dict = new Dictionary<string, object>();
        foreach(var key in @this.KeySet()!) {
            var javaKey = ConvertToJavaMapKey(key);
            var keyString = key?.ToString();
            if(keyString is null) {
                throw new ArgumentException("Dictionary contains a null key.");
            }

            dict[keyString] = ConvertToObject(typeof(object), @this.Get(javaKey))!;
        }
        return dict;
    }

    public static IDictionary ToDictionary(this ArrayMap @this, Type keyType, Type valueType)
    {
        var dict = (IDictionary)
            Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(keyType, valueType))!;
        foreach(var key in @this.KeySet()!) {
            var javaKey = ConvertToJavaMapKey(key);
            dict![ConvertToObject(keyType, key)!] = ConvertToObject(
                valueType,
                @this.Get(javaKey)
            );
        }
        return dict!;
    }

    public static IDictionary<string, object> ToDictionary(this ArrayMap @this)
    {
        var dict = new Dictionary<string, object>();
        foreach(var key in @this.KeySet()!) {
            var javaKey = ConvertToJavaMapKey(key);
            var keyString = key?.ToString();
            if(keyString is null) {
                throw new ArgumentException("Dictionary contains a null key.");
            }

            dict[keyString] = ConvertToObject(typeof(object), @this.Get(javaKey))!;
        }
        return dict;
    }

    private static object? ConvertToObject(Type targetType, object? value)
    {
        if(value is null) {
            return null;
        }

        if(targetType == typeof(string)) {
            return value is Java.Lang.ICharSequence charSequence
                ? charSequence.ToString()
                : value.ToString();
        }

        if(value is Java.Lang.Object javaValue) {
            return javaValue.ToObject(targetType);
        }

        return targetType == typeof(object)
            ? value
            : Convert.ChangeType(value, Nullable.GetUnderlyingType(targetType) ?? targetType);
    }

    private static Java.Lang.Object ConvertToJavaMapKey(object? key)
    {
        if(key is null) {
            throw new ArgumentException("Dictionary contains a null key.");
        }

        if(key is Java.Lang.Object javaKey) {
            return javaKey;
        }

        return key.ToJavaObject()
            ?? throw new ArgumentException("Dictionary contains a null key.");
    }
}