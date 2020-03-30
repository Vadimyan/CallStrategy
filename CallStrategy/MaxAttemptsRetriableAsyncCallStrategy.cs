using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CallStrategy
{
    public class MaxAttemptsRetriableAsyncCallStrategy : IRetriableAsyncCallStrategy
    {
        private readonly int pauseBetweenCallsMilliseconds;
        private readonly int retriesBeforeStop;
        
        public MaxAttemptsRetriableAsyncCallStrategy(int retriesBeforeStop, int pauseBetweenCallsMilliseconds)
        {
            this.pauseBetweenCallsMilliseconds = pauseBetweenCallsMilliseconds;
            if (this.pauseBetweenCallsMilliseconds <= 0)
                this.pauseBetweenCallsMilliseconds = 1000;

            this.retriesBeforeStop = retriesBeforeStop;
            if (this.retriesBeforeStop <= 0)
                this.retriesBeforeStop = 1;
        }

        public async Task CallAsync(Func<Task> action, Func<Exception, bool> isExceptionRetriable, Action beforeRetry = null)
        {
            var exceptions = new SortedSet<Exception>(new ObstinateCallExceptionComparer());
            var message = "";
            for (var failedTries = 0; failedTries < retriesBeforeStop; ++failedTries)
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
                    if (ExceptionFinder.FindException(ex, isExceptionRetriable) == null)
                        throw;
                    message = ex.Message;
                    if (failedTries < retriesBeforeStop - 1)
                    {
                        await Task.Delay(pauseBetweenCallsMilliseconds).ConfigureAwait(false);
                    }

                    if (!exceptions.Contains(ex))
                        exceptions.Add(ex);
                }
            }

            throw new AttemptsExceededException(message, exceptions);
        }

        public async Task<T> CallAsync<T>(Func<Task<T>> action, Func<Exception, bool> isExceptionRetriable, Action beforeRetry = null)
        {
            var result = default(T);
            await CallAsync((Func<Task>) (async () => result = await action().ConfigureAwait(false)), isExceptionRetriable, beforeRetry).ConfigureAwait(false);
            return result;
        }
    }
}