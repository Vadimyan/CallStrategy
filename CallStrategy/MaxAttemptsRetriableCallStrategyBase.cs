using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CallStrategy
{
    public abstract class MaxAttemptsRetriableCallStrategyBase
    {
        private readonly int pauseBetweenCallsMilliseconds;
        private readonly int retriesBeforeStop;

        protected MaxAttemptsRetriableCallStrategyBase(int retriesBeforeStop, int pauseBetweenCallsMilliseconds)
        {
            this.pauseBetweenCallsMilliseconds = pauseBetweenCallsMilliseconds;
            if (this.pauseBetweenCallsMilliseconds <= 0)
                this.pauseBetweenCallsMilliseconds = 1000;

            this.retriesBeforeStop = retriesBeforeStop;
            if (this.retriesBeforeStop <= 0)
                this.retriesBeforeStop = 1;
        }
        
        protected async Task PerformCall(Func<Task> action, Func<Exception, bool> isExceptionRetriable, Action beforeRetry = null)
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
                        Task.Delay(pauseBetweenCallsMilliseconds).Wait();
                    }

                    if (!exceptions.Contains(ex))
                        exceptions.Add(ex);
                }
            }

            throw new AttemptsExceededException(message, exceptions);
        }
    }
}