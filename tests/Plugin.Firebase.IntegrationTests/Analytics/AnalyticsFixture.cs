using Plugin.Firebase.Analytics;

namespace Plugin.Firebase.IntegrationTests.Analytics
{
    [Collection("Sequential")]
    [TestLogging]
    [Preserve(AllMembers = true)]
    public sealed class AnalyticsFixture
    {
#if ANDROID
        [Fact]
        public void throws_actionable_exception_when_android_analytics_is_not_initialized()
        {
            var firebaseAnalyticsField = typeof(FirebaseAnalyticsImplementation).GetField(
                "_firebaseAnalytics",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static
            );
            Assert.NotNull(firebaseAnalyticsField);

            var originalFirebaseAnalytics = firebaseAnalyticsField.GetValue(null);
            try {
                firebaseAnalyticsField.SetValue(null, null);

                var logEventException = Assert.Throws<InvalidOperationException>(
                    () => CrossFirebaseAnalytics.Current.LogEvent("test_uninitialized_analytics_guard")
                );
                AssertAndroidAnalyticsNotInitializedException(logEventException);

                var setConsentException = Assert.Throws<InvalidOperationException>(
                    () => CrossFirebaseAnalytics.Current.SetConsent(new Dictionary<ConsentType, ConsentStatus> {
                        { ConsentType.AnalyticsStorage, ConsentStatus.Granted }
                    })
                );
                AssertAndroidAnalyticsNotInitializedException(setConsentException);
            }
            finally {
                firebaseAnalyticsField.SetValue(null, originalFirebaseAnalytics);
            }
        }
#endif

        [RealFirebaseFact]
        public void does_not_throw_any_exception_when_logging_events()
        {
            var sut = CrossFirebaseAnalytics.Current;

            sut.LogEvent("test_without_parameters");
            sut.LogEvent("test_with_single_tuple_parameter", ("some_parameter", "some_value"));

            sut.LogEvent("test_with_multiple_tuple_parameters",
                ("some_string", "some_value"),
                ("some_int", 1337),
                ("some_long", 1337L),
                ("some_double", 13.37),
                ("some_float", 133.7f),
                ("some_bool", true),
                ("some_dictionary", new Dictionary<string, object> { { "some_string", "some_value" } }),
                ("some_dictionary_collection", new[] { new Dictionary<string, object> { { "dict_string", "dict_value" } } })
            );

            sut.LogEvent("test_with_dictionary", new Dictionary<string, object> {
                { "some_string", "some_value" },
                { "some_int", 1337 },
                { "some_long", 1337L },
                { "some_double", 13.37 },
                { "some_float", 133.7f },
                { "some_bool", true },
                { "some_dictionary", new Dictionary<string, object> { { "some_key", "some_value" } } },
                { "some_dictionary_collection", new [] { new Dictionary<string, object> { { "some_key", "some_value" } } }}
            });
        }

        [RealFirebaseFact]
        public void does_not_throw_any_exception_when_setting_user_properties()
        {
            var sut = CrossFirebaseAnalytics.Current;
            sut.SetUserId("some_id");
            sut.SetUserProperty("some_name", "some_value");
        }

        [RealFirebaseFact]
        public void does_not_throw_any_exception_when_setting_consent()
        {
            var sut = CrossFirebaseAnalytics.Current;

            try {
                sut.SetConsent(new Dictionary<ConsentType, ConsentStatus> {
                    { ConsentType.AnalyticsStorage, ConsentStatus.Granted },
                    { ConsentType.AdStorage, ConsentStatus.Denied },
                    { ConsentType.AdUserData, ConsentStatus.Granted },
                    { ConsentType.AdPersonalization, ConsentStatus.Denied }
                });
            }
            finally {
                sut.SetConsent(new Dictionary<ConsentType, ConsentStatus> {
                    { ConsentType.AnalyticsStorage, ConsentStatus.Granted },
                    { ConsentType.AdStorage, ConsentStatus.Granted },
                    { ConsentType.AdUserData, ConsentStatus.Granted },
                    { ConsentType.AdPersonalization, ConsentStatus.Granted }
                });
            }
        }

        [RealFirebaseFact]
        public void does_not_throw_any_exception_when_setting_empty_consent()
        {
            CrossFirebaseAnalytics.Current.SetConsent(new Dictionary<ConsentType, ConsentStatus>());
        }

