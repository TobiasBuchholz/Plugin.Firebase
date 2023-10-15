namespace Playground.FcmDemo
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void fcmRefresh_Clicked(object sender, EventArgs e)
        {
            // Get Token from "global store" as this is async and I try to keep it simple
            fcmTokenEntry.Text = App.FcmToken;
        }

        // HOW TO CONTINUE?
        // 0. Validate that the Token stays the same on every app-start (as this Token is a unique device identifier for all devices running your app)
        // 1. Use token to push a message directly to this device
        // or 2. Alternative: use Firebase Console, create a message and send it to all devices or devices with the channel "com.PluginFirebase.Playground.FcmDemo.general" (as in: "PCKGNAME.general"). The channel is configured and registered in Android MainActivity.cs.
        // BEWARE: starting the app in Debug works HOWEVER closing it using the debugger crashes the Android FCM system. In this case you have to restart the app and close it properly. And then start it again. Or restart the emulator. Something like this.
    }

}
