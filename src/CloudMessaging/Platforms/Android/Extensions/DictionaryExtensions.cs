using Android.OS;

namespace Plugin.Firebase.CloudMessaging.Platforms.Android.Extensions;

public static class DictionaryExtensions
{
    public static Bundle ToBundle(this IDictionary<string, string> dictionary)
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

    public static IDictionary<string, string> ToDictionary(this Bundle bundle)
    {
        return bundle
            .KeySet()?
            .Select(x => (x, bundle.GetObject(x).ToString()))
            .ToDictionary(x => x.Item1, x => x.Item2);
    }

    private static object GetObject(this Bundle bundle, string key)
    {
        if(bundle.GetString(key) is { } str) {
            return str;
        }

        if(bundle.GetStringArray(key) is { } strArray) {
            return strArray;
        }

        if(bundle.GetStringArrayList(key) is { } strArrayList) {
            return strArrayList;
        }

        var i = bundle.GetInt(key, int.MaxValue);
        if(i != int.MaxValue) {
            return i;
        }

        if(bundle.GetIntArray(key) is { } intArray) {
            return intArray;
        }

        if(bundle.GetIntegerArrayList(key) is { } intArrayList) {
            return intArrayList;
        }

        var l = bundle.GetLong(key, long.MaxValue);
        if(l != long.MaxValue) {
            return l;
        }

        if(bundle.GetLongArray(key) is { } longArray) {
            return longArray;
        }

        var f = bundle.GetFloat(key, float.MaxValue);
        if(f < float.MaxValue) {
            return f;
        }

        if(bundle.GetFloatArray(key) is { } floatArray) {
            return floatArray;
        }

        var d = bundle.GetDouble(key, double.MaxValue);
        if(d < double.MaxValue) {
            return d;
        }

        if(bundle.GetDoubleArray(key) is { } doubleArray) {
            return doubleArray;
        }

        var sh = bundle.GetShort(key, short.MaxValue);
        if(sh < short.MaxValue) {
            return sh;
        }

        if(bundle.GetShortArray(key) is { } shortArray) {
            return shortArray;
        }

        var c = bundle.GetChar(key, char.MaxValue);
        if(c < char.MaxValue) {
            return c;
        }

        if(bundle.GetCharArray(key) is { } charArray) {
            return charArray;
        }

        if(bundle.GetCharSequence(key) is { } charSequence) {
            return charSequence;
        }

        if(bundle.GetCharSequenceArray(key) is { } charSequenceArray) {
            return charSequenceArray;
        }

        if(bundle.GetCharSequenceFormatted(key) is { } charSequenceFormatted) {
            return charSequenceFormatted;
        }

        if(bundle.GetCharSequenceArrayFormatted(key) is { } charSequenceArrayFormatted) {
            return charSequenceArrayFormatted;
        }

        if(bundle.GetCharSequenceArrayList(key) is { } charSequenceArrayList) {
            return charSequenceArrayList;
        }

        if(bundle.GetSize(key) is { } size) {
            return size;
        }

        if(bundle.GetSizeF(key) is { } sizeF) {
            return sizeF;
        }

        // TODO: crashes like this
        // var b = bundle.GetByte(key, sbyte.MaxValue);
        // if(b != sbyte.MaxValue) {
        //     return b;
        // }

        if(bundle.GetByteArray(key) is { } byteArray) {
            return byteArray;
        }

        if(bundle.GetBooleanArray(key) is { } booleanArray) {
            return booleanArray;
        }

        if(bundle.GetBundle(key) is { } bdl) {
            return bdl;
        }

        if(bundle.GetParcelable(key) is { } parcelable) {
            return parcelable;
        }

        if(bundle.GetParcelableArray(key) is { } parcelableArray) {
            return parcelableArray;
        }

        if(bundle.GetParcelableArrayList(key) is { } parcelableArrayList) {
            return parcelableArrayList;
        }

        if(bundle.GetSparseParcelableArray(key) is { } sparseParcelableArray) {
            return sparseParcelableArray;
        }

        if(bundle.GetSerializable(key) is { } serializable) {
            return serializable;
        }

        return bundle.GetBoolean(key);
    }
}
