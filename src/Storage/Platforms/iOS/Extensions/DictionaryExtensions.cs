namespace Plugin.Firebase.Storage.Platforms.iOS.Extensions;

/// <summary>
/// Provides extension methods for converting between .NET dictionaries and native iOS NSDictionary types for Firebase Storage.
/// </summary>
public static class DictionaryExtensions
{
    /// <summary>
    /// Converts a native NSDictionary to a .NET dictionary.
    /// </summary>
    /// <param name="this">The NSDictionary to convert.</param>
    /// <returns>A .NET dictionary containing the key-value pairs.</returns>
    public static IDictionary<string, string> ToDictionary(
        this NSDictionary<NSString, NSString> @this
    )
    {
        var dict = new Dictionary<string, string>();
        foreach(var (key, value) in @this) {
            dict[key.ToString()] = new NSString(value.ToString());
        }
        return dict;
    }

    /// <summary>
    /// Converts a .NET dictionary to a native NSDictionary.
    /// </summary>
    /// <param name="this">The dictionary to convert.</param>
    /// <returns>An NSDictionary containing the key-value pairs.</returns>
    public static NSDictionary<NSString, NSString> ToNSDictionary(
        this IDictionary<string, string> @this
    )
    {
        return NSDictionary<NSString, NSString>.FromObjectsAndKeys(
            @this.Values.ToArray<object>(),
            @this.Keys.ToArray<object>()
        );
    }
}