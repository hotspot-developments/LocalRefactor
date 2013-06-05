using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text;

namespace HotspotDevelopments.LocalRefactor
{
    public class Declaration
    {
        public readonly string VariableName;
        public readonly string TypeName;
        public readonly Span Span;
        public readonly Span StatementSpan;
        public readonly bool HasAssignment;

        public Declaration(Span span, Span lineSpan, string variableName, string typeName, bool hasEquals)
        {
            this.VariableName = variableName;
            this.TypeName = typeName;
            this.Span = span;
            this.StatementSpan = lineSpan;
            this.HasAssignment = hasEquals;
        }

        public override string ToString()
        {
            return TypeName + " " + VariableName;
        }
    }
}
