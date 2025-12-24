using System.Collections;

namespace Plugin.Firebase.Auth.Platforms.iOS.Extensions;

public static class DictionaryExtensions
{
    public static IDictionary ToDictionary(this NSDictionary @this, Type keyType, Type valueType)
    {
        var dict = (IDictionary) Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(keyType, valueType));
        foreach(var pair in @this) {
            dict[pair.Key.ToObject(keyType)] = pair.Value.ToObject(valueType);
        }
        return dict;
    }

    public static IDictionary<string, object> ToDictionary(this NSDictionary<NSString, NSObject> @this)
    {
        var dict = new Dictionary<string, object>();
        foreach(var (key, value) in @this) {
            dict[key.ToString()] = value.ToObject();
        }
        return dict;
    }
}