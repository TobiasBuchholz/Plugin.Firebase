using Foundation;

namespace Plugin.Firebase.iOS.Storage;

public static class DictionaryExtensions
{
    public static IDictionary<string, string> ToDictionary(this NSDictionary<NSString, NSString> @this)
    {
        var dict = new Dictionary<string, string>();
        foreach(var (key, value) in @this) {
            dict[key.ToString()] = new NSString(value.ToString());
        }
        return dict;
    }
    
    public static NSDictionary<NSString, NSString> ToNSDictionary(this IDictionary<string, string> @this)
    {
        return NSDictionary<NSString, NSString>.FromObjectsAndKeys(@this.Values.ToArray(), @this.Keys.ToArray());
    }
}