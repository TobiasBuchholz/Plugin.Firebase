using System.Collections;
using Android.Runtime;

namespace Plugin.Firebase.Firestore.Platforms.Android.Extensions;

public static class ListExtensions
{
    public static IList ToList(this JavaList @this, Type targetType = null)
    {
        var list = targetType == null ? new List<object>() : (IList) Activator.CreateInstance(typeof(List<>).MakeGenericType(targetType));
        for(var i = 0; i < @this.Size(); i++) {
            var value = @this[i];
            if(value is Java.Lang.Object javaValue) {
                list.Add(javaValue.ToObject(targetType));
            } else if(targetType == typeof(string)) {
                list.Add(Convert.ToString(value));
            } else if(targetType == typeof(int)) {
                list.Add(Convert.ToInt32(value));
            } else if(targetType == typeof(long)) {
                list.Add(Convert.ToInt64(value));
            } else if(targetType == typeof(float)) {
                list.Add(Convert.ToSingle(value));
            } else if(targetType == typeof(double)) {
                list.Add(Convert.ToDouble(value));
            } else if(targetType == typeof(decimal)) {
                list.Add(Convert.ToDecimal(value));
            } else if(targetType == typeof(bool)) {
                list.Add(Convert.ToBoolean(value));
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
