using System;
using Genesis.Logging;

namespace System.Reactive.Concurrency
{
    public static class SchedulerExtensions
    {
        public static IScheduler LogExceptions(this IScheduler @this)
        {
            return @this.Catch<Exception>(e => 
            {
                var logger = LoggerService.GetLogger(@this.GetType());
                logger.Error(e, "An exception was caught:\n");
                return false;
            });
        }
    }
}
