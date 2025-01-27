using System.Collections;
using NativeFieldValue = Firebase.CloudFirestore.FieldValue;

namespace Plugin.Firebase.Firestore.Platforms.iOS.Extensions;

public static class DictionaryExtensions
{
    public static NSDictionary<NSString, NSObject> ToNSDictionaryFromNonGeneric(this IDictionary dictionary)
    {
        if(dictionary.Count > 0) {
            var nsDictionary = new NSMutableDictionary<NSString, NSObject>();

            foreach(DictionaryEntry entry in dictionary) {
                PutIntoNSDictionary(new KeyValuePair<string, object>(entry.Key.ToString(), entry.Value), ref nsDictionary);
            }
            return NSDictionary<NSString, NSObject>.FromObjectsAndKeys(
                nsDictionary.Values.ToArray(),
                nsDictionary.Keys.ToArray(),
                (nint) nsDictionary.Count);
        } else {
            return new NSDictionary<NSString, NSObject>();
        }
    }

    private static void PutIntoNSDictionary(KeyValuePair<string, object> pair, ref NSMutableDictionary<NSString, NSObject> nsDictionary)
    {
        switch(pair.Value) {
            case bool x:
                nsDictionary.Add((NSString) pair.Key, new NSNumber(x));
                break;
            case double x:
                nsDictionary.Add((NSString) pair.Key, new NSNumber(x));
                break;
            case float x:
                nsDictionary.Add((NSString) pair.Key, new NSNumber(x));
                break;
            case long x:
                nsDictionary.Add((NSString) pair.Key, new NSNumber(x));
                break;
            case int x:
                nsDictionary.Add((NSString) pair.Key, new NSNumber(x));
                break;
            case short x:
                nsDictionary.Add((NSString) pair.Key, new NSNumber(x));
                break;
            case string x:
                nsDictionary.Add((NSString) pair.Key, new NSString(x));
                break;
            case NSObject x:
                nsDictionary.Add((NSString) pair.Key, x);
                break;
            default:
                if(pair.Value is Enum @enum) {
                    nsDictionary.Add((NSString) pair.Key, new NSNumber(Convert.ToInt64(@enum)));
                    break;
                } else if(pair.Value == null) {
                    nsDictionary.Add((NSString) pair.Key, new NSNull());
                    break;
                } else {
                    throw new ArgumentException($"Couldn't put object of type {pair.Value.GetType()} into NSDictionary");
                }
        }
    }

    public static Dictionary<object, object> ToDictionary(this object @this)
    {
        var dict = new Dictionary<object, object>();
        var properties = @this.GetType().GetProperties();
        foreach(var property in properties) {
            var attributes = property.GetCustomAttributes(typeof(FirestorePropertyAttribute), true);
            if(attributes.Any()) {
                var attribute = (FirestorePropertyAttribute) attributes[0];
                var value = property.GetValue(@this);
                if(value is Enum) {
                    dict[attribute.PropertyName] = value;
                } else if(value != null) {
                    dict[attribute.PropertyName] = value.ToNSObject();
                }
            }

            var timestampAttributes = property.GetCustomAttributes(typeof(FirestoreServerTimestampAttribute), true);
            if(timestampAttributes.Any()) {
                var attribute = (FirestoreServerTimestampAttribute) timestampAttributes[0];
                dict[attribute.PropertyName] = NativeFieldValue.ServerTimestamp;
            }
        }
        return dict;
    }

    public static object ToDictionaryObject(this NSDictionary @this, Type targetType)
    {
        if(targetType == null) {
            return @this.ToDictionary();
        } else if(targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Dictionary<,>)) {
            var types = targetType.GenericTypeArguments;
            return @this.ToDictionary(types[0], types[1]);
        } else {
            return @this.Cast(targetType);
        }
    }

    public static IDictionary ToDictionary(this NSDictionary @this, Type keyType, Type valueType)
    {
        var dict = (IDictionary) Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(keyType, valueType));
        foreach(var pair in @this) {
            dict[pair.Key.ToObject(keyType)] = pair.Value.ToObject(valueType);
        }
        return dict;
    }

    public static Dictionary<object, object> ToNSObjectDictionary(this Dictionary<object, object> @this)
    {
        return @this.ToDictionary(x => x.Key, x => (object) x.Value.ToNSObject());
    }

    public static Dictionary<object, object> ToNSObjectDictionary(this IEnumerable<(string, object)> @this)
    {
        var dict = new Dictionary<object, object>();
        foreach(var (key, value) in @this) {
            dict.Add(key, value.ToNSObject());
        }
        return dict;
    }
}