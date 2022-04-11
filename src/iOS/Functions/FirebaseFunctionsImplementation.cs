using Plugin.Firebase.Common;
using Plugin.Firebase.iOS.Functions;
using Firebase.CloudFunctions;

namespace Plugin.Firebase.Functions
{
    public sealed class FirebaseFunctionsImplementation : DisposableBase, IFirebaseFunctions
    {
        private readonly CloudFunctions _functions;

        public FirebaseFunctionsImplementation()
        {
            _functions = CloudFunctions.DefaultInstance;
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
}