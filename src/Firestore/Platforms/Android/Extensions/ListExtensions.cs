using System.Collections;
using Android.Runtime;

namespace Plugin.Firebase.Firestore.Platforms.Android.Extensions;

public static class ListExtensions
{
    public static IList ToList(this JavaList @this, Type? targetType = null)
    {
        var list =
            targetType == null
                ? new List<object?>()
                : (IList?) Activator.CreateInstance(typeof(List<>).MakeGenericType(targetType));
        if(list is null) {
            throw new InvalidOperationException("Could not create list of type " + targetType);
        }

        for(var i = 0; i < @this.Size(); i++) {
            var value = @this[i];
            if(value is Java.Lang.Object javaValue) {
                list.AddConvertedValue(javaValue.ToObject(targetType), targetType);
            } else if(targetType == typeof(string)) {
                list.AddConvertedValue(Convert.ToString(value), targetType);
            } else if(targetType == typeof(int)) {
                list.AddConvertedValue(Convert.ToInt32(value), targetType);
            } else if(targetType == typeof(long)) {
                list.AddConvertedValue(Convert.ToInt64(value), targetType);
            } else if(targetType == typeof(float)) {
                list.AddConvertedValue(Convert.ToSingle(value), targetType);
            } else if(targetType == typeof(double)) {
                list.AddConvertedValue(Convert.ToDouble(value), targetType);
            } else if(targetType == typeof(decimal)) {
                list.AddConvertedValue(Convert.ToDecimal(value), targetType);
            } else if(targetType == typeof(bool)) {
                list.AddConvertedValue(Convert.ToBoolean(value), targetType);
            } else {
                list.AddConvertedValue(value, targetType);
            }
        }
        return list;
    }

    private static void AddConvertedValue(this IList list, object? value, Type? targetType)
    {
        list.Add(value.ConvertToTargetType(targetType));
    }

    public static JavaList ToJavaList(this IEnumerable @this)
    {
        // Refactored to address https://github.com/TobiasBuchholz/Plugin.Firebase/issues/392
        var list = new List<object?>();
        foreach (var item in @this)
        {
            list.Add(item.ToJavaObject());
        }
        return new JavaList(list);
    }

}
