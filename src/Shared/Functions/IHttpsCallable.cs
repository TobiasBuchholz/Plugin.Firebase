using System.Threading.Tasks;

namespace Plugin.Firebase.Functions
{
    public interface IHttpsCallable
    {
        Task CallAsync(string dataJson = null);
        Task<TResponse> CallAsync<TResponse>(string dataJson = null);
    }
}