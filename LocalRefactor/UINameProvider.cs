using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Shell.Interop;
using System.Globalization;
using System.Windows.Forms;

namespace HotspotDevelopments.LocalRefactor
{
    public class UINameProvider : HotspotDevelopments.LocalRefactor.INameProvider
    {
        private NameForm nameEntry = new NameForm();

        public string GetName()
        {
            return GetPromptedName("Enter variable name:", "var1");
        }

        public string GetConstantName()
        {
            return GetPromptedName("Enter constant name:", "CONST1");
        }

        private string GetPromptedName(string prompt, string val)
        {
            nameEntry.Label = prompt;
            nameEntry.EnteredName = val;
            if (nameEntry.ShowDialog() == DialogResult.OK)
            {
                return nameEntry.EnteredName;
            }
            return null;
        }

    }
}
