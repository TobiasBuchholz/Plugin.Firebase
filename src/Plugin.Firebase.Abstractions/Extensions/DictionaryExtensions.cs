using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace System.Collections.Generic
{
    public static class DictionaryExtensions
    {
        public static Dictionary<object, TValue> ToDictionary<TValue>(this object obj)
        {       
            var json = JsonConvert.SerializeObject(obj);
            return JsonConvert.DeserializeObject<Dictionary<object, TValue>>(json);   
        } 
        
        public static Dictionary<object, object> ToDictionary(this IEnumerable<(object, object)> tuples)
        {
            var dict = new Dictionary<object, object>();
            tuples.ToList().ForEach(x => dict.Add(x.Item1, x.Item2));
            return dict;
        }
    }
}