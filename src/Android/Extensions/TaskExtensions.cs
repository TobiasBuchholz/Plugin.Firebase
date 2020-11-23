using System;
using System.Threading.Tasks;
using Android.Runtime;
using Plugin.Firebase.Android.Common;
using Object = Java.Lang.Object;
using Task = System.Threading.Tasks.Task;
using GmsTask = Android.Gms.Tasks.Task;

namespace Plugin.Firebase.Extensions
{
    public static class TaskExtensions
    {
        public static Task<T> ToTask<T>(this GmsTask @this) where T : class, IJavaObject
        {
            var tcs = new TaskCompletionSource<T>();
            @this
                .AddOnSuccessListener(x => tcs.SetResult(x.JavaCast<T>()))
                .AddOnFailureListener(e => tcs.SetException(e));
            return tcs.Task;
        }
        
        public static Task ToTask(this GmsTask @this) 
        {
            var tcs = new TaskCompletionSource<Object>();
            @this
                .AddOnSuccessListener(x => tcs.SetResult(x))
                .AddOnFailureListener(e => tcs.SetException(e));
            return tcs.Task;
        }
        
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