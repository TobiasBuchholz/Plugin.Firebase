using System.Reflection;
using Xunit.Sdk;

[assembly: Plugin.Firebase.IntegrationTests.TestLogging]

namespace Plugin.Firebase.IntegrationTests;

/// <summary>
/// Writes per-test progress to the simulator console so local runs can identify
/// the last test started/completed when the visual runner hangs or fails.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method)]
public sealed class TestLoggingAttribute : BeforeAfterTestAttribute
{
    public override void Before(MethodInfo methodUnderTest)
    {
        TestLog.Write(
            $"[TEST START] {DateTimeOffset.UtcNow:O} {methodUnderTest.DeclaringType?.FullName}.{methodUnderTest.Name}"
        );
    }

    public override void After(MethodInfo methodUnderTest)
    {
        TestLog.Write(
            $"[TEST END] {DateTimeOffset.UtcNow:O} {methodUnderTest.DeclaringType?.FullName}.{methodUnderTest.Name}"
        );
    }
}

internal static class TestLog
{
    private static readonly Lock Sync = new();

    public static void Write(string message)
    {
        Console.WriteLine(message);
        System.Diagnostics.Debug.WriteLine(message);
        AppendToFile(message);
    }

    private static void AppendToFile(string message)
    {
        try {
            var logPath = Path.Combine(FileSystem.CacheDirectory, "plugin-firebase-it.log");
            lock(Sync) {
                File.AppendAllText(logPath, message + Environment.NewLine);
            }
        } catch {
            // Best-effort diagnostics only.
        }
    }
}