using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Java.Util;
using Android.Runtime;
using Firebase.Firestore;
using Plugin.Firebase.Firestore;
using Plugin.Firebase.Android.Firestore;
using GeoPoint = Plugin.Firebase.Firestore.GeoPoint;
using NativeFirebase = Firebase;

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
                var attributes = property.GetCustomAttributes(typeof(FirestorePropertyAttribute), true);
                if(attributes.Any()) {
                    var attribute = (FirestorePropertyAttribute) attributes[0];
                    var value = @this[attribute.PropertyName];
                    if(value == null) {
                        Console.WriteLine($"[Plugin.Firebase] Couldn't cast property '{attribute.PropertyName}' of '{targetType}' because it's not contained in the dictionary.");
                    } else if(value is Java.Lang.Object javaValue) {
                        property.SetValue(instance, javaValue.ToObject(property.PropertyType));
                    } else {
                        property.SetValue(instance, value);
                    }
                }
            }
            return instance; 
        }

        public static object ToObject(this Java.Lang.Object @this, Type targetType = null)
        {
            switch(@this) {
                case Java.Lang.ICharSequence x:
                    return x.ToString();
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
                case Date x:
                    return x.ToDateTime();
                case NativeFirebase.Timestamp x:
                    return x.ToDate().ToDateTime();
                case IDictionary x:
                    return x.ToDictionaryObject(targetType);
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

        private static object ToDictionaryObject(this IDictionary @this, Type targetType)
        {
            if(targetType != null && targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Dictionary<,>)) {
                var types = targetType.GenericTypeArguments;
                return @this.ToDictionary(types[0], types[1]);
            } else {
                return @this.Cast(targetType);
            }
        }

        public static Java.Lang.Object ToJavaObject(this object @this)
        {
            switch(@this) {
                case string x:
                    return x;
                case int x:
                    return x;
                case long x:
                    return x;
                case float x:
                    return x;
                case double x:
                    return x;
                case bool x:
                    return x;
                case DateTime x:
                    return x.ToJavaDate();
                case JavaDictionary x:
                    return x;
                case IDictionary<string, object> x:
                    return x.ToHashMap();
            }
            throw new ArgumentException($"Could not convert object of type {@this.GetType()} to Java.Lang.Object");
        }
    }
}