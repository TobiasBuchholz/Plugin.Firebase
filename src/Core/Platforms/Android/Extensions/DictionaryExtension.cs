using System;
using Android.OS;
using Android.Runtime;
using Plugin.Firebase.Android.Extensions;

namespace System.Collections.Generic;

public static class DictionaryExtension
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

    public static Bundle ToBundle(this IDictionary<string, string> dictionary)
    {
        var bundle = new Bundle();
        dictionary.ToList().ForEach(x => bundle.Put(x.Key, x.Value));
        return bundle;
    }

    public static IDictionary<string, string> ToDictionary(this Bundle bundle)
    {
        return bundle
            .KeySet()?
            .Select(x => (x, bundle.GetObject(x).ToString()))
            .ToDictionary(x => x.Item1, x => x.Item2);
    }

    private static object GetObject(this Bundle bundle, string key)
    {
        dynamic obj = bundle.GetString(key);
        if(obj != null) {
            return obj;
        }

        obj = bundle.GetStringArray(key);
        if(obj != null) {
            return obj;
        }

        obj = bundle.GetStringArrayList(key);
        if(obj != null) {
            return obj;
        }

        obj = bundle.GetInt(key, int.MaxValue);
        if(obj != int.MaxValue) {
            return obj;
        }

        obj = bundle.GetIntArray(key);
        if(obj != null) {
            return obj;
        }

        obj = bundle.GetIntegerArrayList(key);
        if(obj != null) {
            return obj;
        }

        obj = bundle.GetLong(key, long.MaxValue);
        if(obj != long.MaxValue) {
            return obj;
        }

        obj = bundle.GetLongArray(key);
        if(obj != null) {
            return obj;
        }

        obj = bundle.GetFloat(key, float.MaxValue);
        if(obj < float.MaxValue) {
            return obj;
        }

        obj = bundle.GetFloatArray(key);
        if(obj != null) {
            return obj;
        }

        obj = bundle.GetDouble(key, double.MaxValue);
        if(obj < double.MaxValue) {
            return obj;
        }

        obj = bundle.GetDoubleArray(key);
        if(obj != null) {
            return obj;
        }

        obj = bundle.GetShort(key, short.MaxValue);
        if(obj < short.MaxValue) {
            return obj;
        }

        obj = bundle.GetShortArray(key);
        if(obj != null) {
            return obj;
        }

        obj = bundle.GetChar(key, char.MaxValue);
        if(obj < char.MaxValue) {
            return obj;
        }

        obj = bundle.GetCharArray(key);
        if(obj != null) {
            return obj;
        }

        obj = bundle.GetCharSequence(key);
        if(obj != null) {
            return obj;
        }

        obj = bundle.GetCharSequenceArray(key);
        if(obj != null) {
            return obj;
        }

        obj = bundle.GetCharSequenceFormatted(key);
        if(obj != null) {
            return obj;
        }

        obj = bundle.GetCharSequenceArrayFormatted(key);
        if(obj != null) {
            return obj;
        }

        obj = bundle.GetCharSequenceArrayList(key);
        if(obj != null) {
            return obj;
        }

        obj = bundle.GetSize(key);
        if(obj != null) {
            return obj;
        }

        obj = bundle.GetSizeF(key);
        if(obj != null) {
            return obj;
        }

        // TODO: crashes like this
        // obj = bundle.GetByte(key, sbyte.MaxValue);
        // if(obj != sbyte.MaxValue) {
        //     return obj;
        // }

        obj = bundle.GetByteArray(key);
        if(obj != null) {
            return obj;
        }

        obj = bundle.GetBooleanArray(key);
        if(obj != null) {
            return obj;
        }

        obj = bundle.GetBundle(key);
        if(obj != null) {
            return obj;
        }

        obj = bundle.GetParcelable(key);
        if(obj != null) {
            return obj;
        }

        obj = bundle.GetParcelableArray(key);
        if(obj != null) {
            return obj;
        }

        obj = bundle.GetParcelableArrayList(key);
        if(obj != null) {
            return obj;
        }

        obj = bundle.GetSparseParcelableArray(key);
        if(obj != null) {
            return obj;
        }

        obj = bundle.GetSerializable(key);
        if(obj != null) {
            return obj;
        }

        return bundle.GetBoolean(key);
    }
    
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

    public static IDictionary<string, Java.Lang.Object> ToJavaObjectDictionary(this IDictionary<object, object> @this)
    {
        var dict = new Dictionary<string, object>();
        @this.ToList().ForEach(x => dict.Add(x.Key.ToString(), x.Value));
        return dict.ToJavaObjectDictionary();
    }
    
    public static JavaList ToJavaList(this IEnumerable @this)
    {
        var list = new JavaList();
        foreach(var item in @this) {
            list.Add(item.ToJavaObject());
        }
        return list;
    }
    
    // public static HashMap ToHashMap(this IDictionary<string, object> dictionary)
    // {
    //     var hashMap = new HashMap();
    //     dictionary.ToList().ForEach(x => {
    //         if(x.Value is JsonObject jsonValue) {
    //             hashMap.Put(x.Key, jsonValue.Deserialize<Dictionary<string, object>>().ToHashMap());
    //         } else {
    //             hashMap.Put(x.Key, x.Value);
    //         }
    //     });
    //     return hashMap;
    // }
}