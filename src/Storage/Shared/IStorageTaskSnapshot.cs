using System;

namespace Plugin.Firebase.Storage
{
    /// <summary>
    /// <c>IStorageTaskSnapshot</c> represents an immutable view of a task. A Snapshot contains a task, storage reference, metadata (if it exists),
    /// progress, and an error (if one occurred).
    /// </summary>
    public interface IStorageTaskSnapshot
    {
        /// <summary>
        /// The total bytes uploaded so far.
        /// </summary>
        long TransferredUnitCount { get; }

        /// <summary>
        /// The number of bytes to upload. Will return -1 if uploading from a stream. 
        /// </summary>
        long TotalUnitCount { get; }

        /// <summary>
        /// The total bytes uploaded so far as a fraction.
        /// </summary>
        double TransferredFraction { get; }

        /// <summary>
        /// Metadata returned by the task, or null if no metadata returned.
        /// </summary>
        IStorageMetadata Metadata { get; }

        /// <summary>
        /// Error during task execution, or null if no error occurred.
        /// </summary>
        Exception Error { get; }
    }
}