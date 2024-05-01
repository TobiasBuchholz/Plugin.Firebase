using Firebase.Functions;
using Plugin.Firebase.Core;
using Plugin.Firebase.Functions.Platforms.Android;

namespace Plugin.Firebase.Functions;

public sealed class FirebaseFunctionsImplementation : DisposableBase, IFirebaseFunctions
{
    private readonly FirebaseFunctions _functions;

    public FirebaseFunctionsImplementation()
    {
        _functions = FirebaseFunctions.Instance;
    }

    public FirebaseFunctionsImplementation(string region)
    {
        _functions = FirebaseFunctions.GetInstance(region);
    }

    public IHttpsCallable GetHttpsCallable(string name)
    {
        return new HttpsCallableWrapper(_functions.GetHttpsCallable(name));
    }

    public void UseEmulator(string host, int port)
    {
        _functions.UseEmulator(host, port);
    }
}