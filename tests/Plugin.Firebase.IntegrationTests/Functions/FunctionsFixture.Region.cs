using System.Text.Json;
using Plugin.Firebase.Auth;
using Plugin.Firebase.Functions;

namespace Plugin.Firebase.IntegrationTests.Functions;

public sealed partial class FunctionsFixture
{
    [EmulatorBackendFact]
    public async Task uses_configured_region_when_initialize_runs_after_emulator_configuration()
    {
        try {
            ResetFunctionsToDefaultRegion();
            ConfigureFunctionsEmulator();

            CrossFirebaseFunctions.Initialize(RegionalFunctionsRegion);

            var response = await CrossFirebaseFunctions.Current
                .GetHttpsCallable(RegionalPingFunctionName)
                .CallAsync<SimpleResponseData>("{}");

            Assert.Equal(RegionalPingOutputValue, response.OutputValue);
        }
        finally {
            RestoreDefaultFunctionsConfiguration();
        }
    }


    [EmulatorBackendFact]
    public async Task uses_configured_region_after_is_supported_was_checked()
    {
        try {
            ResetFunctionsToDefaultRegion();
            Assert.True(CrossFirebaseFunctions.IsSupported);

            CrossFirebaseFunctions.Initialize(RegionalFunctionsRegion);
            ConfigureFunctionsEmulator();

            var response = await CrossFirebaseFunctions.Current
                .GetHttpsCallable(RegionalPingFunctionName)
                .CallAsync<SimpleResponseData>("{}");

            Assert.Equal(RegionalPingOutputValue, response.OutputValue);
        }
        finally {
            RestoreDefaultFunctionsConfiguration();
        }
    }

}