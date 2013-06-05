using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

using System.Linq;

namespace HotspotDevelopments.LocalRefactor
{
    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType("code")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    class VsTextViewCreationListener : IVsTextViewCreationListener
    {
        [Import]
        IVsEditorAdaptersFactoryService AdaptersFactory = null;

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            var wpfTextView = AdaptersFactory.GetWpfTextView(textViewAdapter);
            if (wpfTextView == null)
            {
                Debug.Fail("Unable to get IWpfTextView from text view adapter");
                return;
            }

            LocalRefactorCommandFilter filter = new LocalRefactorCommandFilter(wpfTextView);

            IOleCommandTarget next;
            if (ErrorHandler.Succeeded(textViewAdapter.AddCommandFilter(filter, out next)))
                filter.Next = next;
        }
    }

    class LocalRefactorCommandFilter : IOleCommandTarget
    {
        CodeManipulator codeManipulator;

        public LocalRefactorCommandFilter(IWpfTextView view)
        {
            this.codeManipulator = new CodeManipulator(view, new UINameProvider());
        }

        internal IOleCommandTarget Next { get; set; }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (pguidCmdGroup == GuidList.guidLocalRefactorCmdSet)
            {
                if (nCmdID == PkgCmdIDList.cmdidExtractVariable)
                {
                    codeManipulator.ExtractVariable();
                    return VSConstants.S_OK;
                }
                if (nCmdID == PkgCmdIDList.cmdidExtractConstant)
                {
                    codeManipulator.ExtractConstant();
                    return VSConstants.S_OK;
                }
                if (nCmdID == PkgCmdIDList.cmdidAssignParameter)
                {
                    codeManipulator.AssignParameterToField();
                    return VSConstants.S_OK;
                }
                if (nCmdID == PkgCmdIDList.cmdidConvertVariable)
                {
                    codeManipulator.ConvertVariableToField();
                    return VSConstants.S_OK;
                }
            }
            return Next.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (pguidCmdGroup == GuidList.guidLocalRefactorCmdSet &&
                (prgCmds[0].cmdID == PkgCmdIDList.cmdidExtractVariable || prgCmds[0].cmdID == PkgCmdIDList.cmdidExtractConstant))
            {
                if (codeManipulator.HasExpressionSelected)
                    prgCmds[0].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                else
                    prgCmds[0].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED);

                return VSConstants.S_OK;
            }

            if (pguidCmdGroup == GuidList.guidLocalRefactorCmdSet &&
                (prgCmds[0].cmdID == PkgCmdIDList.cmdidAssignParameter || prgCmds[0].cmdID == PkgCmdIDList.cmdidConvertVariable))
            {
                if (codeManipulator.HasCode)
                    prgCmds[0].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                else
                    prgCmds[0].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED);

                return VSConstants.S_OK;
            }

            return Next.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

    }
}