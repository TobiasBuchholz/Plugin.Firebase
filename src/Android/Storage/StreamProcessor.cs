using System.IO;
using Firebase.Storage;

namespace Plugin.Firebase.Android.Storage
{
    public sealed class StreamProcessor : Java.Lang.Object, StreamDownloadTask.IStreamProcessor
    {
        public void DoInBackground(StreamDownloadTask.TaskSnapshot snapshot, Stream stream)
        {
            stream.Close();
        }
    }
}