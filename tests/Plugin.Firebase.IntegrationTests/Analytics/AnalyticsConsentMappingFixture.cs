using Plugin.Firebase.Analytics;

#if ANDROID
using Plugin.Firebase.Analytics.Platforms.Android.Extensions;
using NativeConsentStatus = Firebase.Analytics.FirebaseAnalytics.ConsentStatus;
using NativeConsentType = Firebase.Analytics.FirebaseAnalytics.ConsentType;
#elif IOS
using Plugin.Firebase.Analytics.Platforms.iOS.Extensions;
using NativeConsentStatus = Firebase.Analytics.ConsentStatus;
using NativeConsentType = Firebase.Analytics.ConsentType;
#endif

namespace Plugin.Firebase.IntegrationTests.Analytics
{
#if ANDROID || IOS
    [Preserve(AllMembers = true)]
    public sealed class AnalyticsConsentMappingFixture
    {
        [Fact]
        public void maps_every_consent_type_to_native_values()
        {
            foreach(var consentType in AllConsentTypes) {
                var nativeSettings = new Dictionary<ConsentType, ConsentStatus> {
                    { consentType, ConsentStatus.Granted }
                }.ToNativeConsentSettings();
                var nativeSetting = Assert.Single(nativeSettings);

                Assert.Equal(ExpectedNativeType(consentType), nativeSetting.Key);
                Assert.Equal(NativeConsentStatus.Granted, nativeSetting.Value);
            }
        }

        [Fact]
        public void maps_every_consent_status_to_native_values()
        {
            foreach(var consentStatus in AllConsentStatuses) {
                var nativeSettings = new Dictionary<ConsentType, ConsentStatus> {
                    { ConsentType.AdStorage, consentStatus }
                }.ToNativeConsentSettings();
                var nativeSetting = Assert.Single(nativeSettings);

                Assert.Equal(NativeConsentType.AdStorage, nativeSetting.Key);
                Assert.Equal(ExpectedNativeStatus(consentStatus), nativeSetting.Value);
            }
        }

        [Fact]
        public void maps_every_consent_type_status_pair_to_native_values()
        {
            foreach(var consentType in AllConsentTypes) {
                foreach(var consentStatus in AllConsentStatuses) {
                    AssertNativeSettingsEqual(new Dictionary<ConsentType, ConsentStatus> {
                        { consentType, consentStatus }
                    });
                }
            }
        }

        [Fact]
        public void full_all_granted_consent_settings_map_to_native_values()
        {
            AssertNativeSettingsEqual(CreateAllConsentSettings(ConsentStatus.Granted));
        }

        [Fact]
        public void full_all_denied_consent_settings_map_to_native_values()
        {
            AssertNativeSettingsEqual(CreateAllConsentSettings(ConsentStatus.Denied));
        }

        [Fact]
        public void alternating_consent_settings_map_to_native_values()
        {
            AssertNativeSettingsEqual(CreateAlternatingConsentSettings());
        }

        [Fact]
        public void single_entry_consent_settings_map_to_native_values()
        {
            foreach(var consentType in AllConsentTypes) {
                AssertNativeSettingsEqual(new Dictionary<ConsentType, ConsentStatus> {
                    { consentType, ConsentStatus.Denied }
                });
            }
        }

        [Fact]
        public void partial_consent_settings_preserve_only_supplied_values()
        {
            var consentSettings = new Dictionary<ConsentType, ConsentStatus> {
                { ConsentType.AnalyticsStorage, ConsentStatus.Granted },
                { ConsentType.AdPersonalization, ConsentStatus.Denied }
            };

            var nativeSettings = consentSettings.ToNativeConsentSettings();

            Assert.Equal(2, nativeSettings.Count);
            Assert.Equal(
                NativeConsentStatus.Granted,
                nativeSettings[NativeConsentType.AnalyticsStorage]
            );
            Assert.Equal(
                NativeConsentStatus.Denied,
                nativeSettings[NativeConsentType.AdPersonalization]
            );
            Assert.False(nativeSettings.ContainsKey(NativeConsentType.AdStorage));
            Assert.False(nativeSettings.ContainsKey(NativeConsentType.AdUserData));
        }

