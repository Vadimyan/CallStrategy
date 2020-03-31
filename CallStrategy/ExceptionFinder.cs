using System;
using System.Linq;

namespace CallStrategy
{
    public class ExceptionFinder
    {
        public static TException FindException<TException>(Exception rootEx, Func<TException, bool> condition)
            where TException : Exception
        {
            return FindException(rootEx,
                ex =>
                {
                    var te = ex as TException;
                    return te != null && condition(te);
                }) as TException;
        }

        public static Exception FindException(Exception rootEx, Func<Exception, bool> condition)
        {
            var ex = rootEx;
            while (ex != null && !condition(ex))
                ex = GetChildEx(ex);
            return ex;
        }

        private static Exception GetChildEx(Exception ex)
        {
            var aggregateEx = ex as AggregateException;
            return aggregateEx != null
                ? aggregateEx.InnerExceptions.LastOrDefault()
                : ex.InnerException;
        }
    }
}