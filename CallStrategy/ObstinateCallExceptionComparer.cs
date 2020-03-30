﻿using System;
using System.Collections.Generic;

namespace CallStrategy
{
    internal class ObstinateCallExceptionComparer : IComparer<Exception>
    {
        public int Compare(Exception e1, Exception e2)
        {
            var cmp = string.CompareOrdinal(e1.Message, e2.Message);
            return cmp != 0 ? cmp : string.Compare(e1.StackTrace, e2.StackTrace, StringComparison.Ordinal);
        }
    }
}