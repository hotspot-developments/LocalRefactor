using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HotspotDevelopments.LocalRefactor
{
    public class UnrecognisedExpressionException : Exception
    {
        public UnrecognisedExpressionException() : base("The selection was not recognised as a valid expression") { }
        public UnrecognisedExpressionException(string message) : base(message) { }
        public UnrecognisedExpressionException(string message, Exception exception) : base(message, exception) { }
    }
}
