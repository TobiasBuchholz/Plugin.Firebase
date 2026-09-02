namespace Plugin.Firebase.IntegrationTests;

internal static class IntegrationTestDiagnostics
{
    public static void WriteStartupConfiguration()
    {
        var runner = Environment.GetEnvironmentVariable("DEVICE_RUNNERS_AUTORUN") == "1"
            ? "dotnet-test"
            : "visual";

        TestLog.Write("[INTEGRATION CONFIG] "
            + $"backend={IntegrationTestEnvironment.Backend}; "
            + $"runner={runner}; "
            + $"platform={DeviceInfo.Platform}; "
            + $"deviceType={DeviceInfo.DeviceType}; "
            + $"appId={AppInfo.PackageName}; "
            + $"authEmulator={FormatEndpoint(IntegrationTestEnvironment.AuthEmulatorEndpoint)}; "
            + $"firestoreEmulator={FormatEndpoint(IntegrationTestEnvironment.FirestoreEmulatorEndpoint)}; "
            + $"functionsEmulator={FormatEndpoint(IntegrationTestEnvironment.FunctionsEmulatorEndpoint)}; "
            + $"storageEmulator={FormatEndpoint(IntegrationTestEnvironment.StorageEmulatorEndpoint)}; "
            + $"appCheckTokenOptIn={IntegrationTestEnvironment.ShouldRunAppCheckTokenTests}; "
            + $"fcmTokenOptIn={IsEnabled(IntegrationTestOptions.RunFcmTokenTestsEnvironmentVariableName)}; "
            + $"fcmDeliveryOptIn={IsEnabled(IntegrationTestOptions.RunFcmDeliveryTestsEnvironmentVariableName)}; "
            + $"crashlyticsForceCrashOptIn={IsEnabled(IntegrationTestOptions.ForceCrashlyticsCrashEnvironmentVariableName)}; "
            + $"crashlyticsPreviousCrashOptIn={IsEnabled(IntegrationTestOptions.ExpectPreviousCrashEnvironmentVariableName)}; "
            + $"installationsDeleteOptIn={IsEnabled(IntegrationTestOptions.RunInstallationsDeleteTestsEnvironmentVariableName)}; "
            + "phoneAuthOptIn="
            + IntegrationTestEnvironment.IsFeatureEnabled(
                IntegrationTestOptions.RunPhoneAuthTestsEnvironmentVariableName,
                IntegrationTestOptions.RunPhoneAuthTestsAndroidSystemPropertyName));
    }

    private static string FormatEndpoint(EmulatorEndpoint endpoint)
    {
        return $"{endpoint.Host}:{endpoint.Port}";
    }

    private static bool IsEnabled(string environmentVariableName)
    {
        return IntegrationTestEnvironment.IsFeatureEnabled(environmentVariableName, null);
    }
}