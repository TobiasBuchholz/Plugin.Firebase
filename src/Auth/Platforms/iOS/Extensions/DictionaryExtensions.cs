using System.Collections;

namespace Plugin.Firebase.Auth.Platforms.iOS.Extensions;

/// <summary>
/// Provides extension methods for converting native iOS NSDictionary types to .NET dictionaries.
/// </summary>
public static class DictionaryExtensions
{
    /// <summary>
    /// Converts a native NSDictionary to a .NET IDictionary with the specified key and value types.
    /// </summary>
    /// <param name="this">The NSDictionary to convert.</param>
    /// <param name="keyType">The target type for dictionary keys.</param>
    /// <param name="valueType">The target type for dictionary values.</param>
    /// <returns>A .NET dictionary containing the converted key-value pairs.</returns>
    public static IDictionary ToDictionary(this NSDictionary @this, Type keyType, Type valueType)
    {
        var dict = (IDictionary)
            Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(keyType, valueType));
        foreach(var pair in @this) {
            dict[pair.Key.ToObject(keyType)] = pair.Value.ToObject(valueType);
        }
        return dict;
    }

    /// <summary>
    /// Converts a typed NSDictionary to a .NET dictionary with string keys and object values.
    /// </summary>
    /// <param name="this">The typed NSDictionary to convert.</param>
    /// <returns>A .NET dictionary containing the converted key-value pairs.</returns>
    public static IDictionary<string, object> ToDictionary(
        this NSDictionary<NSString, NSObject> @this
    )
    {
        var dict = new Dictionary<string, object>();
        foreach(var (key, value) in @this) {
            dict[key.ToString()] = value.ToObject();
        }
        return dict;
    }
}