using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CallStrategy
{
    public class MaxAttemptsRetriableCallStrategy : IRetriableCallStrategy
    {
	    private readonly int pauseBetweenCallsMilliseconds;
        private readonly int retriesBeforeStop;

		public MaxAttemptsRetriableCallStrategy(int retriesBeforeStop, int pauseBetweenCallsMilliseconds)
		{
			this.pauseBetweenCallsMilliseconds = pauseBetweenCallsMilliseconds;
			if (this.pauseBetweenCallsMilliseconds <= 0)
				this.pauseBetweenCallsMilliseconds = 1000;

			this.retriesBeforeStop = retriesBeforeStop;
			if (this.retriesBeforeStop <= 0)
				this.retriesBeforeStop = 1;
		}
		
		public void Call(Action action, Func<Exception, bool> isExceptionRetriable, Action beforeRetry = null)
		{
			var exceptions = new SortedSet<Exception>(new ObstinateCallExceptionComparer());
			var message = "";
			for (var failedTries = 0; failedTries < retriesBeforeStop; ++failedTries)
			{
				try
				{
					if (failedTries > 0)
						beforeRetry?.Invoke();
					action();
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

		public T Call<T>(Func<T> action, Func<Exception, bool> isExceptionRetriable, Action beforeRetry = null)
		{
			var res = default(T);
			Call(delegate { res = action(); }, isExceptionRetriable, beforeRetry);
			return res;
		}
    }
}