using System.Reactive.Concurrency;

namespace Playground.Common.Services.Scheduler
{
    public interface ISchedulerService
    {
        IScheduler Main { get; }
        IScheduler TaskPool { get; }
    }
}