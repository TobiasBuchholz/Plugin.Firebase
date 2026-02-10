using System.Collections;

namespace Plugin.Firebase.Firestore.Platforms.iOS.Extensions;

/// <summary>
/// Extension methods for converting between .NET lists and native iOS NSArray types.
/// </summary>
public static class ListExtensions
{
    /// <summary>
    /// Converts a .NET list to a native iOS NSArray.
    /// </summary>
    /// <param name="this">The list to convert.</param>
    /// <returns>A native iOS NSArray containing the converted elements.</returns>
    public static NSArray ToNSArray(this IList @this)
    {
        var array = new NSMutableArray();
        foreach(var item in @this) {
            array.Add(item.ToNSObject());
        }
        return array;
    }

    /// <summary>
    /// Converts a native iOS NSArray to a typed .NET list.
    /// </summary>
    /// <param name="this">The NSArray to convert.</param>
    /// <param name="targetType">The element type for the resulting list.</param>
    /// <returns>A typed list containing the converted elements.</returns>
    public static IList ToList(this NSArray @this, Type targetType)
    {
        var list = (IList) Activator.CreateInstance(typeof(List<>).MakeGenericType(targetType));
        for(nuint i = 0; i < @this.Count; i++) {
            var item = @this.GetItem<NSObject>(i);
            if(item != null)
                list.Add(item.ToObject(targetType));
        }
        return list;
    }
}