using Android.Gms.Tasks;
using Exception = Java.Lang.Exception;
using Object = Java.Lang.Object;

namespace Plugin.Firebase.Core.Platforms.Android;

/// <summary>
/// Android callback adapter that wraps a failure handler delegate for use with Google Play Services Task API.
/// </summary>
public sealed class OnFailureListener : Object, IOnFailureListener
{
    private readonly Action<Exception> _action;

    /// <summary>
    /// Initializes a new instance of the <see cref="OnFailureListener"/> class with the specified failure handler.
    /// </summary>
    /// <param name="action">The delegate to invoke when the task fails.</param>
    public OnFailureListener(Action<Exception> action)
    {
        _action = action;
    }

    /// <summary>
    /// Called when the task fails.
    /// </summary>
    /// <param name="e">The exception that caused the task to fail.</param>
    public void OnFailure(Exception e)
    {
        _action(e);
    }
}