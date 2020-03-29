using System;
using System.Collections.Generic;

namespace CallStrategy
{
    public class AttemptsExceededException : AggregateException
    {
        public AttemptsExceededException(string message, IEnumerable<Exception> innerExceptions)
            : base(message, innerExceptions)
        {
        }
    }
}