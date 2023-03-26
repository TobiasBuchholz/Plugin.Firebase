using Android.OS;

namespace Plugin.Firebase.Analytics.Platforms.Android.Extensions;

public static class DictionaryExtensions
{
    public static Bundle ToBundle(this IDictionary<string, object> dictionary)
    {
        var bundle = new Bundle();
        dictionary.ToList().ForEach(x => bundle.Put(x.Key, x.Value));
        return bundle;
    }

    private static void Put(this Bundle @this, string key, object value)
    {
        switch(value) {
            case bool x:
                @this.PutBoolean(key, x);
                break;
            case char x:
                @this.PutChar(key, x);
                break;
            case double x:
                @this.PutDouble(key, x);
                break;
            case float x:
                @this.PutFloat(key, x);
                break;
            case long x:
                @this.PutLong(key, x);
                break;
            case int x:
                @this.PutInt(key, x);
                break;
            case short x:
                @this.PutShort(key, x);
                break;
            case string x:
                @this.PutString(key, x);
                break;
            default:
                if(value == null) {
                    @this.PutString(key, null);
                    break;
                } else {
                    throw new ArgumentException($"Couldn't put object of type {value.GetType()} into {nameof(Bundle)}");
                }
        }
    }
}