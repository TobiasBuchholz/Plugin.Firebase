using System.Threading.Tasks;
using Android.Gms.Extensions;
using Plugin.Firebase.Functions;
using Firebase.Functions;
using GoogleGson;
using GoogleGson.Reflect;
using Java.Lang;
using Java.Util;
using Newtonsoft.Json;

namespace Plugin.Firebase.Android.Functions
{
    public sealed class HttpsCallableWrapper : IHttpsCallable
    {
        private readonly HttpsCallableReference _httpsCallable;

        public HttpsCallableWrapper(HttpsCallableReference httpsCallable)
        {
            _httpsCallable = httpsCallable;
        }

        public Task CallAsync(string dataJson = null)
        {
            return _httpsCallable.Call(ConvertJsonToData(dataJson)).AsAsync();
        }
        
        private static Object ConvertJsonToData(string dataJson = null)
        {
            return new Gson().FromJson(dataJson, TypeToken.GetParameterized(
                    TypeToken.Get(Class.FromType(typeof(HashMap))).Type,
                    TypeToken.Get(Class.FromType(typeof(String))).Type,
                    TypeToken.Get(Class.FromType(typeof(Object))).Type)
                .Type);
        }

        public async Task<TResponse> CallAsync<TResponse>(string dataJson = null)
        {
            var result = await _httpsCallable.Call(ConvertJsonToData(dataJson)).AsAsync<HttpsCallableResult>();
            return JsonConvert.DeserializeObject<TResponse>(result.Data.ToString());
        }
    }
}