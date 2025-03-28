using System.Collections;
using Foundation;

namespace Plugin.Firebase.Firestore.Platforms.iOS.Extensions;

public static class ListExtensions
{
    public static NSArray ToNSArray(this IList @this)
    {
        var array = new NSMutableArray();
        foreach(var item in @this) {
            array.Add(item.ToNSObject());
        }
        return array;
    }

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