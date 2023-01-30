using Plugin.Firebase.RemoteConfig;
using Plugin.Firebase.Storage;

namespace Playground;

public partial class MainPage : ContentPage
{
    int count = 0;

    public MainPage()
    {
        InitializeComponent();
    }

    private void OnCounterClicked(object sender, EventArgs e)
    {
        count++;

        if(count == 1)
            CounterBtn.Text = $"Clicked {count} time";
        else
            CounterBtn.Text = $"Clicked {count} times";
        
        TryTestRemoteConfigAsync();
        TryTestStorage();
    }

    private async Task TryTestRemoteConfigAsync()
    {
        try {
            await TestRemoteConfigAsync();
        } catch(Exception e) {
            Console.WriteLine(e);
        }
    }

    private async Task TestRemoteConfigAsync()
    {
        var sut = CrossFirebaseRemoteConfig.Current;
        await sut.SetDefaultsAsync(
            ("remote_string", "default_value"),
            ("remote_long", 123L),
            ("remote_double", 12.3),
            ("remote_bool", false));

        await sut.SetRemoteConfigSettingsAsync(new RemoteConfigSettings(minimumFetchInterval: TimeSpan.Zero));
        await sut.FetchAndActivateAsync();

        var assert1 = sut.GetString("remote_string");
        var assert2 = sut.GetLong("remote_long");
        var assert3 = sut.GetDouble("remote_double");
        var assert4 = sut.GetBoolean("remote_bool");
    }

    private void TryTestStorage()
    {
        try {
            TestStorage();
        } catch(Exception e) {
            Console.WriteLine(e);
        }
    }

    private void TestStorage()
    {
        var reference = CrossFirebaseStorage.Current.GetRootReference();
        System.Diagnostics.Debug.WriteLine($"{reference}");
    }
}