using System;
using Firebase.CloudFirestore;
using Foundation;
using Plugin.Firebase.Abstractions.Firestore;
using Plugin.Firebase.Firestore;
using GeoPoint = Plugin.Firebase.Abstractions.Firestore.GeoPoint;

namespace Plugin.Firebase.iOS.Extensions
{
    public static class NSObjectExtension
    {
        public static T Cast<T>(this NSDictionary @this)
        {
            return (T) @this.Cast(typeof(T));
        }
        
        private static object Cast(this NSDictionary @this, Type targetType)
        {
            var instance = Activator.CreateInstance(targetType);
            var properties = targetType.GetProperties();
            foreach(var property in properties) {
                var attribute = (FirestorePropertyAttribute) property.GetCustomAttributes(typeof(FirestorePropertyAttribute), true)[0];
                var value = @this[attribute.PropertyName];
                if(value == null) {
                    Console.WriteLine($"[Plugin.Firebase] Couldn't cast property '{attribute.PropertyName}' of '{targetType}' because it's not contained in the NSDictionary.");
                } else {
                    property.SetValue(instance, value.ToObject(property.PropertyType));
                }
            }
            return instance; 
        }

        public static object ToObject(this NSObject @this, Type targetType = null)
        {
            switch(@this) {
                case NSNumber x:
                    return x.ToObject(targetType);
                case NSString x:
                    return x.ToString();
                case NSDate x:
                    return x.ToDateTime();
                case NSDictionary x:
                    return x.Cast(targetType);
                case NSArray x:
                    return x.ToList(targetType?.GenericTypeArguments[0]);
                case global::Firebase.CloudFirestore.GeoPoint x:
                    return new GeoPoint(x.Latitude, x.Longitude);
                case Timestamp x:
                    return x.ToDateTime(); 
                case DocumentReference x:
                    return new DocumentReferenceWrapper(x);
                case NSNull x:
                    return null;
                default:
                    throw new ArgumentException($"Could not convert NSObject of type {@this.GetType()} to object");
            }
        }

        public static object ToObject(this NSNumber @this, Type targetType = null)
        {
            if(targetType == null) {
                return @this.Int32Value;
            }
            
            switch(Type.GetTypeCode(targetType)) {
                case TypeCode.Boolean:
                    return @this.BoolValue;
                case TypeCode.Char:
                    return Convert.ToChar(@this.ByteValue);
                case TypeCode.SByte:
                    return @this.SByteValue;
                case TypeCode.Byte:
                    return @this.ByteValue;
                case TypeCode.Int16:
                    return @this.Int16Value;
                case TypeCode.UInt16:
                    return @this.UInt16Value;
                case TypeCode.Int32:
                    return @this.Int32Value;
                case TypeCode.UInt32:
                    return @this.UInt32Value;
                case TypeCode.Int64:
                    return @this.Int64Value;
                case TypeCode.UInt64:
                    return @this.UInt64Value;
                case TypeCode.Single:
                    return @this.FloatValue;
                case TypeCode.Double:
                    return @this.DoubleValue;
                default:
                    return null;
            }
        }        
    }
}