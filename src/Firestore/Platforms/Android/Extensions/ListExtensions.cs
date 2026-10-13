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
            // a stored null element can arrive as a Java.Lang.Object proxy with a zero handle
            // rather than a managed null; converting it would corrupt the value (e.g. "" for
            // string targets), so null-equivalent values bypass conversion entirely
            var isNull = value is null || (value as Java.Lang.Object)?.Handle == IntPtr.Zero;
            // CTS-mapped CLR values (e.g. long, double) are narrowed by the same conversion as dictionary and model reads
            list.AddConvertedValue(
                isNull ? null : value is Java.Lang.Object javaValue ? javaValue.ToObject(targetType) : value,
                targetType);
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
        foreach(var item in @this) {
            list.Add(item.ToJavaObject());
        }
        return new JavaList(list);
    }

}