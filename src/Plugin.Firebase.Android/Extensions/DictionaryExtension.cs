using System.Linq;
using Android.OS;
using Java.Util;
using Plugin.Firebase.Abstractions.Firestore;
using Plugin.Firebase.Android.Extensions;

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
                case DateTime x:
                    @this.Put(key.ToString(), x.ToJavaDate());
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

        public static HashMap ToHashMap(this IEnumerable<(object, object)> @this)
        {
            var map = new HashMap();
            @this.ToList().ForEach(x => map.Put(x.Item1, x.Item2));
            return map;
        }

        public static HashMap ToHashMap(this object @this)
        {
            var map = new HashMap();
            var properties = @this.GetType().GetProperties();
            foreach(var property in properties) {
                var attribute = (FirestorePropertyAttribute) property.GetCustomAttributes(typeof(FirestorePropertyAttribute), true)[0];
                map.Put(attribute.PropertyName, property.GetValue(@this));
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
                .KeySet()
                .Select(x => (x, bundle.GetObject(x).ToString()))
                .ToDictionary(x => x.Item1, x => x.Item2);
        }

        private static object GetObject(this Bundle bundle, string key)
        {
            dynamic obj = bundle.GetString(key);
            if(obj != null) {
                return obj;
            }

            obj = bundle.GetBundle(key);
            if(obj != null) {
                return obj;
            }

            obj = bundle.GetInt(key, int.MaxValue);
            if(obj != int.MaxValue) {
                return obj;
            }

            obj = bundle.GetLong(key, long.MaxValue);
            if(obj != long.MaxValue) {
                return obj;
            }

            obj = bundle.GetFloat(key, float.MaxValue);
            if(obj < float.MaxValue) {
                return obj;
            }
            
            obj = bundle.GetDouble(key, double.MaxValue);
            if(obj < double.MaxValue) {
                return obj;
            }
            throw new ArgumentException($"Couldn't extract value for key {key} from bundle: {bundle}");
        }
    }
}