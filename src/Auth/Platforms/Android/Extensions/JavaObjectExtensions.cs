using Java.Util;
using Android.Runtime;
using IList = System.Collections.IList;

namespace Plugin.Firebase.Auth.Platforms.Android.Extensions;

public static class JavaObjectExtensions
{
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
                return x.ToDateTimeOffset();
            case JavaList x:
                return x.ToList(targetType?.GenericTypeArguments[0]);
            default:
                throw new ArgumentException($"Could not convert Java.Lang.Object of type {@this.GetType()} to object");
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
            case DateTimeOffset x:
                return x.ToJavaDate();
            case JavaDictionary x:
                return x;
            case HashMap x:
                return x;
            case IList x:
                return x.ToJavaList();
            default:
                if(@this is Enum) {
                    var enumType = Enum.GetUnderlyingType(@this.GetType());
                    if(enumType == typeof(int)) {
                        return Convert.ToInt32(@this);
                    } else if(enumType == typeof(uint)) {
                        return (int) Convert.ToUInt32(@this);
                    } else if(enumType == typeof(long)) {
                        return Convert.ToInt64(@this);
                    } else if(enumType == typeof(ulong)) {
                        return (long) Convert.ToUInt64(@this);
                    } else if(enumType == typeof(sbyte)) {
                        return Convert.ToSByte(@this);
                    } else if(enumType == typeof(byte)) {
                        return (sbyte) Convert.ToByte(@this);
                    } else if(enumType == typeof(short)) {
                        return Convert.ToInt16(@this);
                    } else if(enumType == typeof(ushort)) {
                        return (short) Convert.ToUInt16(@this);
                    }
                }
                throw new ArgumentException($"Could not convert object of type {@this.GetType()} to Java.Lang.Object");
        }
    }
}