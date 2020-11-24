using System.Reactive.Concurrency;

namespace Playground.Common.Services.Scheduler
{
    public static class Schedulers
    {
        public static void Initialize(ISchedulerService schedulerService)
        {
            Main = schedulerService.Main;
            TaskPool = schedulerService.TaskPool;
        }

        public static IScheduler Main { get; private set; }
        public static IScheduler TaskPool { get; private set; }
    }
}