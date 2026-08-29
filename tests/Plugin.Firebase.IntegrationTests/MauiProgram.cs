using DeviceRunners.UITesting;
using DeviceRunners.VisualRunners;

namespace Plugin.Firebase.IntegrationTests;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        IntegrationTestDiagnostics.WriteStartupConfiguration();

        var builder = MauiApp
            .CreateBuilder()
            .ConfigureUITesting()
            .UseVisualTestRunner(conf => {
                conf.AddCliConfiguration()
                    .AddConsoleResultChannel()
                    .AddTestAssembly(typeof(MauiProgram).Assembly)
                    .AddXunit();
            })
            .RegisterFirebaseServices();

        return builder.Build();
    }
}