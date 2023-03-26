using Firebase.Storage;

namespace Plugin.Firebase.Storage.Platforms.Android;

public sealed class StreamProcessor : Java.Lang.Object, StreamDownloadTask.IStreamProcessor
{
    public void DoInBackground(StreamDownloadTask.TaskSnapshot snapshot, Stream stream)
    {
        stream.Close();
    }
}