using Plugin.Firebase.Functions;
using Xunit.Sdk;

namespace Plugin.Firebase.IntegrationTests.Functions
{
    [Collection("Sequential")]
    [TestLogging]
    [Preserve(AllMembers = true)]
    public sealed class FunctionsFixture
    {
        private const string RegionalFunctionsRegion = "southamerica-east1";
        private const string RegionalPingFunctionName = "regionalPing";
        private const long RegionalPingOutputValue = 541;

        [Fact]
        public async Task executes_simple_callable_function()
        {
            var sut = CrossFirebaseFunctions.Current;
            await sut.GetHttpsCallable("convertToLeet").CallAsync();
        }

        [Fact]
        public async Task executes_callable_function_with_json_body()
        {
            var sut = CrossFirebaseFunctions.Current;
            var json = new SimpleRequestData(123).ToJson();
            await sut.GetHttpsCallable("convertToLeet").CallAsync(json);
        }

        [Fact]
        public async Task executes_callable_function_with_json_body_and_response()
        {
            var sut = CrossFirebaseFunctions.Current;
            var json = new SimpleRequestData(123).ToJson();
            var response = await sut.GetHttpsCallable("convertToLeet").CallAsync<SimpleResponseData>(json);

            Assert.Equal(123, response.InputValue);
            Assert.Equal(1337, response.OutputValue);
        }

        [Fact]
        public async Task throws_exception_when_function_does_not_exist()
        {
            var sut = CrossFirebaseFunctions.Current;
            await Assert.ThrowsAnyAsync<Exception>(() => sut.GetHttpsCallable("doesNotExist").CallAsync());
        }

        [Fact]
        public async Task uses_configured_region_when_initialize_runs_after_emulator_configuration()
        {
            SkipIfRealBackend();

            try {
                ResetFunctionsToDefaultRegion();
                ConfigureFunctionsEmulator();

                CrossFirebaseFunctions.Initialize(RegionalFunctionsRegion);

                var response = await CrossFirebaseFunctions.Current
                    .GetHttpsCallable(RegionalPingFunctionName)
                    .CallAsync<SimpleResponseData>("{}");

                Assert.Equal(RegionalPingOutputValue, response.OutputValue);
            } finally {
                RestoreDefaultFunctionsConfiguration();
            }
        }

        [Fact]
        public async Task uses_configured_region_after_is_supported_was_checked()
        {
            SkipIfRealBackend();

            try {
                ResetFunctionsToDefaultRegion();
                Assert.True(CrossFirebaseFunctions.IsSupported);

                CrossFirebaseFunctions.Initialize(RegionalFunctionsRegion);
                ConfigureFunctionsEmulator();

                var response = await CrossFirebaseFunctions.Current
                    .GetHttpsCallable(RegionalPingFunctionName)
                    .CallAsync<SimpleResponseData>("{}");

                Assert.Equal(RegionalPingOutputValue, response.OutputValue);
            } finally {
                RestoreDefaultFunctionsConfiguration();
            }
        }

        private static void SkipIfRealBackend()
        {
            if(IntegrationTestEnvironment.UsesRealBackend) {
                throw SkipException.ForSkip(
                    "This test uses the emulator-only regional function fixture.");
            }
        }

        private static void RestoreDefaultFunctionsConfiguration()
        {
            ResetFunctionsToDefaultRegion();
            ConfigureFunctionsEmulator();
        }

        private static void ResetFunctionsToDefaultRegion()
        {
            CrossFirebaseFunctions.Initialize(null);
            CrossFirebaseFunctions.Dispose();
        }

        private static void ConfigureFunctionsEmulator()
        {
            var functions = IntegrationTestEnvironment.FunctionsEmulatorEndpoint;
            CrossFirebaseFunctions.Current.UseEmulator(functions.Host, functions.Port);
        }
    }
}
