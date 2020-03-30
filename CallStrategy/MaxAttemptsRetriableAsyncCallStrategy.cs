using System;
using System.Threading.Tasks;

namespace CallStrategy
{
    public class MaxAttemptsRetriableAsyncCallStrategy : MaxAttemptsRetriableCallStrategyBase, IRetriableAsyncCallStrategy
    {
        public MaxAttemptsRetriableAsyncCallStrategy(int retriesBeforeStop, int pauseBetweenCallsMilliseconds)
            : base(retriesBeforeStop, pauseBetweenCallsMilliseconds)
        {
        }

        public async Task CallAsync(Func<Task> action, Func<Exception, bool> isExceptionRetriable, Action beforeRetry = null)
        {
            await PerformCall(action, isExceptionRetriable, beforeRetry);
        }

        public async Task<T> CallAsync<T>(Func<Task<T>> action, Func<Exception, bool> isExceptionRetriable, Action beforeRetry = null)
        {
            var result = default(T);
            await CallAsync((Func<Task>) (async () => result = await action().ConfigureAwait(false)), isExceptionRetriable, beforeRetry).ConfigureAwait(false);
            return result;
        }
    }
}