using System;
using Plugin.Firebase.Android.Common;
using Object = Java.Lang.Object;
using GmsTask = Android.Gms.Tasks.Task;

namespace Plugin.Firebase.Extensions
{
    public static class TaskExtensions
    {
        public static GmsTask AddOnSuccessListener(this GmsTask @this, Action<Object> action)
        {
            return @this.AddOnSuccessListener(new OnSuccessListener(action));
        }
        
        public static GmsTask AddOnFailureListener(this GmsTask @this, Action<Exception> action)
        {
            return @this.AddOnFailureListener(new OnFailureListener(action));
        }
    }
}