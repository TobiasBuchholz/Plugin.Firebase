using System;
using System.Threading.Tasks;
using Android.Content;
using Android.Gms.Extensions;
using Android.Runtime;
using Firebase.DynamicLinks;
using Plugin.Firebase.DynamicLinks.EventArgs;
using Plugin.Firebase.Android.DynamicLinks;
using Exception = Java.Lang.Exception;

namespace Plugin.Firebase.DynamicLinks
{
    [Preserve(AllMembers = true)]
    public class FirebaseDynamicLinksImplementation : IFirebaseDynamicLinks
    {
        private static event EventHandler<DynamicLinkReceivedEventArgs> _dynamicLinkReceived;
        private static event EventHandler<DynamicLinkFailedEventArgs> _dynamicLinkFailed;

        private static string _dynamicLink;

        public static async Task HandleDynamicLinkAsync(Intent intent)
        {
            try {
                if(await FirebaseDynamicLinks.Instance.GetDynamicLink(intent) is PendingDynamicLinkData dynamicLink) {
                    _dynamicLink = dynamicLink.Link.ToString();
                    _dynamicLinkReceived?.Invoke(null, new DynamicLinkReceivedEventArgs(_dynamicLink));
                }
            } catch(Exception e) {
                _dynamicLinkFailed?.Invoke(null, new DynamicLinkFailedEventArgs(e.Message));
            }
        }

        public string GetDynamicLink()
        {
            return _dynamicLink;
        }

        public IDynamicLinkBuilder CreateDynamicLink()
        {
            return new DynamicLinkBuilder();
        }

        public void Dispose()
        {
        }

        public event EventHandler<DynamicLinkReceivedEventArgs> DynamicLinkReceived {
            add => _dynamicLinkReceived += value;
            remove => _dynamicLinkReceived -= value;
        }

        public event EventHandler<DynamicLinkFailedEventArgs> DynamicLinkFailed {
            add => _dynamicLinkFailed += value;
            remove => _dynamicLinkFailed -= value;
        }
    }
}