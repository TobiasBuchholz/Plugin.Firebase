using System.Collections;
using NativeFieldValue = Firebase.CloudFirestore.FieldValue;

namespace Plugin.Firebase.Firestore.Platforms.iOS.Extensions;

/// <summary>
/// Extension methods for converting between .NET dictionaries and native iOS NSDictionary types.
/// </summary>
public static class DictionaryExtensions
{
    /// <summary>
    /// Converts a non-generic <see cref="IDictionary"/> to a native iOS NSDictionary.
    /// </summary>
    /// <param name="dictionary">The dictionary to convert.</param>
    /// <returns>A native iOS NSDictionary containing the converted key-value pairs.</returns>
    public static NSDictionary<NSString, NSObject> ToNSDictionaryFromNonGeneric(
        this IDictionary dictionary
    )
    {
        if(dictionary.Count > 0) {
            var nsDictionary = new NSMutableDictionary<NSString, NSObject>();

            foreach(DictionaryEntry entry in dictionary) {
                PutIntoNSDictionary(
                    new KeyValuePair<string, object?>(
                        entry.Key.ToString() ?? throw new ArgumentException("Dictionary contains a null key."),
                        entry.Value
                    ),
                    ref nsDictionary
                );
            }
            return NSDictionary<NSString, NSObject>.FromObjectsAndKeys(
                nsDictionary.Values.ToArray(),
                nsDictionary.Keys.ToArray(),
                (nint) nsDictionary.Count
            );
        } else {
            return new NSDictionary<NSString, NSObject>();
        }
    }

    private static void PutIntoNSDictionary(
        KeyValuePair<string, object?> pair,
        ref NSMutableDictionary<NSString, NSObject> nsDictionary
    )
    {
        nsDictionary.Add((NSString) pair.Key, pair.Value.ToNSObject());
    }

    /// <summary>
    /// Converts an object to a dictionary using Firestore property attributes.
    /// </summary>
    /// <param name="this">The object to convert.</param>
    /// <returns>A dictionary with property names as keys and their values.</returns>
    public static Dictionary<object, object?> ToDictionary(this object @this)
    {
        var dict = new Dictionary<object, object?>();
        var properties = @this.GetType().GetProperties();
        foreach(var property in properties) {
            var attributes = property.GetCustomAttributes(typeof(FirestorePropertyAttribute), true);
            if(attributes.Any()) {
                var attribute = (FirestorePropertyAttribute) attributes[0];
                var value = property.GetValue(@this);
                if(value is Enum) {
                    dict[attribute.PropertyName] = value;
                } else {
                    dict[attribute.PropertyName] = value.ToNSObject();
                }
            }

            var timestampAttributes = property.GetCustomAttributes(
                typeof(FirestoreServerTimestampAttribute),
                true
            );
            if(timestampAttributes.Any()) {
                var attribute = (FirestoreServerTimestampAttribute) timestampAttributes[0];
                dict[attribute.PropertyName] = NativeFieldValue.ServerTimestamp;
            }
        }
        return dict;
    }

    /// <summary>
    /// Converts a native iOS NSDictionary to a .NET object of the specified type.
    /// </summary>
    /// <param name="this">The NSDictionary to convert.</param>
    /// <param name="targetType">The target type to convert to.</param>
    /// <returns>The converted object.</returns>
    public static object? ToDictionaryObject(this NSDictionary @this, Type? targetType)
    {
        if(targetType == null) {
            return @this.ToDictionary();
        } else if(
              targetType.IsGenericType
              && (
                  targetType.GetGenericTypeDefinition() == typeof(IDictionary<,>)
                  || targetType.GetGenericTypeDefinition() == typeof(Dictionary<,>)
              )
          ) {
            var types = targetType.GenericTypeArguments;
            return @this.ToDictionary(types[0], types[1]);
        } else {
            return @this.Cast(targetType);
        }
    }

    /// <summary>
    /// Converts a native iOS NSDictionary to a typed .NET dictionary.
    /// </summary>
    /// <param name="this">The NSDictionary to convert.</param>
    /// <param name="keyType">The type for dictionary keys.</param>
    /// <param name="valueType">The type for dictionary values.</param>
    /// <returns>A typed dictionary containing the converted key-value pairs.</returns>
    public static IDictionary ToDictionary(this NSDictionary @this, Type keyType, Type valueType)
    {
        var dict =
            Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(keyType, valueType)) as IDictionary;
        if(dict is null) {
            throw new InvalidOperationException("Could not create dictionary of type " + valueType);
        }

        foreach(var pair in @this) {
            var key = pair.Key.ToObject(keyType);
            if(key is null) {
                throw new ArgumentException("Dictionary contains a null key.");
            }
            dict[key] = pair.Value.ToObject(valueType);
        }
        return dict;
    }

    /// <summary>
    /// Converts dictionary values to native iOS NSObject types.
    /// </summary>
    /// <param name="this">The dictionary to convert.</param>
    /// <returns>A dictionary with values converted to NSObject types.</returns>
    public static Dictionary<object, object> ToNSObjectDictionary(
        this Dictionary<object, object?> @this
    )
    {
        return @this.ToDictionary(x => x.Key, x => (object) x.Value.ToNSObject());
    }

    /// <summary>
    /// Converts a collection of key-value tuples to a dictionary with NSObject values.
    /// </summary>
    /// <param name="this">The collection of tuples to convert.</param>
    /// <returns>A dictionary with NSObject values.</returns>
    public static Dictionary<object, object> ToNSObjectDictionary(
        this IEnumerable<(string, object?)> @this
    )
    {
        var dict = new Dictionary<object, object>();
        foreach(var (key, value) in @this) {
            dict.Add(key, value.ToNSObject());
        }
        return dict;
    }
}