        [RealFirebaseFact]
        public void does_not_throw_any_exception_when_setting_all_granted_consent()
        {
            CrossFirebaseAnalytics.Current.SetConsent(CreateAllConsentSettings(ConsentStatus.Granted));
        }

        [RealFirebaseFact]
        public void does_not_throw_any_exception_when_setting_all_denied_consent()
        {
            SetConsentAndRestore(CreateAllConsentSettings(ConsentStatus.Denied));
        }

        [RealFirebaseFact]
        public void does_not_throw_any_exception_when_setting_single_consent_values()
        {
            foreach(var consentType in AllConsentTypes) {
                foreach(var consentStatus in AllConsentStatuses) {
                    SetConsentAndRestore(new Dictionary<ConsentType, ConsentStatus> {
                        { consentType, consentStatus }
                    });
                }
            }
        }

        [RealFirebaseFact]
        public void does_not_throw_any_exception_when_setting_partial_consent()
        {
            SetConsentAndRestore(new Dictionary<ConsentType, ConsentStatus> {
                { ConsentType.AdStorage, ConsentStatus.Denied },
                { ConsentType.AdPersonalization, ConsentStatus.Granted }
            });
        }

        [RealFirebaseFact]
        public void throws_argument_null_exception_when_setting_null_consent()
        {
            IDictionary<ConsentType, ConsentStatus> consentSettings = null;

            var exception = Assert.Throws<ArgumentNullException>(
                () => CrossFirebaseAnalytics.Current.SetConsent(consentSettings)
            );

            Assert.Equal("consentSettings", exception.ParamName);
        }

        [RealFirebaseFact]
        public void throws_argument_out_of_range_exception_when_setting_invalid_consent_type()
        {
            var invalidConsentType = (ConsentType) 999;
            var consentSettings = new Dictionary<ConsentType, ConsentStatus> {
                { invalidConsentType, ConsentStatus.Granted }
            };

            var exception = Assert.Throws<ArgumentOutOfRangeException>(
                () => CrossFirebaseAnalytics.Current.SetConsent(consentSettings)
            );

            Assert.Equal("consentType", exception.ParamName);
            Assert.Equal(invalidConsentType, exception.ActualValue);
        }

        [RealFirebaseFact]
        public void throws_argument_out_of_range_exception_when_setting_invalid_consent_status()
        {
            var invalidConsentStatus = (ConsentStatus) 999;
            var consentSettings = new Dictionary<ConsentType, ConsentStatus> {
                { ConsentType.AdStorage, invalidConsentStatus }
            };

            var exception = Assert.Throws<ArgumentOutOfRangeException>(
                () => CrossFirebaseAnalytics.Current.SetConsent(consentSettings)
            );

            Assert.Equal("consentStatus", exception.ParamName);
            Assert.Equal(invalidConsentStatus, exception.ActualValue);
        }

        [RealFirebaseFact]
        public async Task does_not_throw_any_exception_when_getting_app_instance_id()
        {
            var sut = CrossFirebaseAnalytics.Current;
            Assert.NotNull(await sut.GetAppInstanceIdAsync());
        }

        [RealFirebaseFact]
        public void does_not_throw_any_exception_at_other_methods()
        {
            var sut = CrossFirebaseAnalytics.Current;
            sut.IsAnalyticsCollectionEnabled = true;
            sut.SetSessionTimoutDuration(TimeSpan.FromSeconds(90));
            sut.ResetAnalyticsData();
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

        private static void SetConsentAndRestore(IDictionary<ConsentType, ConsentStatus> consentSettings)
        {
            try {
                CrossFirebaseAnalytics.Current.SetConsent(consentSettings);
            }
            finally {
                CrossFirebaseAnalytics.Current.SetConsent(CreateAllConsentSettings(ConsentStatus.Granted));
            }
        }

        private static void AssertAndroidAnalyticsNotInitializedException(InvalidOperationException exception)
        {
            Assert.Contains("Firebase Analytics has not been initialized on Android", exception.Message);
            Assert.Contains("FirebaseAnalyticsImplementation.Initialize(activity)", exception.Message);
            Assert.Contains("isAnalyticsEnabled: true", exception.Message);
        }
    }
}
