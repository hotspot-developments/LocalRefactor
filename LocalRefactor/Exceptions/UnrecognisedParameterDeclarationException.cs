using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HotspotDevelopments.LocalRefactor
{
    public class UnrecognisedParameterDeclarationException : Exception
    {
        public UnrecognisedParameterDeclarationException() : base("The position was not recognised as a valid parameter declaration") { }
        public UnrecognisedParameterDeclarationException(string message) : base(message) { }
        public UnrecognisedParameterDeclarationException(string message, Exception exception) : base(message, exception) { }
    }
}
