using System;
using Plugin.Firebase.DynamicLinks.EventArgs;

namespace Plugin.Firebase.DynamicLinks
{
    public interface IFirebaseDynamicLinks : IDisposable
    {
        string GetDynamicLink();
        IDynamicLinkBuilder CreateDynamicLink();
        
        event EventHandler<DynamicLinkReceivedEventArgs> DynamicLinkReceived;
        event EventHandler<DynamicLinkFailedEventArgs> DynamicLinkFailed;
    }
}