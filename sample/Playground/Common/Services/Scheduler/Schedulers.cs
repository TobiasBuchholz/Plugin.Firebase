namespace Playground.Common.Services.Scheduler;

public static class Schedulers
{
    public static void Initialize(ISchedulerService schedulerService)
    {
        Main = schedulerService.Main.LogExceptions();
        TaskPool = schedulerService.TaskPool.LogExceptions();
    }

    public static IScheduler Main { get; private set; }
    public static IScheduler TaskPool { get; private set; }
}