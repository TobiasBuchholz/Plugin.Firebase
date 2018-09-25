using System;
using System.Collections;
using System.Collections.Generic;
using Android.Runtime;

namespace Plugin.Firebase.Android.Extensions
{
    public static class ListExtensions
    {
        public static IList ToList(this JavaList @this, Type targetType = null) 
        {
            var list = (IList) Activator.CreateInstance((typeof(List<>).MakeGenericType(targetType))); 
            for(var i = 0; i < @this.Size(); i++) {
                var value = @this[i];
                if(value is Java.Lang.Object javaValue) {
                    list.Add(javaValue.ToObject(targetType));
                } else {
                    list.Add(value);
                }
            }
            return list;
        }
    }
}