using System.Linq;
using Plugin.Firebase.Abstractions.Common;

namespace System.Collections.Generic
{
    public static class DictionaryExtensions
    {
        public static Dictionary<object, object> ToDictionary(this IEnumerable<(object, object)> @this)
        {
            var dict = new Dictionary<object, object>();
            @this.ToList().ForEach(x => dict.Add(x.Item1, x.Item2));
            return dict;
        }
    }
}