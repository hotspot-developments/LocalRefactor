using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HotspotDevelopments.LocalRefactor
{
    public class FailedInsertionPointException : Exception
    {
        public FailedInsertionPointException() : base("Failed to find insertion point for declaration.") { }
        public FailedInsertionPointException(string message) : base(message) { }
        public FailedInsertionPointException(string message, Exception exception) : base(message, exception) { }
    }
}
