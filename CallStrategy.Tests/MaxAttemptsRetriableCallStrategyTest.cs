using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CallStrategy.Tests
{
    public class MaxAttemptsRetriableCallStrategyTest
    {
        private class CallCounter
        {
            public int Count { get; set; }
        }

        private int CorrectAction(int x) => x * x;

        private async Task<int> CorrectActionAsync(int x) => await Task.FromResult(x * x);
        
        private bool IOExceptionAction(CallCounter counter)
        {
            counter.Count++;
            throw new IOException();
        }
        
        private async Task<bool> IOExceptionActionAsync(CallCounter counter)
        {
            counter.Count++;
            throw new IOException();
        }
        
        [TestCase(2, 4)]
        [TestCase(3, 9)]
        [TestCase(-5, 25)]
        public void MaxAttemptsCorrectMethodTest(int inputValue, int expectedResult)
        {
            var beforeRetryCount = 0;
            var callStrategy = new MaxAttemptsRetriableCallStrategy(5, 1);

            var result = callStrategy.Call(() => CorrectAction(inputValue), e => e is IOException, () => beforeRetryCount++);
            
            Assert.That(beforeRetryCount, Is.EqualTo(0));
            Assert.That(result, Is.EqualTo(expectedResult));
        }
        
        [TestCase(2, 4)]
        [TestCase(3, 9)]
        [TestCase(-5, 25)]
        public async Task AsyncMaxAttemptsCorrectMethodTest(int inputValue, int expectedResult)
        {
            var beforeRetryCount = 0;
            var callStrategy = new MaxAttemptsRetriableAsyncCallStrategy(5, 1);

            var result = await callStrategy.CallAsync(() => CorrectActionAsync(inputValue), e => e is IOException, () => beforeRetryCount++);
            
            Assert.That(beforeRetryCount, Is.EqualTo(0));
            Assert.That(result, Is.EqualTo(expectedResult));
        }
        
        [Test]
        public void MaxAttemptsFitExceptionTest()
        {
            var beforeRetryCount = 0;
            var callActionCount = new CallCounter();
            var callStrategy = new MaxAttemptsRetriableCallStrategy(5, 1);

            Assert.Throws<AttemptsExceededException>(() =>
                callStrategy.Call(() => IOExceptionAction(callActionCount), e => e is IOException, () => beforeRetryCount++));
            Assert.That(beforeRetryCount, Is.EqualTo(4));
            Assert.That(callActionCount.Count, Is.EqualTo(5));
        }
        
        [Test]
        public void MaxAttemptsNotFitExceptionTest()
        {
            var beforeRetryCount = 0;
            var callActionCount = new CallCounter();
            var callStrategy = new MaxAttemptsRetriableCallStrategy(5, 1);

            Assert.Throws<IOException>(() =>
                callStrategy.Call(() => IOExceptionAction(callActionCount), e => e is StackOverflowException, () => beforeRetryCount++));
            Assert.That(beforeRetryCount, Is.EqualTo(0));
            Assert.That(callActionCount.Count, Is.EqualTo(1));
        }

        
        
        [Test]
        public void AsyncMaxAttemptsFitExceptionTest()
        {
            var beforeRetryCount = 0;
            var callActionCount = new CallCounter();
            var callStrategy = new MaxAttemptsRetriableAsyncCallStrategy(5, 1);

            Assert.ThrowsAsync<AttemptsExceededException>(() =>
                callStrategy.CallAsync(() => IOExceptionActionAsync(callActionCount), e => e is IOException, () => beforeRetryCount++));
            Assert.That(beforeRetryCount, Is.EqualTo(4));
            Assert.That(callActionCount.Count, Is.EqualTo(5));
        }
        
        [Test]
        public void AsyncMaxAttemptsNotFitExceptionTest()
        {
            var beforeRetryCount = 0;
            var callActionCount = new CallCounter();
            var callStrategy = new MaxAttemptsRetriableAsyncCallStrategy(5, 1);

            Assert.ThrowsAsync<IOException>(() =>
                callStrategy.CallAsync(() => IOExceptionActionAsync(callActionCount), e => e is StackOverflowException, () => beforeRetryCount++));
            Assert.That(beforeRetryCount, Is.EqualTo(0));
            Assert.That(callActionCount.Count, Is.EqualTo(1));
        }
    }
}