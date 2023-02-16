using Android.Runtime;
using Xamarin.Facebook;

namespace Plugin.Firebase.Android.Auth.Facebook;

public sealed class FacebookCallback<TResult> : Java.Lang.Object, IFacebookCallback where TResult : Java.Lang.Object
{
    private readonly Action _onCancel;
    private readonly Action<FacebookException> _onError;
    private readonly Action<TResult> _onSuccess;

    public FacebookCallback(
        Action onCancel = null,
        Action<FacebookException> onError = null,
        Action<TResult> onSuccess = null)
    {
        _onCancel = onCancel;
        _onError = onError;
        _onSuccess = onSuccess;
    }

    public void OnCancel()
    {
        _onCancel?.Invoke();
    }

    public void OnError(FacebookException error)
    {
        _onError?.Invoke(error);
    }

    public void OnSuccess(Java.Lang.Object result)
    {
        _onSuccess?.Invoke(result.JavaCast<TResult>());
    }
}