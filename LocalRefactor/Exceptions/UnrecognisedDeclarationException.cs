using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HotspotDevelopments.LocalRefactor
{
    public class UnrecognisedDeclarationException : Exception
    {
        public UnrecognisedDeclarationException() : base("The position was not recognised as a valid declaration") { }
        public UnrecognisedDeclarationException(string message) : base(message) { }
        public UnrecognisedDeclarationException(string message, Exception exception) : base(message, exception) { }
    }
}
