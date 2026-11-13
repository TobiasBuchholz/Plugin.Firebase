using System.Collections;
using Android.Runtime;
using Plugin.Firebase.Firestore.Internal;

namespace Plugin.Firebase.Firestore.Platforms.Android.Extensions;

public static class ListExtensions
{
    public static IList ToList(this JavaList @this, Type? targetType = null)
    {
        return @this.ToList(targetType, field: null);
    }

    internal static IList ToList(this JavaList @this, Type? targetType, string? field)
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
            // CTS-mapped CLR values (e.g. long, double) follow the same rules as dictionary and model reads
            list.Add(value is Java.Lang.Object javaValue
                ? javaValue.ToObject(targetType, field)
                : FirestoreValueConversion.ToTargetType(value, targetType, field));
        }
        return list;
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
