using System.Collections;
using Android.Runtime;

namespace Plugin.Firebase.RemoteConfig.Platforms.Android.Extensions;

public static class ListExtensions
{
    public static IList ToList(this JavaList @this, Type targetType = null)
    {
        var list = targetType == null ? new List<object>() : (IList) Activator.CreateInstance(typeof(List<>).MakeGenericType(targetType));
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

    public static JavaList ToJavaList(this IEnumerable @this)
    {
        var list = new JavaList();
        foreach(var item in @this) {
            list.Add(item.ToJavaObject());
        }
        return list;
    }
}