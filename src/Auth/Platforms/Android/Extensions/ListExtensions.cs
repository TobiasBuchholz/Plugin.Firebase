using System.Collections;
using Android.Runtime;

namespace Plugin.Firebase.Auth.Platforms.Android.Extensions;

public static class ListExtensions
{
    public static IList ToList(this Java.Util.IList @this, Type? targetType = null)
    {
        var list = CreateList(targetType);
        for(var i = 0; i < @this.Size(); i++) {
            AddConvertedValue(list, @this.Get(i), targetType);
        }
        return list;
    }

    public static IList ToList(this JavaList @this, Type? targetType = null)
    {
        var list = CreateList(targetType);
        for(var i = 0; i < @this.Size(); i++) {
            AddConvertedValue(list, @this[i], targetType);
        }
        return list;
    }

    public static JavaList ToJavaList(this IEnumerable @this)
    {
        var list = new JavaList();
        foreach(var item in @this) {
            list.Add(item.ToJavaObject());
        }
        return list;
    }

    private static IList CreateList(Type? targetType)
    {
        return targetType == null
            ? new List<object?>()
            : (IList) Activator.CreateInstance(typeof(List<>).MakeGenericType(targetType))!;
    }

    private static void AddConvertedValue(IList list, object? value, Type? targetType)
    {
        if(value is Java.Lang.Object javaValue) {
            list.Add(javaValue.ToObject(targetType));
        } else {
            list.Add(value);
        }
    }
}