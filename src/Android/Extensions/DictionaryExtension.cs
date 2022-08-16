using System;
using System.Linq;
using Android.OS;
using Android.Runtime;
using AndroidX.Collection;
using Java.Util;
using Newtonsoft.Json.Linq;
using Plugin.Firebase.Android.Extensions;
using Plugin.Firebase.Common;
using Plugin.Firebase.Firestore;
using Plugin.Firebase.Android.Firestore;
using NativeFieldValue = Firebase.Firestore.FieldValue;

namespace System.Collections.Generic
{
    public static class DictionaryExtension
    {
        public static HashMap ToHashMap(this IDictionary<object, object> dictionary)
        {
            var map = new HashMap();
            dictionary.ToList().ForEach(x => map.Put(x.Key, x.Value));
            return map;
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
                case DateTimeOffset x:
                    @this.Put(key.ToString(), x.ToJavaDate());
                    break;
                case IFirestoreObject x:
                    @this.Put(key.ToString(), x.ToJavaObject());
                    break;
                case IDictionary x:
                    @this.Put(key.ToString(), x.ToHashMapFromNonGenericDict());
                    break;
                case IDocumentReference x:
                    @this.Put(key.ToString(), x.ToJavaObject());
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

        public static JavaList ToJavaList(this IList @this)
        {
            var list = new JavaList();
            foreach(var item in @this) {
                list.Add(item.ToJavaObject());
            }
            return list;
        }

        public static HashMap ToHashMap(this IEnumerable<(object, object)> @this)
        {
            var map = new HashMap();
            @this.ToList().ForEach(x => map.Put(x.Item1, x.Item2));
            return map;
        }

        public static HashMap ToHashMap(this IDictionary<string, object> dictionary)
        {
            var hashMap = new HashMap();
            dictionary.ToList().ForEach(x => {
                if(x.Value is JObject jsonValue) {
                    hashMap.Put(x.Key, jsonValue.ToObject<Dictionary<string, object>>().ToHashMap());
                } else {
                    hashMap.Put(x.Key, x.Value);
                }
            });
            return hashMap;
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

        public static IDictionary<string, Java.Lang.Object> ToJavaObjectDictionary(this IDictionary<string, object> dictionary)
        {
            var result = new Dictionary<string, Java.Lang.Object>();
            dictionary.ToList().ForEach(x => PutIntoJavaObjectDictionary(x, ref result));
            return result;
        }

        private static void PutIntoJavaObjectDictionary(KeyValuePair<string, object> pair, ref Dictionary<string, Java.Lang.Object> dictionary)
        {
            switch(pair.Value) {
                case bool x:
                    dictionary.Add(pair.Key, x);
                    break;
                case char x:
                    dictionary.Add(pair.Key, x);
                    break;
                case double x:
                    dictionary.Add(pair.Key, x);
                    break;
                case float x:
                    dictionary.Add(pair.Key, x);
                    break;
                case long x:
                    dictionary.Add(pair.Key, x);
                    break;
                case int x:
                    dictionary.Add(pair.Key, x);
                    break;
                case short x:
                    dictionary.Add(pair.Key, x);
                    break;
                case string x:
                    dictionary.Add(pair.Key, x);
                    break;
                case FieldValue x:
                    dictionary.Add(pair.Key, x.ToNative());
                    break;
                default:
                    if(pair.Value == null) {
                        dictionary.Add(pair.Key, null);
                        break;
                    } else {
                        throw new ArgumentException($"Couldn't put object of type {pair.Value.GetType()} into Java.Lang.Object dictionary");
                    }
            }
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

        public static IDictionary ToDictionary(this IDictionary @this, Type keyType, Type valueType)
        {
            var dict = (IDictionary) Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(keyType, valueType));
            foreach(DictionaryEntry pair in @this) {
                var key = pair.Key.ToJavaObject().ToObject(keyType);
                var value = pair.Value.ToJavaObject().ToObject(valueType);
                dict[key] = value;
            }
            return dict;
        }

        public static IDictionary<string, object> ToDictionary(this IDictionary<string, Java.Lang.Object> @this)
        {
            var dict = new Dictionary<string, object>();
            foreach(var (key, value) in @this) {
                dict[key] = value.ToObject();
            }
            return dict;
        }

        public static IDictionary<string, object> ToDictionary(this ArrayMap @this)
        {
            var dict = new Dictionary<string, object>();
            var keys = @this.KeySet()!;
            foreach(var key in keys) {
                dict[key.ToString()] = @this.Get(key.ToString()).ToObject();
            }
            return dict;
        }
    }
}