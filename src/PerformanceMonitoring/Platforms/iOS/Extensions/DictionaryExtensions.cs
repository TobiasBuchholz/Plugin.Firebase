namespace Plugin.Firebase.PerformanceMonitoring.Platforms.iOS.Extensions;

internal static class DictionaryExtensions
{
    public static IReadOnlyDictionary<string, string> ToReadOnlyDictionary(
        this NSDictionary<NSString, NSString> @this
    )
    {
        var dict = new Dictionary<string, string>();
        if(@this == null) {
            return dict;
        }

        foreach(var (key, value) in @this) {
            dict[key.ToString()] = value.ToString();
        }
        return dict;
    }
}
