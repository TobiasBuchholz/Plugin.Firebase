using System.Collections.Generic;

namespace Plugin.Firebase.Storage
{
    public interface IStorageListResult
    {
        /// <summary>
        /// The items (files) returned by the <c>ListAsync()</c> operation.
        /// </summary>
        IEnumerable<IStorageReference> Items { get; }

        /// <summary>
        /// The prefixes (folders) returned by the <c>ListAsync()</c> operation.
        /// </summary>
        IEnumerable<IStorageReference> Prefixes { get; }

        /// <summary>
        /// Returns a token that can be used to resume a previous <c>ListAsync()</c> operation. null indicates that there are no more results.
        /// </summary>
        string PageToken { get; }
    }
}