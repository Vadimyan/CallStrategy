using System;
using System.Threading.Tasks;

namespace CallStrategy
{
    public interface IRetriableAsyncCallStrategy
    {
        Task CallAsync(Func<Task> action, Func<Exception, bool> isExceptionRetriable, Action beforeRetry = null);
        Task<T> CallAsync<T>(Func<Task<T>> action, Func<Exception, bool> isExceptionRetriable, Action beforeRetry = null);
    }
}