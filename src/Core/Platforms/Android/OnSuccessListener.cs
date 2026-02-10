using Android.Gms.Tasks;
using Object = Java.Lang.Object;

namespace Plugin.Firebase.Core.Platforms.Android;

/// <summary>
/// Android callback adapter that wraps a success action delegate for use with Google Play Services Task API.
/// </summary>
public sealed class OnSuccessListener : Object, IOnSuccessListener
{
    private readonly Action<Object> _action;

    /// <summary>
    /// Initializes a new instance of the <see cref="OnSuccessListener"/> class with the specified success action.
    /// </summary>
    /// <param name="action">The delegate to invoke when the task completes successfully.</param>
    public OnSuccessListener(Action<Object> action)
    {
        _action = action;
    }

    /// <summary>
    /// Called when the task completes successfully.
    /// </summary>
    /// <param name="result">The result of the completed task.</param>
    public void OnSuccess(Object result)
    {
        _action(result);
    }
}