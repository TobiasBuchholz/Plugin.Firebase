using Firebase.CloudFunctions;
using Plugin.Firebase.Core;
using Plugin.Firebase.Functions.Platforms.iOS;

namespace Plugin.Firebase.Functions;

/// <summary>
/// iOS implementation of <see cref="IFirebaseFunctions"/> that wraps the native Firebase Cloud Functions SDK.
/// </summary>
public sealed class FirebaseFunctionsImplementation : DisposableBase, IFirebaseFunctions
{
    private readonly CloudFunctions _functions;

    /// <summary>
    /// Initializes a new instance of the <see cref="FirebaseFunctionsImplementation"/> class using the default region.
    /// </summary>
    public FirebaseFunctionsImplementation()
    {
        _functions = CloudFunctions.DefaultInstance;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FirebaseFunctionsImplementation"/> class for a specific region.
    /// </summary>
    /// <param name="region">The region where the Cloud Functions are deployed.</param>
    public FirebaseFunctionsImplementation(string region)
    {
        _functions = CloudFunctions.FromRegion(region);
    }

    /// <inheritdoc/>
    public IHttpsCallable GetHttpsCallable(string name)
    {
        return new HttpsCallableWrapper(_functions.HttpsCallable(name));
    }

    /// <inheritdoc/>
    public void UseEmulator(string host, int port)
    {
        _functions.UseEmulatorOriginWithHost(host, (uint) port);
    }
}