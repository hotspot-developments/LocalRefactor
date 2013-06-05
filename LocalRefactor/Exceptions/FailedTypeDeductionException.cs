using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HotspotDevelopments.LocalRefactor
{
    public class FailedTypeDeductionException : Exception
    {
        public FailedTypeDeductionException() : base("Failed to deduce type of expression.") { }
        public FailedTypeDeductionException(string message) : base(message) { }
        public FailedTypeDeductionException(string message, Exception exception) : base(message, exception) { }
    }
}
