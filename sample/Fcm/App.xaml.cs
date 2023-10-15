using Plugin.Firebase.CloudMessaging;

namespace Playground.FcmDemo
{
    public partial class App : Application
    {
        public static string FcmToken { get; private set; }

        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }

        #region Fcm

        internal async Task<bool> AwaitFcmTokenRetrieval()
        {
#if DEBUG
            // iOS does not support FCM on Debug devices
            if(DeviceInfo.Platform == DevicePlatform.iOS)
            {
                return true;
            }
#endif

            string token = "";
            try
            {
                token = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();
                Console.WriteLine($"FCM Token received = {token}");
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error getting FCM token\n{ex}");
                return false;
            }

            // Ensure that Token is correctly stored
            return FcmTokenRegistration(token);
        }

        private object tokenRegistrationLock = new object();
        internal bool FcmTokenRegistration(string fcmToken)
        {
            try 
            {
                lock(tokenRegistrationLock)
                {
                    if(String.IsNullOrWhiteSpace(fcmToken))
                        return false;

                    // In a real app you should / could store your token here
                    FcmToken = fcmToken;

                    return true;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error executing FcmTokenRegistration()\n{ex}");
                return false;
            }
        }

        #endregion

        #region Application Lifecyle

        protected override void OnStart()
        {
            try 
            {
                CrossFirebaseCloudMessaging.Current.CheckIfValidAsync().Wait();

                CrossFirebaseCloudMessaging.Current.Error += (s, p) =>
                {
                    if(p == null)
                        return;

                    Console.WriteLine($"PushNotification.Error, Message = {p.Message}");
                };

                CrossFirebaseCloudMessaging.Current.TokenChanged += (s, p) =>
                {
                    if(p == null || String.IsNullOrWhiteSpace(p.Token))
                        return;

                    FcmTokenRegistration(p.Token);

                    // Create notification
                    //DependencyService.Get<INotificationService>().SendNotification(p.Notification.Title, p.Notification.Body);
                };

                CrossFirebaseCloudMessaging.Current.NotificationReceived += (s, p) =>
                {
                    if(p == null || p.Notification == null)
                        return;

                    // Create notification
                    //DependencyService.Get<INotificationService>().SendNotification(p.Notification.Title, p.Notification.Body);
                };

                CrossFirebaseCloudMessaging.Current.NotificationTapped += (s, p) =>
                {
                    if(p == null || p.Notification == null)
                        return;

                    Console.WriteLine($"PushNotification.NotificationTapped");
                };
            } 
            catch(Exception ex)
            {
                Console.WriteLine($"App.OnStart() crashed\n{ex}");
            }
        }

        protected override void OnSleep()
        {

        }

        protected override void OnResume()
        {

        }

        #endregion
    }
}
