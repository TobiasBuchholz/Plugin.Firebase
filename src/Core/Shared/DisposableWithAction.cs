namespace Plugin.Firebase.Core;

/// <summary>
/// <see cref="DisposableBase"/> implementation that executes an <see cref="Action"/> when disposed.
/// </summary>
public sealed class DisposableWithAction : DisposableBase
{
    private readonly Action _action;

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="action">Action to execute when disposing.</param>
    public DisposableWithAction(Action action)
    {
        _action = action;
    }

    /// <inheritdoc />
    public override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if(disposing) {
            _action?.Invoke();
        }
    }
}