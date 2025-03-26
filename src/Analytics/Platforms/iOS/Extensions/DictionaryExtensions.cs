using System.Collections;
using Foundation;

namespace Plugin.Firebase.Analytics.Platforms.iOS.Extensions;

public static class DictionaryExtensions
{
    public static NSDictionary<NSString, NSObject> ToNSDictionary(this IDictionary<string, object> dictionary)
    {
        return ((IDictionary) dictionary).ToNSDictionaryFromNonGeneric();
    }

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
            case IDictionary<string, object> x:
                nsDictionary.Add((NSString) pair.Key, x.ToNSDictionary());
                break;
            case IEnumerable<IDictionary<string, object>> x:
                nsDictionary.Add((NSString) pair.Key, NSArray.FromObjects(x.Select(d => d.ToNSDictionary()).ToArray()));
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
}