using System;
using Android.Gms.Tasks;

namespace Plugin.Firebase.Android.Common
{
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
}