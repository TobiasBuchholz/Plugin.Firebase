using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Firebase.CloudFirestore;
using Foundation;
using Plugin.Firebase.Common;
using Plugin.Firebase.Firestore;
using Plugin.Firebase.iOS.Firestore;
using FieldValue = Plugin.Firebase.Firestore.FieldValue;
using GeoPoint = Plugin.Firebase.Firestore.GeoPoint;

namespace Plugin.Firebase.iOS.Extensions
{
    public static class NSObjectExtension
    {
        public static T Cast<T>(this NSDictionary @this, string documentId = null)
        {
            return (T) @this.Cast(typeof(T), documentId);
        }

        private static object Cast(this NSDictionary @this, Type targetType, string documentId = null)
        {
            var instance = Activator.CreateInstance(targetType);
            var properties = targetType.GetProperties();
            foreach(var property in properties) {
                if(documentId != null && property.GetCustomAttributes(typeof(FirestoreDocumentIdAttribute), true).Any()) {
                    property.SetValue(instance, documentId);
                    continue;
                }

                var attributes = property.GetCustomAttributes(typeof(FirestorePropertyAttribute), true);
                if(attributes.Any()) {
                    var attribute = (FirestorePropertyAttribute) attributes[0];
                    var value = @this[attribute.PropertyName];
                    if(value == null) {
                        Console.WriteLine($"[Plugin.Firebase] Couldn't cast property '{attribute.PropertyName}' of '{targetType}' because it's not contained in the NSDictionary.");
                    } else {
                        property.SetValue(instance, value.ToObject(property.PropertyType));
                    }
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
                    return x.ToDateTimeOffset();
                case NSDictionary x:
                    return x.ToDictionaryObject(targetType);
                case NSArray x:
                    return x.ToList(GetGenericListType(targetType));
                case global::Firebase.CloudFirestore.GeoPoint x:
                    return new GeoPoint(x.Latitude, x.Longitude);
                case Timestamp x:
                    return x.ToDateTimeOffset();
                case DocumentReference x:
                    return new DocumentReferenceWrapper(x);
                case NSNull x:
                    return null;
                default:
                    throw new ArgumentException($"Could not convert NSObject of type {@this.GetType()} to object");
            }
        }

        private static Type GetGenericListType(Type targetType)
        {
            var genericType = targetType.GenericTypeArguments?.FirstOrDefault();
            if(genericType == null) {
                throw new ArgumentException($"Couldn't get generic list type of targetType {targetType}. Make sure to use a list IList<T> instead of an array T[] as type in your FirestoreObject.");
            }
            return genericType;
        }

        private static object ToDictionaryObject(this NSDictionary @this, Type targetType)
        {
            if(targetType != null && targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Dictionary<,>)) {
                var types = targetType.GenericTypeArguments;
                return @this.ToDictionary(types[0], types[1]);
            } else {
                return @this.Cast(targetType);
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

        public static NSObject ToNSObject(this object @this)
        {
            switch(@this) {
                case string x:
                    return new NSString(x);
                case int x:
                    return new NSNumber(x);
                case long x:
                    return new NSNumber(x);
                case float x:
                    return new NSNumber(x);
                case double x:
                    return new NSNumber(x);
                case bool x:
                    return new NSNumber(x);
                case DateTime x:
                    return x.ToNSDate();
                case DateTimeOffset x:
                    return x.ToNSDate();
                case FieldValue x:
                    return x.ToNative();
                case IList x:
                    return x.ToNSArray();
                case IDictionary x:
                    return x.ToNSDictionaryFromNonGeneric();
                case DocumentReferenceWrapper x:
                    return x.Wrapped;
                case IFirestoreObject x:
                    return x.ToNSObject();
                default:
                    if(@this is Enum) {
                        var enumType = Enum.GetUnderlyingType(@this.GetType());
                        if(enumType == typeof(int)) {
                            return new NSNumber(Convert.ToInt32(@this));
                        } else if(enumType == typeof(uint)) {
                            return new NSNumber(Convert.ToUInt32(@this));
                        } else if(enumType == typeof(long)) {
                            return new NSNumber(Convert.ToInt64(@this));
                        } else if(enumType == typeof(ulong)) {
                            return new NSNumber(Convert.ToUInt64(@this));
                        } else if(enumType == typeof(sbyte)) {
                            return new NSNumber(Convert.ToSByte(@this));
                        } else if(enumType == typeof(byte)) {
                            return new NSNumber(Convert.ToByte(@this));
                        } else if(enumType == typeof(short)) {
                            return new NSNumber(Convert.ToInt16(@this));
                        } else if(enumType == typeof(ushort)) {
                            return new NSNumber(Convert.ToUInt16(@this));
                        }
                    }
                    throw new ArgumentException($"Could not convert object of type {@this.GetType()} to NSObject. Does it extend {nameof(IFirestoreObject)}?");
            }
        }

        public static NSArray ToNSArray(this IList @this)
        {
            var array = new NSMutableArray();
            foreach(var item in @this) {
                array.Add(item.ToNSObject());
            }
            return array;
        }

        public static NSObject ToNSObject(this IFirestoreObject @this)
        {
            return @this.ToDictionary().ToNSObject();
        }
    }
}