using System;
using System.Collections;
using System.Collections.Generic;
using Foundation;

namespace Plugin.Firebase.iOS.Extensions
{
    public static class ListExtensions
    {
        public static IList ToList(this NSArray @this, Type targetType)
        {
            var list = (IList) Activator.CreateInstance(typeof(List<>).MakeGenericType(targetType)); 
            for(nuint i = 0; i < @this.Count; i++) {
                list.Add(@this.GetItem<NSObject>(i).ToObject(targetType));
            }
            return list;
        }
    }
}