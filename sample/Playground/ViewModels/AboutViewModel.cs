using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Plugin.Firebase.RemoteConfig;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Playground.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        public AboutViewModel()
        {
            Title = "About";
            OpenWebCommand = new Command(async () => await Browser.OpenAsync("https://aka.ms/xamarin-quickstart"));

            TestRemoteConfigAsync();
        }
        
        private async Task TestRemoteConfigAsync()
        {
            try {
                var remoteConfig = CrossFirebaseRemoteConfig.Current;
                await remoteConfig.FetchAndActivateAsync();
                System.Diagnostics.Debug.WriteLine(CrossFirebaseRemoteConfig.Current.GetString("some_remote_config_key"));
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }

        public ICommand OpenWebCommand { get; }
    }
}