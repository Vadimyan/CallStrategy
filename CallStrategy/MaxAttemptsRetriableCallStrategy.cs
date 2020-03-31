using System;
using System.Threading.Tasks;

namespace CallStrategy
{
    public class MaxAttemptsRetriableCallStrategy : MaxAttemptsRetriableCallStrategyBase, IRetriableCallStrategy
    {
		public MaxAttemptsRetriableCallStrategy(int retriesBeforeStop, int pauseBetweenCallsMilliseconds)
			: base(retriesBeforeStop, pauseBetweenCallsMilliseconds)
		{
		}
		
		public void Call(Action action, Func<Exception, bool> isExceptionRetriable, Action beforeRetry = null)
		{
			Task RunAsync(Action a)
			{
				a();
				return Task.FromResult(0);
			}
			
			PerformCall(() => RunAsync(action), isExceptionRetriable, beforeRetry).GetAwaiter().GetResult();
		}

		public T Call<T>(Func<T> action, Func<Exception, bool> isExceptionRetriable, Action beforeRetry = null)
		{
			var res = default(T);
			Call(delegate { res = action(); }, isExceptionRetriable, beforeRetry);
			return res;
		}
    }
}