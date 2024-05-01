using Firebase.CloudFunctions;
using Plugin.Firebase.Core;
using Plugin.Firebase.Functions.Platforms.iOS;

namespace Plugin.Firebase.Functions;

public sealed class FirebaseFunctionsImplementation : DisposableBase, IFirebaseFunctions
{
    private readonly CloudFunctions _functions;

    public FirebaseFunctionsImplementation()
    {
        _functions = CloudFunctions.DefaultInstance;
    }

    public FirebaseFunctionsImplementation(string region)
    {
        _functions = CloudFunctions.FromRegion(region);
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