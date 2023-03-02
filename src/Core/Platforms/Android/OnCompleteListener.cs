using Android.Gms.Tasks;
using Task = Android.Gms.Tasks.Task;

namespace Plugin.Firebase.Core.Platforms.Android;

public class OnCompleteListener : Java.Lang.Object, IOnCompleteListener
{
    private readonly Action<Task> _action;

    public OnCompleteListener(Action<Task> action)
    {
        _action = action;
    }

    public void OnComplete(Task task)
    {
        _action?.Invoke(task);
    }
}