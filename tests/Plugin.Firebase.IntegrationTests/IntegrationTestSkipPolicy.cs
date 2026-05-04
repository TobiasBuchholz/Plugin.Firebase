namespace Plugin.Firebase.IntegrationTests;

internal static class IntegrationTestSkipReasons
{
    public const string EmulatorBackendRequired =
        "This test requires Firebase emulators. Set PLUGIN_FIREBASE_TEST_BACKEND=emulator to run it.";
    public const string RealBackendRequired =
        "This test requires a real Firebase project. Set PLUGIN_FIREBASE_TEST_BACKEND=real to run it.";
    public const string AndroidRequired = "This test only applies to Android.";
    public const string IosRequired = "This test only applies to iOS.";
    public const string IosDeviceRequired = "This test requires a real iOS device with APNs configured.";
    public const string IosSimulatorUnsupported = "This test does not run on the iOS simulator.";
}

internal static class IntegrationTestSkipPolicy
{
    public static string? RequireEmulatorBackend()
    {
        return IntegrationTestEnvironment.UsesRealBackend
            ? IntegrationTestSkipReasons.EmulatorBackendRequired
            : null;
    }

    public static string? RequireRealBackend()
    {
        return IntegrationTestEnvironment.UsesEmulatorBackend
            ? IntegrationTestSkipReasons.RealBackendRequired
            : null;
    }

    public static string? RequireRealBackendOptIn(
        string environmentVariableName,
        string? androidSystemPropertyName,
        bool skipIosSimulator)
    {
        return RequireRealBackend()
            ?? RequireIosDeviceWhen(skipIosSimulator)
            ?? RequireOptIn(environmentVariableName, androidSystemPropertyName, realBackend: true);
    }

    public static string? RequireOptIn(
        string environmentVariableName,
        string? androidSystemPropertyName)
    {
        return RequireOptIn(environmentVariableName, androidSystemPropertyName, realBackend: false);
    }

    public static string? RequireAndroid()
    {
        return OperatingSystem.IsAndroid()
            ? null
            : IntegrationTestSkipReasons.AndroidRequired;
    }

    public static string? RequireIos()
    {
        return OperatingSystem.IsIOS()
            ? null
            : IntegrationTestSkipReasons.IosRequired;
    }

    public static string? RequireIosDevice()
    {
        return RequireIos() ?? RequireIosDeviceWhen(skipIosSimulator: true);
    }

    public static string? RequireNonIosSimulator()
    {
        return IsIosSimulator()
            ? IntegrationTestSkipReasons.IosSimulatorUnsupported
            : null;
    }

    private static string? RequireIosDeviceWhen(bool skipIosSimulator)
    {
        return skipIosSimulator && IsIosSimulator()
            ? IntegrationTestSkipReasons.IosDeviceRequired
            : null;
    }

    private static string? RequireOptIn(
        string environmentVariableName,
        string? androidSystemPropertyName,
        bool realBackend)
    {
        if(IntegrationTestEnvironment.IsFeatureEnabled(environmentVariableName, androidSystemPropertyName)) {
            return null;
        }

        var backendScope = realBackend
            ? " opt-in real Firebase"
            : " opt-in integration";
        return $"Set {environmentVariableName}=1 to run this{backendScope} test.";
    }

    private static bool IsIosSimulator()
    {
        return OperatingSystem.IsIOS()
            && DeviceInfo.DeviceType == DeviceType.Virtual;
    }
}