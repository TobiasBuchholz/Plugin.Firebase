namespace Plugin.Firebase.IntegrationTests;

internal static class IntegrationTestTasks
{
    public static async Task<T> WaitForTestAsync<T>(
        this Task<T> task,
        TimeSpan timeout,
        string operationName)
    {
        try {
            return await task.WaitAsync(timeout);
        } catch(TimeoutException e) {
            throw new TimeoutException(
                $"Timed out waiting for {operationName} after {timeout}.",
                e);
        }
    }

    public static async Task WaitForTestAsync(
        this Task task,
        TimeSpan timeout,
        string operationName)
    {
        try {
            await task.WaitAsync(timeout);
        } catch(TimeoutException e) {
            throw new TimeoutException(
                $"Timed out waiting for {operationName} after {timeout}.",
                e);
        }
    }

    public static async Task EventuallyAsync(
        Action assertion,
        TimeSpan timeout,
        TimeSpan? pollInterval = null)
    {
        var interval = pollInterval ?? IntegrationTestTimeouts.PollInterval;
        var deadline = DateTimeOffset.UtcNow.Add(timeout);

        while(DateTimeOffset.UtcNow < deadline) {
            try {
                assertion();
                return;
            } catch(Exception e) when(IsAssertionException(e)) {
                await Task.Delay(interval);
            }
        }

        assertion();
    }

    private static bool IsAssertionException(Exception exception)
    {
        var type = exception.GetType();
        return type.Namespace == "Xunit.Sdk"
            || type.FullName?.StartsWith("Xunit.Sdk.", StringComparison.Ordinal) == true;
    }
}