using Android.Gms.Tasks;
using Object = Java.Lang.Object;

namespace Plugin.Firebase.Core.Platforms.Android;

public sealed class OnSuccessListener : Object, IOnSuccessListener
{
    private readonly Action<Object> _action;

    public OnSuccessListener(Action<Object> action)
    {
        _action = action;
    }

    public void OnSuccess(Object result)
    {
        _action(result);
    }
}