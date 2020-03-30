using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CallStrategy
{
    public class InfiniteRetriableAsyncCallStrategy : IRetriableAsyncCallStrategy
    {
        private const double PauseRandomDeviation = 0.1;
        private const double PauseIncreaseStep = 1.15;
        
        private readonly long tolerableMemoryLeakSize;
        private readonly TimeSpan pauseBetweenCallsOnStart = TimeSpan.FromSeconds(1);
        private readonly TimeSpan maxPauseBetweenCalls = TimeSpan.FromMinutes(5);
        
        public InfiniteRetriableAsyncCallStrategy(long tolerableMemoryLeakSize)
        {
            this.tolerableMemoryLeakSize = tolerableMemoryLeakSize;
        }

        public async Task CallAsync(Func<Task> action, Func<Exception, bool> isExceptionRetriable, Action beforeRetry = null)
        {
            var exceptions = new SortedSet<Exception>(new ObstinateCallExceptionComparer());
            long memoryUsageAtFirstFail = 0;
            var pause = pauseBetweenCallsOnStart;
            for (var failedTries = 0;;)
            {
                try
                {
                    if (failedTries > 0)
                        beforeRetry?.Invoke();
                    await action().ConfigureAwait(false);
                    return;
                }
                catch (Exception ex)
                {
                    ++failedTries;
                    if (ExceptionFinder.FindException(ex, isExceptionRetriable) == null)
                        throw;
                    if (!exceptions.Contains(ex))
                        exceptions.Add(ex);
                    var currentMemory = GC.GetTotalMemory(true);
                    if (failedTries == 1)
                        memoryUsageAtFirstFail = currentMemory;
                    if (currentMemory > memoryUsageAtFirstFail + tolerableMemoryLeakSize)
                    {
                        throw new AttemptsExceededException(ex.Message, exceptions);
                    }

                    await Task.Delay(pause).ConfigureAwait(false);
                    pause = GetNextPause(pause);
                }
            }
        }

        public async Task<T> CallAsync<T>(Func<Task<T>> action, Func<Exception, bool> isExceptionRetriable, Action beforeRetry = null)
        {
            var result = default(T);
            await CallAsync((Func<Task>) (async () => result = await action().ConfigureAwait(false)), isExceptionRetriable, beforeRetry).ConfigureAwait(false);
            return result;
        }
        
        private TimeSpan GetNextPause(TimeSpan pause)
        {
            var random = new Random();
            pause = TimeSpan.FromMilliseconds(pause.TotalMilliseconds * PauseIncreaseStep);
            pause = TimeSpan.FromTicks(Math.Min(pause.Ticks, maxPauseBetweenCalls.Ticks));
            return TimeSpan.FromMilliseconds(pause.TotalMilliseconds * (1 + PauseRandomDeviation * (random.NextDouble() * 2 - 1)));
        }
    }
}