        [Fact]
        public void empty_consent_settings_map_to_empty_native_settings()
        {
            var nativeSettings = new Dictionary<ConsentType, ConsentStatus>().ToNativeConsentSettings();

            Assert.Empty(nativeSettings);
        }

        [Fact]
        public void conversion_returns_new_dictionary_decoupled_from_source()
        {
            var consentSettings = CreateAllConsentSettings(ConsentStatus.Denied);

            var nativeSettings = consentSettings.ToNativeConsentSettings();

            Assert.NotSame(consentSettings, nativeSettings);

            consentSettings[ConsentType.AdStorage] = ConsentStatus.Granted;

            Assert.Equal(NativeConsentStatus.Denied, nativeSettings[NativeConsentType.AdStorage]);
        }

        [Fact]
        public void non_dictionary_consent_settings_map_to_native_values()
        {
            IDictionary<ConsentType, ConsentStatus> consentSettings = new SortedList<ConsentType, ConsentStatus> {
                { ConsentType.AdStorage, ConsentStatus.Granted },
                { ConsentType.AnalyticsStorage, ConsentStatus.Denied },
                { ConsentType.AdUserData, ConsentStatus.Granted },
                { ConsentType.AdPersonalization, ConsentStatus.Denied }
            };

            AssertNativeSettingsEqual(consentSettings);
        }

        [Fact]
        public void invalid_consent_types_throw_argument_out_of_range_exception()
        {
            foreach(var invalidConsentType in new[] { (ConsentType) 999, (ConsentType) (-1) }) {
                var settings = new Dictionary<ConsentType, ConsentStatus> {
                    { invalidConsentType, ConsentStatus.Granted }
                };

                var exception = Assert.Throws<ArgumentOutOfRangeException>(
                    () => settings.ToNativeConsentSettings()
                );

                Assert.Equal("consentType", exception.ParamName);
                Assert.Equal(invalidConsentType, exception.ActualValue);
            }
        }

        [Fact]
        public void invalid_consent_statuses_throw_argument_out_of_range_exception()
        {
            foreach(var invalidConsentStatus in new[] { (ConsentStatus) 999, (ConsentStatus) (-1) }) {
                var settings = new Dictionary<ConsentType, ConsentStatus> {
                    { ConsentType.AdStorage, invalidConsentStatus }
                };

                var exception = Assert.Throws<ArgumentOutOfRangeException>(
                    () => settings.ToNativeConsentSettings()
                );

                Assert.Equal("consentStatus", exception.ParamName);
                Assert.Equal(invalidConsentStatus, exception.ActualValue);
            }
        }

        [Fact]
        public void invalid_consent_statuses_throw_for_every_valid_consent_type()
        {
            var invalidConsentStatus = (ConsentStatus) 999;

            foreach(var consentType in AllConsentTypes) {
                var settings = new Dictionary<ConsentType, ConsentStatus> {
                    { consentType, invalidConsentStatus }
                };

                var exception = Assert.Throws<ArgumentOutOfRangeException>(
                    () => settings.ToNativeConsentSettings()
                );

                Assert.Equal("consentStatus", exception.ParamName);
                Assert.Equal(invalidConsentStatus, exception.ActualValue);
            }
        }

        [Fact]
        public void mixed_valid_and_invalid_consent_type_settings_throw_argument_out_of_range_exception()
        {
            var invalidConsentType = (ConsentType) 999;
            var settings = new Dictionary<ConsentType, ConsentStatus> {
                { ConsentType.AdStorage, ConsentStatus.Granted },
                { invalidConsentType, ConsentStatus.Denied }
            };

            var exception = Assert.Throws<ArgumentOutOfRangeException>(
                () => settings.ToNativeConsentSettings()
            );

            Assert.Equal("consentType", exception.ParamName);
            Assert.Equal(invalidConsentType, exception.ActualValue);
        }

