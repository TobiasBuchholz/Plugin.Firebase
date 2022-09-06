namespace Playground.Common.Services.Scheduler;

public sealed class SchedulerService : ISchedulerService
{
    public SchedulerService()
    {
        Main = RxApp.MainThreadScheduler.LogExceptions();
        TaskPool = RxApp.TaskpoolScheduler.LogExceptions();

    }

    public IScheduler Main { get; }
    public IScheduler TaskPool { get; }
}