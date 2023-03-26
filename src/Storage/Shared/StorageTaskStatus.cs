namespace Plugin.Firebase.Storage;

public enum StorageTaskStatus : long
{
    Unknown,
    Progress,
    Pause,
    Success,
    Failure
}