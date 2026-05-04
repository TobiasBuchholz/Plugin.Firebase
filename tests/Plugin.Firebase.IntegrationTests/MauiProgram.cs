using DeviceRunners.UITesting;
using DeviceRunners.VisualRunners;
using DeviceRunners.XHarness;

namespace Plugin.Firebase.IntegrationTests;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var useVisualRunner = IntegrationTestEnvironment.IsFeatureEnabled(
            IntegrationTestOptions.UseVisualRunnerEnvironmentVariableName,
            IntegrationTestOptions.UseVisualRunnerAndroidSystemPropertyName);
        IntegrationTestDiagnostics.WriteStartupConfiguration(useVisualRunner);

        var builder = MauiApp
            .CreateBuilder()
            .ConfigureUITesting()
            .UseVisualTestRunner(conf => {
                conf.SetTestRunnerUsage(useVisualRunner ? VisualTestRunnerUsage.Always : VisualTestRunnerUsage.Never);

                conf.AddConsoleResultChannel()
                    .AddTestAssembly(typeof(MauiProgram).Assembly)
                    .AddXunit();
            })
            .UseXHarnessTestRunner(conf => {
                conf.SetTestRunnerUsage(
                    useVisualRunner ? XHarnessTestRunnerUsage.Never : XHarnessTestRunnerUsage.Always);

                conf.AddTestAssembly(typeof(MauiProgram).Assembly)
                    .AddXunit();
            })
            .RegisterFirebaseServices();

        return builder.Build();
    }
}