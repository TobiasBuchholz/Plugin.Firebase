namespace Plugin.Firebase.Storage;

/// <summary>
/// Represents the status of a storage transfer task.
/// </summary>
public enum StorageTaskStatus : long
{
    /// <summary>
    /// The task status is unknown.
    /// </summary>
    Unknown,

    /// <summary>
    /// The task is in progress.
    /// </summary>
    Progress,

    /// <summary>
    /// The task is paused.
    /// </summary>
    Pause,

    /// <summary>
    /// The task completed successfully.
    /// </summary>
    Success,

    /// <summary>
    /// The task failed.
    /// </summary>
    Failure,
}