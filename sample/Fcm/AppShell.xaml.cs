namespace Playground.FcmDemo
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            _ = LoadTokenAsync();
        }

        private async Task LoadTokenAsync()
        {
            // There should be a "StartupPage" where you do all the initialization which might come AFTER app has been launched (check login, retrieve fcm token if not stored, ...)

            if(!await ((App) Application.Current).AwaitFcmTokenRetrieval())
            {
                Console.WriteLine("Token not loaded. Google not connectable?"); // Show a message to the user, app does not work due Google FCM was not ok
                return;
            }

            // Check Token on backend and store it for later use (e.g. FCM push from backend)

            // Continue app startup
        }
    }
}
