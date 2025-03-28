using System.Collections;
using System.Text.Json;
using System.Text.Json.Nodes;
using Java.Util;
using IMap = Java.Util.IMap;
using IList = System.Collections.IList;
using NativeFieldValue = Firebase.Firestore.FieldValue;
using NativeGeoPoint = Firebase.Firestore.GeoPoint;

namespace Plugin.Firebase.Firestore.Platforms.Android.Extensions;

public static class DictionaryExtensions
{
    public static HashMap ToHashMap(this IDictionary<object, object> dictionary)
    {
        var map = new HashMap();
        dictionary.ToList().ForEach(x => map.Put(x.Key, x.Value));
        return map;
    }

    public static HashMap ToHashMap(this IDictionary<string, object> dictionary)
    {
        var hashMap = new HashMap();
        dictionary.ToList().ForEach(x => {
            if(x.Value is JsonObject jsonValue) {
                hashMap.Put(x.Key, jsonValue.Deserialize<Dictionary<string, object>>().ToHashMap());
            } else {
                hashMap.Put(x.Key, x.Value);
            }
        });
        return hashMap;
    }

    private static void Put(this IMap @this, object key, object value)
    {
        switch(value) {
            case bool x:
                @this.Put(key.ToString(), x);
                break;
            case char x:
                @this.Put(key.ToString(), x);
                break;
            case double x:
                @this.Put(key.ToString(), x);
                break;
            case float x:
                @this.Put(key.ToString(), x);
                break;
            case long x:
                @this.Put(key.ToString(), x);
                break;
            case int x:
                @this.Put(key.ToString(), x);
                break;
            case short x:
                @this.Put(key.ToString(), x);
                break;
            case string x:
                @this.Put(key.ToString(), x);
                break;
            case Enum x:
                @this.Put(key.ToString(), Convert.ToInt64(x));
                break;
            case IList x:
                @this.Put(key.ToString(), x.ToJavaList());
                break;
            case DateTime x:
                @this.Put(key.ToString(), x.ToJavaDate());
                break;
            case DateTimeOffset x:
                @this.Put(key.ToString(), x.ToJavaDate());
                break;
            case IFirestoreObject x:
                @this.Put(key.ToString(), x.ToJavaObject());
                break;
            case IDictionary x:
                @this.Put(key.ToString(), x.ToHashMapFromNonGenericDict());
                break;
            case FieldValue x:
                @this.Put(key.ToString(), x.ToNative());
                break;
            case IDocumentReference x:
                @this.Put(key.ToString(), x.ToJavaObject());
                break;
            case GeoPoint x:
                @this.Put(key.ToString(), new NativeGeoPoint(x.Latitude, x.Longitude));
                break;
            default:
                if(value == null) {
                    @this.Put(key.ToString(), null);
                    break;
                } else {
                    throw new ArgumentException($"Couldn't put object of type {value.GetType()} into {nameof(HashMap)}");
                }
        }
    }

    public static HashMap ToHashMapFromNonGenericDict(this IDictionary dictionary)
    {
        var hashMap = new HashMap();
        foreach(DictionaryEntry entry in dictionary) {
            hashMap.Put(entry.Key, entry.Value);
        }
        return hashMap;
    }

    public static HashMap ToHashMap(this object @this)
    {
        var map = new HashMap();
        var properties = @this.GetType().GetProperties();
        foreach(var property in properties) {
            var attributes = property.GetCustomAttributes(typeof(FirestorePropertyAttribute), true);
            if(attributes.Any()) {
                var attribute = (FirestorePropertyAttribute) attributes[0];
                map.Put(attribute.PropertyName, property.GetValue(@this));
            }

            var timestampAttributes = property.GetCustomAttributes(typeof(FirestoreServerTimestampAttribute), true);
            if(timestampAttributes.Any()) {
                var attribute = (FirestoreServerTimestampAttribute) timestampAttributes[0];
                map.Put(attribute.PropertyName, NativeFieldValue.ServerTimestamp());
            }
        }
        return map;
    }

    public static IDictionary<string, Java.Lang.Object> ToJavaObjectDictionary(this IEnumerable<(string, object)> tuples)
    {
        var dict = new Dictionary<string, object>();
        tuples.ToList().ForEach(x => dict.Add(x.Item1, x.Item2));
        return dict.ToJavaObjectDictionary();
    }

    public static IDictionary<string, Java.Lang.Object> ToJavaObjectDictionary(this IDictionary<string, object> dictionary)
    {
        var result = new Dictionary<string, Java.Lang.Object>();
        dictionary.ToList().ForEach(x => result.Add(x.Key, x.Value.ToJavaObject()));
        return result;
    }

    public static IDictionary<string, Java.Lang.Object> ToJavaObjectDictionary(this IDictionary<object, object> @this)
    {
        var dict = new Dictionary<string, object>();
        @this.ToList().ForEach(x => dict.Add(x.Key.ToString(), x.Value));
        return dict.ToJavaObjectDictionary();
    }
}