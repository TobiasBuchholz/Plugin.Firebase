using System;
using System.Collections;
using System.Collections.Generic;
using Java.Util;
using Plugin.Firebase.Abstractions.Common;
using Android.Runtime;
using Firebase.Firestore;
using Plugin.Firebase.Firestore;
using GeoPoint = Plugin.Firebase.Abstractions.Firestore.GeoPoint;

namespace Plugin.Firebase.Android.Extensions
{
    public static class JavaObjectExtensions
    {
        public static T Cast<T>(this IDictionary<string, Java.Lang.Object> @this)
        {
            return (T) ((IDictionary) @this).Cast(typeof(T));
        }
        
        private static object Cast(this IDictionary @this, Type targetType)
        {
            var instance = Activator.CreateInstance(targetType);
            var properties = targetType.GetProperties();
            foreach(var property in properties) {
                var attribute = (FirestorePropertyAttribute) property.GetCustomAttributes(typeof(FirestorePropertyAttribute), true)[0];
                var value = @this[attribute.PropertyName];
                if(value == null) {
                    Console.WriteLine($"[Plugin.Firebase] Couldn't cast property '{attribute.PropertyName}' of '{targetType}' because it's not contained in the dictionary.");
                } else if(value is Java.Lang.Object javaValue) {
                    property.SetValue(instance, javaValue.ToObject(property.PropertyType));
                } else {
                    property.SetValue(instance, value);
                }
            }
            return instance; 
        }

        public static object ToObject(this Java.Lang.Object @this, Type targetType = null)
        {
            switch(@this) {
                case Java.Lang.Boolean x:
                    return x.BooleanValue();
                case Java.Lang.Integer x:
                    return x.IntValue();
                case Java.Lang.Double x:
                    return x.DoubleValue();
                case Java.Lang.Float x:
                    return x.FloatValue();
                case Java.Lang.Long x:
                    return x.LongValue();
                case Java.Lang.String x:
                    return x.ToString();
                case Date x:
                    return x.ToDateTime();
                case IDictionary x:
                    return x.Cast(targetType);
                case JavaList x:
                    return x.ToList(targetType?.GenericTypeArguments[0]);
                case global::Firebase.Firestore.GeoPoint x:
                    return new GeoPoint(x.Latitude, x.Longitude);
                case DocumentReference x:
                    return new DocumentReferenceWrapper(x);
                default:
                    throw new ArgumentException($"Could not convert Java.Lang.Object of type {@this.GetType()} to object");
            }
        }
    }
}