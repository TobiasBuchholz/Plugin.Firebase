namespace Plugin.Firebase.Auth.Platforms.Android.Extensions;

public static class DictionaryExtensions
{
    public static IDictionary<string, object> ToDictionary(this IDictionary<string, Java.Lang.Object> @this)
    {
        var dict = new Dictionary<string, object>();
        foreach(var (key, value) in @this) {
            dict[key] = value.ToObject();
        }
        return dict;
    }
}