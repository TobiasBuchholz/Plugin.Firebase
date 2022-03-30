using System;
using System.Threading.Tasks;

namespace Plugin.Firebase.Storage
{
    /// <summary>
    /// A controllable Task that has a synchronized state machine.
    /// </summary>
    public interface IStorageTransferTask
    {
        Task AwaitAsync();

        /// <summary>
        /// Observes changes in the upload status: Progress, Pause, Success, and Failure.
        /// </summary>
        /// <param name="status">The <c>StorageTaskStatus</c> change to observe.</param>
        /// <param name="observer">
        /// A callback that fires every time the status event occurs, returns a <c>IStorageTaskSnapshot</c> containing the state of the task.
        /// </param>
        void AddObserver(StorageTaskStatus status, Action<IStorageTaskSnapshot> observer);

        /// <summary>
        /// Removes the single observer action.
        /// </summary>
        /// <param name="observer">The observer action of the task to remove.</param>
        void RemoveObserver(Action<IStorageTaskSnapshot> observer);

        /// <summary>
        /// Attempts to pause the task. 
        /// </summary>
        void Pause();

        /// <summary>
        /// Attempts to resume a paused task. 
        /// </summary>
        void Resume();

        /// <summary>
        /// Attempts to cancel the task. 
        /// </summary>
        void Cancel();
    }
}