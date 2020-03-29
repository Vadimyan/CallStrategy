using System;

namespace CallStrategy
{
    public interface IRetriableCallStrategy
         {
        void Call(Action action, Func<Exception, bool> isExceptionRetriable, Action beforeRetry = null);
        T Call<T>(Func<T> action, Func<Exception, bool> isExceptionRetriable, Action beforeRetry = null);
    }
}