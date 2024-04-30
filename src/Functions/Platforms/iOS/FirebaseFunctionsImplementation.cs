using Firebase.CloudFunctions;
using Plugin.Firebase.Core;
using Plugin.Firebase.Functions.Platforms.iOS;

namespace Plugin.Firebase.Functions;

public sealed class FirebaseFunctionsImplementation : DisposableBase, IFirebaseFunctions
{
    private readonly CloudFunctions _functions;

    public FirebaseFunctionsImplementation(string region = "us-central1") // Default to "us-central1" if not specified
    {
        _functions = CloudFunctions.GetInstance(region);
    }

    public IHttpsCallable GetHttpsCallable(string name)
    {
        return new HttpsCallableWrapper(_functions.HttpsCallable(name));
    }

    public void UseEmulator(string host, int port)
    {
        _functions.UseEmulatorOriginWithHost(host, (uint) port);
    }
}
