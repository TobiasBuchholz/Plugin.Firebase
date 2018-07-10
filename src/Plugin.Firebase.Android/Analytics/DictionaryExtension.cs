using System.Linq;
using Android.OS;

namespace System.Collections.Generic
{
    public static class DictionaryExtension
    {
        public static Bundle ToBundle(this IDictionary<string, object> dictionary)
        {
            var bundle = new Bundle();
            dictionary.ToList().ForEach(x => PutIntoBundle(x, ref bundle));
            return bundle;
        }
        
        private static void PutIntoBundle(KeyValuePair<string, object> pair, ref Bundle bundle)
        {
            switch(pair.Value) {
                case bool x:
                    bundle.PutBoolean(pair.Key, x);
                    break;
                case char x:
                    bundle.PutChar(pair.Key, x);
                    break;
                case double x:
                    bundle.PutDouble(pair.Key, x);
                    break;
                case float x:
                    bundle.PutFloat(pair.Key, x);
                    break;
                case long x:
                    bundle.PutLong(pair.Key, x);
                    break;
                case int x:
                    bundle.PutInt(pair.Key, x);
                    break;
                case short x:
                    bundle.PutShort(pair.Key, x);
                    break;
                case string x:
                    bundle.PutString(pair.Key, x);
                    break;
                default:
                    throw new ArgumentException($"Couldn't put object of type {pair.Value.GetType()} into android bundle");
            }
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
                    throw new ArgumentException($"Couldn't put object of type {pair.Value.GetType()} into Java.Lang.Object dictionary");
            }
        }
        
        public static IDictionary<string, Java.Lang.Object> ToJavaObjectDictionary(this IEnumerable<(string, object)> tuples)
        {
            var dict = new Dictionary<string, object>();
            tuples.ToList().ForEach(x => dict.Add(x.Item1, x.Item2));
            return dict.ToJavaObjectDictionary();
        }
    }
}