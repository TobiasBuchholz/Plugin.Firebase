using System;

namespace Plugin.Firebase.Functions
{
    public interface IFirebaseFunctions : IDisposable
    {
        IHttpsCallable GetHttpsCallable(string name);
    }
}