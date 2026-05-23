namespace Plugin.Firebase.IntegrationTests;

internal sealed class IntegrationTestResourceScope : IAsyncDisposable
{
    private readonly Stack<IAsyncDisposable> _resources = new();

    public T Add<T>(T resource) where T : IAsyncDisposable
    {
        _resources.Push(resource);
        return resource;
    }

    public async ValueTask DisposeAsync()
    {
        while(_resources.TryPop(out var resource)) {
            await resource.DisposeAsync();
        }
    }
}