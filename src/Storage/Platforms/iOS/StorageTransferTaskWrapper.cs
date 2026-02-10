using Firebase.Storage;
using Plugin.Firebase.Storage.Platforms.iOS.Extensions;

namespace Plugin.Firebase.Storage.Platforms.iOS;

/// <summary>
/// Wraps a native iOS Firebase storage transfer task to implement IStorageTransferTask.
/// </summary>
/// <typeparam name="TStorageTransferTask">The native iOS storage transfer task type.</typeparam>
/// <typeparam name="TCompletionResult">The completion result type.</typeparam>
public sealed class StorageTransferTaskWrapper<TStorageTransferTask, TCompletionResult>
    : IStorageTransferTask
    where TStorageTransferTask : StorageObservableTask, IStorageTaskManagement
{
    private readonly TaskCompletionSource<TCompletionResult> _tcs;
    private readonly IDictionary<Action<IStorageTaskSnapshot>, string> _observerDict;

    /// <summary>
    /// Initializes a new instance of the <see cref="StorageTransferTaskWrapper{TStorageTransferTask, TCompletionResult}"/> class.
    /// </summary>
    public StorageTransferTaskWrapper()
    {
        _tcs = new TaskCompletionSource<TCompletionResult>();
        _observerDict = new Dictionary<Action<IStorageTaskSnapshot>, string>();

        CompletionHandler = (result, error) => {
            if(error == null) {
                _tcs.SetResult(result);
            } else {
                _tcs.SetException(new Exception(error.LocalizedDescription));
            }
        };
    }

    /// <inheritdoc/>
    public Task AwaitAsync()
    {
        return _tcs.Task;
    }

    /// <inheritdoc/>
    public void AddObserver(StorageTaskStatus status, Action<IStorageTaskSnapshot> observer)
    {
        if(TransferTask == null) {
            throw new ArgumentException(
                $"You have to set the {nameof(TransferTask)} property before calling this method"
            );
        }

        var handle = TransferTask.ObserveStatus(
            status.ToNative(),
            x => observer.Invoke(x.ToAbstract())
        );
        _observerDict[observer] = handle;
    }

    /// <inheritdoc/>
    public void RemoveObserver(Action<IStorageTaskSnapshot> observer)
    {
        if(_observerDict.ContainsKey(observer)) {
            TransferTask.RemoveObserver(_observerDict[observer]);
            _observerDict.Remove(observer);
        }
    }

    /// <inheritdoc/>
    public void Pause()
    {
        TransferTask.Pause();
    }

    /// <inheritdoc/>
    public void Resume()
    {
        TransferTask.Resume();
    }

    /// <inheritdoc/>
    public void Cancel()
    {
        TransferTask.Cancel();
    }

    /// <summary>
    /// Delegate for handling storage transfer completion.
    /// </summary>
    /// <param name="result">The completion result.</param>
    /// <param name="error">The error if the transfer failed, otherwise null.</param>
    public delegate void StorageTransferCompletionHandler(TCompletionResult result, NSError error);

    /// <summary>
    /// Gets the completion handler for the transfer task.
    /// </summary>
    public StorageTransferCompletionHandler CompletionHandler { get; }

    /// <summary>
    /// Gets or sets the underlying native transfer task.
    /// </summary>
    public TStorageTransferTask TransferTask { private get; set; }
}