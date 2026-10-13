using Firebase.Storage;

namespace Plugin.Firebase.Storage.Platforms.Android;

/// <summary>
/// Copies a storage object into managed memory while the native download streams on a
/// background thread, so the whole object is never buffered in a Java byte array. Reading
/// stops and the download fails once more than the allowed number of bytes has arrived.
/// </summary>
internal sealed class DownloadStreamProcessor
    : Java.Lang.Object, StreamDownloadTask.IStreamProcessor
{
    private const int BufferSize = 64 * 1024;
    private readonly long _maxSize;

    public DownloadStreamProcessor(long maxSize)
    {
        _maxSize = maxSize;
    }

    /// <summary>
    /// The downloaded contents. Only valid after the stream download task completed
    /// successfully.
    /// </summary>
    public MemoryStream Contents { get; } = new MemoryStream();

    /// <inheritdoc/>
    public void DoInBackground(StreamDownloadTask.TaskSnapshot? snapshot, Stream? inputStream)
    {
        ArgumentNullException.ThrowIfNull(inputStream);

        if(snapshot?.TotalByteCount > _maxSize) {
            throw CreateDownloadSizeExceededException();
        }

        var buffer = new byte[BufferSize];
        long total = 0;
        int read;
        while((read = inputStream.Read(buffer, 0, buffer.Length)) > 0) {
            total += read;
            if(total > _maxSize) {
                throw CreateDownloadSizeExceededException();
            }
            Contents.Write(buffer, 0, read);
        }
    }

    private Java.Lang.Exception CreateDownloadSizeExceededException()
    {
        return new Java.Lang.Exception(
            $"The downloaded data exceeds the maximum allowed size ({_maxSize} bytes)."
        );
    }
}