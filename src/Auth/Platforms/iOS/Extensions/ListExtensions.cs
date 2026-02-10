using System.Collections;

namespace Plugin.Firebase.Auth.Platforms.iOS.Extensions;

/// <summary>
/// Provides extension methods for converting native iOS NSArray types to .NET lists.
/// </summary>
public static class ListExtensions
{
    /// <summary>
    /// Converts a native NSArray to a .NET IList with the specified element type.
    /// </summary>
    /// <param name="this">The NSArray to convert.</param>
    /// <param name="targetType">The target type for list elements.</param>
    /// <returns>A .NET list containing the converted elements.</returns>
    public static IList ToList(this NSArray @this, Type targetType)
    {
        var list = (IList) Activator.CreateInstance(typeof(List<>).MakeGenericType(targetType));
        for(nuint i = 0; i < @this.Count; i++) {
            list.Add(@this.GetItem<NSObject>(i).ToObject(targetType));
        }
        return list;
    }
}