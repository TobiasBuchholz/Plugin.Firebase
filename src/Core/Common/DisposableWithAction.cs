namespace Plugin.Firebase.Common;

public sealed class DisposableWithAction : DisposableBase
{
    private readonly Action _action;

    public DisposableWithAction(Action action)
    {
        _action = action;
    }

    public override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if(disposing) {
            _action?.Invoke();
        }
    }
}