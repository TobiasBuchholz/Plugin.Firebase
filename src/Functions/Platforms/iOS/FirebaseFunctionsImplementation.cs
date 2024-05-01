using Firebase.CloudFunctions;
using Plugin.Firebase.Core;
using Plugin.Firebase.Functions.Platforms.iOS;

namespace Plugin.Firebase.Functions;

public sealed class FirebaseFunctionsImplementation : DisposableBase, IFirebaseFunctions
{
    private readonly CloudFunctions _functions;
    private readonly string _region; // Store the region

    public FirebaseFunctionsImplementation(string region = "us-central1") // Default to "us-central1" if not specified
    {
        _functions = CloudFunctions.SharedInstance;
        _region = region; // Initialize with specified region
    }

    public IHttpsCallable GetHttpsCallable(string name)
    {
        // Specify the region for each function
        var function = _functions.GetHttpsCallable(name);
        function.Region = _region;
        return new HttpsCallableWrapper(function);
    }

    public void UseEmulator(string host, int port)
    {
        _functions.UseFunctionsEmulatorOrigin($"{host}:{port}");
    }
}
