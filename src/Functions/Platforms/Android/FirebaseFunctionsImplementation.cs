using Plugin.Firebase.Functions;
using Plugin.Firebase.Common;
using Firebase.Functions;
using Plugin.Firebase.Android.Functions;

namespace Plugin.Firebase.Functions;

public sealed class FirebaseFunctionsImplementation : DisposableBase, IFirebaseFunctions
{
    private readonly FirebaseFunctions _functions;

    public FirebaseFunctionsImplementation()
    {
        _functions = FirebaseFunctions.Instance;
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