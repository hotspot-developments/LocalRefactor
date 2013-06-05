using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HotspotDevelopments.LocalRefactor
{
    public class InvalidConstantExpressionException : Exception
    {
        public InvalidConstantExpressionException() : base("The selection was not recognised as a valid constant expression") { }
        public InvalidConstantExpressionException(string message) : base(message) { }
        public InvalidConstantExpressionException(string message, Exception exception) : base(message, exception) { }
    }
}
