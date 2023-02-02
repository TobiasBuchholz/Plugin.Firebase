namespace Plugin.Firebase.Android.RemoteConfig.Extensions;

public static class DictionaryExtensions
{
    public static IDictionary<string, Java.Lang.Object> ToJavaObjectDictionary(this IDictionary<string, object> dictionary)
    {
        var result = new Dictionary<string, Java.Lang.Object>();
        dictionary.ToList().ForEach(x => result.Add(x.Key, x.Value.ToJavaObject()));
        return result;
    }
    
    public static IDictionary<string, Java.Lang.Object> ToJavaObjectDictionary(this IEnumerable<(string, object)> tuples)
    {
        var dict = new Dictionary<string, object>();
        tuples.ToList().ForEach(x => dict.Add(x.Item1, x.Item2));
        return dict.ToJavaObjectDictionary();
    }
}