using Android.Gms.Tasks;
using Task = Android.Gms.Tasks.Task;

namespace Plugin.Firebase.Core.Platforms.Android;

/// <summary>
/// Android callback adapter that wraps a task completion delegate for use with Google Play Services Task API.
/// </summary>
public class OnCompleteListener : Java.Lang.Object, IOnCompleteListener
{
    private readonly Action<Task> _action;

    /// <summary>
    /// Initializes a new instance of the <see cref="OnCompleteListener"/> class with the specified completion action.
    /// </summary>
    /// <param name="action">The delegate to invoke when the task completes (success or failure).</param>
    public OnCompleteListener(Action<Task> action)
    {
        _action = action;
    }

    /// <summary>
    /// Called when the task completes.
    /// </summary>
    /// <param name="task">The completed task.</param>
    public void OnComplete(Task task)
    {
        _action?.Invoke(task);
    }
}