        [Fact]
        public void mixed_valid_and_invalid_consent_status_settings_throw_argument_out_of_range_exception()
        {
            var invalidConsentStatus = (ConsentStatus) 999;
            var settings = new Dictionary<ConsentType, ConsentStatus> {
                { ConsentType.AdStorage, ConsentStatus.Granted },
                { ConsentType.AnalyticsStorage, invalidConsentStatus }
            };

            var exception = Assert.Throws<ArgumentOutOfRangeException>(
                () => settings.ToNativeConsentSettings()
            );

            Assert.Equal("consentStatus", exception.ParamName);
            Assert.Equal(invalidConsentStatus, exception.ActualValue);
        }

        [Fact]
        public void null_consent_settings_throw_argument_null_exception()
        {
            IDictionary<ConsentType, ConsentStatus> settings = null;

            var exception = Assert.Throws<ArgumentNullException>(
                () => settings.ToNativeConsentSettings()
            );

            Assert.Equal("consentSettings", exception.ParamName);
        }

        private static ConsentType[] AllConsentTypes => new[] {
            ConsentType.AdStorage,
            ConsentType.AnalyticsStorage,
            ConsentType.AdUserData,
            ConsentType.AdPersonalization
        };

        private static ConsentStatus[] AllConsentStatuses => new[] {
            ConsentStatus.Granted,
            ConsentStatus.Denied
        };

        private static Dictionary<ConsentType, ConsentStatus> CreateAllConsentSettings(
            ConsentStatus consentStatus
        )
        {
            return AllConsentTypes.ToDictionary(
                consentType => consentType,
                _ => consentStatus
            );
        }

        private static Dictionary<ConsentType, ConsentStatus> CreateAlternatingConsentSettings()
        {
            return new Dictionary<ConsentType, ConsentStatus> {
                { ConsentType.AdStorage, ConsentStatus.Granted },
                { ConsentType.AnalyticsStorage, ConsentStatus.Denied },
                { ConsentType.AdUserData, ConsentStatus.Granted },
                { ConsentType.AdPersonalization, ConsentStatus.Denied }
            };
        }

        private static void AssertNativeSettingsEqual(
            IDictionary<ConsentType, ConsentStatus> consentSettings
        )
        {
            var nativeSettings = consentSettings.ToNativeConsentSettings();

            Assert.Equal(consentSettings.Count, nativeSettings.Count);

            foreach(var (consentType, consentStatus) in consentSettings) {
                Assert.True(nativeSettings.ContainsKey(ExpectedNativeType(consentType)));
                Assert.Equal(
                    ExpectedNativeStatus(consentStatus),
                    nativeSettings[ExpectedNativeType(consentType)]
                );
            }

            foreach(var omittedConsentType in AllConsentTypes.Except(consentSettings.Keys)) {
                Assert.False(nativeSettings.ContainsKey(ExpectedNativeType(omittedConsentType)));
            }
        }

        private static NativeConsentType ExpectedNativeType(ConsentType consentType)
        {
            return consentType switch {
                ConsentType.AdStorage => NativeConsentType.AdStorage,
                ConsentType.AnalyticsStorage => NativeConsentType.AnalyticsStorage,
                ConsentType.AdUserData => NativeConsentType.AdUserData,
                ConsentType.AdPersonalization => NativeConsentType.AdPersonalization,
                _ => throw new ArgumentOutOfRangeException(nameof(consentType), consentType, null)
            };
        }

        private static NativeConsentStatus ExpectedNativeStatus(ConsentStatus consentStatus)
        {
            return consentStatus switch {
                ConsentStatus.Granted => NativeConsentStatus.Granted,
                ConsentStatus.Denied => NativeConsentStatus.Denied,
                _ => throw new ArgumentOutOfRangeException(nameof(consentStatus), consentStatus, null)
            };
        }
    }
#endif
}