using System;

namespace Plugin.Firebase.Functions
{
    /// <summary>
    /// FirebaseFunctions lets you call Cloud Functions for Firebase.
    /// </summary>
    public interface IFirebaseFunctions : IDisposable
    {
        /// <summary>
        /// Creates a reference to the Callable HTTPS trigger with the given name.
        /// </summary>
        /// <param name="name">The name of the Callable HTTPS trigger.</param>
        IHttpsCallable GetHttpsCallable(string name);
    }
}