namespace Plugin.Firebase.Functions;

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

    /// <summary>
    /// Modifies this FirebaseFunctions instance to communicate with the Cloud Functions emulator.
    /// Note: Call this method before using the instance to do any functions operations.
    /// </summary>
    /// <param name="host">The emulator host (for example, 10.0.2.2 on android and localhost on iOS)</param>
    /// <param name="port">The emulator port (for example, 5001)</param>
    void UseEmulator(string host, int port);
}