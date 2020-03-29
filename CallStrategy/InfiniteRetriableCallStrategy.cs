using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CallStrategy
{
    public class InfiniteRetriableCallStrategy : IRetriableCallStrategy
	{
		private const double PauseRandomDeviation = 0.1;
		private const double PauseIncreaseStep = 1.15;
		
		private readonly long tolerableMemoryLeakSize;
		private readonly TimeSpan pauseBetweenCallsOnStart = TimeSpan.FromSeconds(1);
		private readonly TimeSpan maxPauseBetweenCalls = TimeSpan.FromMinutes(5);

		public InfiniteRetriableCallStrategy(long tolerableMemoryLeakSize = 256 * 1024 * 1024L)
		{
			this.tolerableMemoryLeakSize = tolerableMemoryLeakSize;
		}

		public void Call(Action action, Func<Exception, bool> isExceptionRetriable, Action beforeRetry = null)
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
					action();
					return;
				}
				catch (Exception ex)
				{
					++failedTries;
					if (!exceptions.Contains(ex))
						exceptions.Add(ex);
					var currentMemory = GC.GetTotalMemory(true);
					if (failedTries == 1)
						memoryUsageAtFirstFail = currentMemory;
					if (currentMemory > memoryUsageAtFirstFail + tolerableMemoryLeakSize)
					{
						throw new AttemptsExceededException(ex.Message, exceptions);
					}

					Task.Delay(pause).Wait();
					pause = GetNextPause(pause);
				}
			}
		}

		public T Call<T>(Func<T> action, Func<Exception, bool> isExceptionRetriable, Action beforeRetry = null)
		{
			var res = default(T);
			Call(delegate { res = action(); }, isExceptionRetriable, beforeRetry);
			return res;
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