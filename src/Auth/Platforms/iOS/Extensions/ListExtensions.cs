using System.Collections;
using Foundation;

namespace Plugin.Firebase.Auth.Platforms.iOS.Extensions;

public static class ListExtensions
{
    public static IList ToList(this NSArray @this, Type targetType)
    {
        var list = (IList) Activator.CreateInstance(typeof(List<>).MakeGenericType(targetType));
        for(nuint i = 0; i < @this.Count; i++) {
            list.Add(@this.GetItem<NSObject>(i).ToObject(targetType));
        }
        return list;
    }
}