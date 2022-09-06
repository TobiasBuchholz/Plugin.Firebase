using Plugin.Firebase.RemoteConfig;

namespace Plugin.Firebase.IntegrationTests.RemoteConfig
{
    [Preserve(AllMembers = true)]
    public sealed class RemoteConfigFixture
    {
        [Fact]
        public async Task ensures_it_is_initialized()
        {
            await CrossFirebaseRemoteConfig.Current.EnsureInitializedAsync();
        }

        [Fact]
        public async Task sets_defaults_via_tuples()
        {
            var sut = CrossFirebaseRemoteConfig.Current;
            var millis = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            await sut.SetDefaultsAsync(
                ("some_string", millis.ToString()),
                ("some_long", millis),
                ("some_double", millis),
                ("some_bool", millis % 2 == 0));

            Assert.Equal(millis.ToString(), sut.GetString("some_string"));
            Assert.Equal(millis, sut.GetLong("some_long"));
            Assert.Equal(millis, sut.GetDouble("some_double"));
            Assert.Equal(millis % 2 == 0, sut.GetBoolean("some_bool"));
        }

        [Fact]
        public async Task sets_defaults_via_dictionary()
        {
            var sut = CrossFirebaseRemoteConfig.Current;
            var millis = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            await sut.SetDefaultsAsync(new Dictionary<string, object> {
                { "some_string", millis.ToString() },
                { "some_long", millis },
                { "some_double", millis },
                { "some_bool", millis%2 == 0 }
            });

            Assert.Equal(millis.ToString(), sut.GetString("some_string"));
            Assert.Equal(millis, sut.GetLong("some_long"));
            Assert.Equal(millis, sut.GetDouble("some_double"));
            Assert.Equal(millis % 2 == 0, sut.GetBoolean("some_bool"));
        }

        [Fact]
        public async Task fetches_and_activates_remote_config_at_once()
        {
            var sut = CrossFirebaseRemoteConfig.Current;

            await sut.SetDefaultsAsync(
                ("remote_string", "default_value"),
                ("remote_long", 123L),
                ("remote_double", 12.3),
                ("remote_bool", false));

            await sut.SetRemoteConfigSettingsAsync(new RemoteConfigSettings(minimumFetchInterval: TimeSpan.Zero));
            await sut.FetchAndActivateAsync();

            Assert.Equal("remote_value", sut.GetString("remote_string"));
            Assert.Equal(1337L, sut.GetLong("remote_long"));
            Assert.Equal(13.37, sut.GetDouble("remote_double"));
            Assert.True(sut.GetBoolean("remote_bool"));
        }

        [Fact]
        public async Task fetches_and_activates_remote_config_separately()
        {
            var sut = CrossFirebaseRemoteConfig.Current;

            await sut.SetDefaultsAsync(
                ("remote_string", "default_value"),
                ("remote_long", 123L),
                ("remote_double", 12.3),
                ("remote_bool", false));

            try {
                await sut.FetchAsync(0);
                await sut.ActivateAsync();
            } catch(Exception e) {
                Assert.Equal(GetExpectedActivationErrorMessage(), e.Message);
            }

            Assert.Equal("remote_value", sut.GetString("remote_string"));
            Assert.Equal(1337L, sut.GetLong("remote_long"));
            Assert.Equal(13.37, sut.GetDouble("remote_double"));
            Assert.True(sut.GetBoolean("remote_bool"));
        }

        private static string GetExpectedActivationErrorMessage()
        {
            return DeviceInfo.Platform == DevicePlatform.iOS
                ? "Error Domain=com.google.remoteconfig.ErrorDomain Code=8003 \"(null)\" UserInfo={ActivationFailureReason=Most recently fetched config already activated}"
                : "Android shouldn't throw an exception";
        }

        [Fact]
        public void gets_keys_by_prefix()
        {
            var keys = CrossFirebaseRemoteConfig.Current.GetKeysByPrefix("remote").ToList();
            Assert.Equal(4, keys.Count());
            Assert.Contains("remote_string", keys);
            Assert.Contains("remote_long", keys);
            Assert.Contains("remote_double", keys);
            Assert.Contains("remote_bool", keys);
        }

        [Fact]
        public async Task gets_info()
        {
            var sut = CrossFirebaseRemoteConfig.Current;
            var configSettings = new RemoteConfigSettings(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30));

            await sut.SetRemoteConfigSettingsAsync(configSettings);
            await sut.FetchAsync();

            var info = sut.Info;
            Assert.Equal(configSettings, info.ConfigSettings);
            Assert.Equal(RemoteConfigFetchStatus.Success, info.LastFetchStatus);
        }
    }
}