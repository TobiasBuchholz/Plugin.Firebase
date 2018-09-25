using System.Linq;
using Foundation;
using Plugin.Firebase.Abstractions.Firestore;
using Plugin.Firebase.iOS.Extensions;

namespace System.Collections.Generic
{
    public static class DictionaryExtension
    {
        public static NSDictionary<NSString, NSObject> ToNSDictionary(this IDictionary<string, object> dictionary)
        {
            var nsDictionary = new NSMutableDictionary<NSString, NSObject>();
            dictionary.ToList().ForEach(x => PutIntoNSDictionary(x, ref nsDictionary));
            return NSDictionary<NSString, NSObject>
                .FromObjectsAndKeys(
                    nsDictionary.Values.ToArray(), 
                    nsDictionary.Keys.ToArray(), 
                    (nint) nsDictionary.Count);
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
                default:
                    if(pair.Value == null) {
                        nsDictionary.Add((NSString) pair.Key, new NSNull());
                        break;
                    } else {
                        throw new ArgumentException($"Couldn't put object of type {pair.Value.GetType()} into NSDictionary");
                    }
            }
        }
        
        public static NSDictionary<NSString, NSObject> ToNSDictionary(this IEnumerable<(string, object)> tuples)
        {
            var dict = new Dictionary<string, object>();
            tuples.ToList().ForEach(x => dict.Add(x.Item1, x.Item2));
            return dict.ToNSDictionary();
        } 
        
        public static Dictionary<object, object> ToDictionary(this object @this)
        {       
            var dict = new Dictionary<object, object>();
            var properties = @this.GetType().GetProperties();
            foreach(var property in properties) {
                var attribute = (FirestorePropertyAttribute) property.GetCustomAttributes(typeof(FirestorePropertyAttribute), true)[0];
                var value = property.GetValue(@this);
                if(value is DateTime date) {
                    dict[attribute.PropertyName] = date.ToNSDate();
                } else {
                    dict[attribute.PropertyName] = value;
                }
            }
            return dict;
        } 
    }
}