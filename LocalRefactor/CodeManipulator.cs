using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace HotspotDevelopments.LocalRefactor
{
    class CodeManipulator
    {
        private IWpfTextView view;
        private INameProvider nameProvider;
        private static readonly char[] operatorChars = new char[] { ':', '+', '-', '*', '/', '%', '&', '|', '!', '~', '<', '>', '=', '^', '?' };
        private static readonly char[] permissableFollowing = operatorChars.Concat(new char[] { '.', ')', ',', ';', '}',']' }).ToArray();
        private static readonly char[] permissablePreceding = operatorChars.Concat(new char[] { '(', ',', '[', '{' }).ToArray();
        private int tabSize = -1;

        public CodeManipulator(IWpfTextView view, INameProvider provider)
        {
            this.view = view;
            this.nameProvider = provider;
        }

        public bool HasCode
        {
            get
            {
                return view.TextViewLines.Count > 0;
            }
        }

        public bool HasExpressionSelected
        {
            get
            {
                return !view.Selection.IsEmpty;
            }
        }

        public void ExtractVariable()
        {
            string selection = view.Selection.SelectedSpans[0].GetText();
            ITextSnapshot snapshot = view.TextBuffer.CurrentSnapshot;

            // Verify the selected text doesn't contain mismatched quotes or braces.
            if (!hasIntegrity(selection)) throw new UnrecognisedExpressionException();

            // Check that what comes after is .,);} or operator
            char next = NextNonWhiteSpaceCharacter(snapshot);
            if (!permissableFollowing.Contains(next)) throw new UnrecognisedExpressionException();

            // Check that what comes before is (,= or operator
            char prev = PreviousNonWhiteSpaceCharacter(snapshot);
            if (!permissablePreceding.Contains(prev)) throw new UnrecognisedExpressionException();

            //  Get a variable name from the supplied provider.
            string varName = nameProvider.GetName();
            if (varName == null) return;
            
            int lineNumber = FindLineAfterWhichToInsertDeclaration(snapshot, view.Selection.SelectedSpans[0].Start.Position);
            if (lineNumber < 0) throw new FailedInsertionPointException();
                
            // replace selection with variable name.
            ITextEdit edit = snapshot.TextBuffer.CreateEdit();
            edit.Replace(view.Selection.SelectedSpans[0], varName);

            int indentSize = GetIndentOfNextNonBlankLine(snapshot, lineNumber);
            // Add declaration.
            edit.Insert(snapshot.GetLineFromLineNumber(lineNumber + 1).Start.Position, ( "".PadLeft(indentSize) + "var " + varName + " = " + selection + ";\n"));

            edit.Apply();
        }

        public void ExtractConstant()
        {
            string selection = view.Selection.SelectedSpans[0].GetText();
            ITextSnapshot snapshot = view.TextBuffer.CurrentSnapshot;

            // Verify the selected text doesn't contain mismatched quotes or braces.
            if (!hasConstantIntegrity(selection)) throw new InvalidConstantExpressionException();

            // Check that what comes after is .,);} or operator
            char next = NextNonWhiteSpaceCharacter(snapshot);
            if (!permissableFollowing.Contains(next)) throw new UnrecognisedExpressionException();

            // Check that what comes before is (,= or operator
            char prev = PreviousNonWhiteSpaceCharacter(snapshot);
            if (!permissablePreceding.Contains(prev)) throw new UnrecognisedExpressionException();

            string typeName = DeduceTypeOfConstant(selection);
            if (typeName == null) throw new FailedTypeDeductionException();

            //  Get a variable name from the supplied provider.
            string varName = nameProvider.GetConstantName();
            if (varName == null) return;
                
            int lineNumber = FindLineAfterWhichToInsertFieldDeclaration(snapshot, view.Selection.SelectedSpans[0].Start.Position);
            if (lineNumber < 0)  throw new FailedInsertionPointException();
                    
            // replace selection with variable name.
            ITextEdit edit = snapshot.TextBuffer.CreateEdit();
            edit.Replace(view.Selection.SelectedSpans[0], varName);

            int indentSize = GetIndentOfNextNonBlankLine(snapshot, lineNumber);

            // Add declaration.
            edit.Insert(snapshot.GetLineFromLineNumber(lineNumber + 1).Start.Position, ("".PadLeft(indentSize) + "private const " + typeName + " " + varName + " = " + selection + ";\n"));

            edit.Apply();
        }

        public void ConvertVariableToField()
        {
            ITextSnapshot snapshot = view.TextBuffer.CurrentSnapshot;
            int pos = view.Caret.Position.BufferPosition.Position;
            Declaration declaration = FindDeclarationNear(snapshot, pos, new char[] { ';', '{' }, new char[] { ';', '=' });
            if (declaration == null) throw new UnrecognisedDeclarationException();

            int lineNumber = FindLineAfterWhichToInsertFieldDeclaration(snapshot, declaration.Span.Start);
            if (lineNumber < 0) throw new FailedInsertionPointException();
                
            // replace selection with variable name.
            ITextEdit edit = snapshot.TextBuffer.CreateEdit();
            if (declaration.HasAssignment)
            {
                edit.Replace(declaration.Span, declaration.VariableName);
            }
            else
            {
                edit.Delete(declaration.StatementSpan);
            }

            int indentSize = GetIndentOfNextNonBlankLine(snapshot, lineNumber);

            // Add declaration.
            edit.Insert(snapshot.GetLineFromLineNumber(lineNumber + 1).Start.Position, ("".PadLeft(indentSize) + "private " + declaration.ToString() + ";\n"));
            edit.Apply();
        }

        public void AssignParameterToField()
        {
            ITextSnapshot snapshot = view.TextBuffer.CurrentSnapshot;
            int pos = view.Caret.Position.BufferPosition.Position;

            Declaration declaration = FindDeclarationNear(snapshot, pos, new char[] { '(', ',' }, new char[] { ',', ')', '=' });
            if (declaration == null) throw new UnrecognisedParameterDeclarationException();
            
            int assignmentLineNumber = FindLineBeforeWhichToInsertMethodStatement(snapshot, declaration.Span.End);
            if (assignmentLineNumber < 0) throw new FailedInsertionPointException("Failed to find insertion point before assignment.");
            ITextSnapshotLine assignmentLine = snapshot.GetLineFromLineNumber(assignmentLineNumber);
                
            int lineNumber = FindLineAfterWhichToInsertFieldDeclaration(snapshot, assignmentLine.Start.Position);
            if (lineNumber < 0) throw new FailedInsertionPointException();

            // add assignment of parameter to method.
            ITextEdit edit = snapshot.TextBuffer.CreateEdit();
            int assignmentIndent = FindIndentOfLine(assignmentLine.GetText()) + TabSize;
            edit.Insert(assignmentLine.Start.Position, ("".PadLeft(assignmentIndent) + "this." + declaration.VariableName + " = " + declaration.VariableName + ";\n"));

            // Add declaration.
            int indentSize = GetIndentOfNextNonBlankLine(snapshot, lineNumber);
            edit.Insert(snapshot.GetLineFromLineNumber(lineNumber + 1).Start.Position, ("".PadLeft(indentSize) + "private " + declaration.ToString() + ";\n"));

            edit.Apply();
        }

        private int TabSize
        {
            get
            {
                if (tabSize == -1)
                {
                    tabSize = view.Options.GetOptionValue(DefaultOptions.TabSizeOptionId);
                }
                return tabSize;
            }
        }


        private int GetIndentOfNextNonBlankLine(ITextSnapshot snapshot, int lineNumber)
        {
            string lineText;
             do
            {
                lineNumber++;
                lineText = snapshot.GetLineFromLineNumber(lineNumber).GetText();
            } while (lineText.Trim().Length == 0 && lineNumber < snapshot.LineCount);

            return FindIndentOfLine(lineText);
        }

        private int FindIndentOfLine(string lineText)
        {
            int indent = 0;
            foreach (char ch in lineText)
            {
                if (!Char.IsWhiteSpace(ch)) break;
                if (ch == '\t')
                {
                    indent += TabSize;
                }
                else
                {
                    indent++;
                }
            }
            return indent;
        }

        private int FindLineAfterWhichToInsertDeclaration(ITextSnapshot snapshot, int p)
        {
            CodeNavigator navigator = new CodeNavigator(snapshot);
            IEnumerator<int> point = navigator.UpFrom(p).GetEnumerator();
            int originalLine = snapshot.GetLineNumberFromPosition(p);
            int lineNumber = -1;
            bool notEOF;
            do
            {
                notEOF = point.MoveNext();
                lineNumber = snapshot.GetLineNumberFromPosition(point.Current);
            }
            while( lineNumber == originalLine && notEOF) ;
            if (lineNumber == originalLine) lineNumber = -1;
            return lineNumber;
        }

        private int FindLineAfterWhichToInsertFieldDeclaration(ITextSnapshot snapshot, int p)
        {
            CodeNavigator navigator = new CodeNavigator(snapshot);
            foreach (int significantPoint in navigator.UpFrom(p))
            {
                if (navigator.IsInClassScope(significantPoint)) return snapshot.GetLineNumberFromPosition(significantPoint);
            }
            return -1;
        }

        private int FindLineBeforeWhichToInsertMethodStatement(ITextSnapshot snapshot, int p)
        {
            CodeNavigator navigator = new CodeNavigator(snapshot);
            int lastLineOfMethod = -1;
            foreach (int significantPoint in navigator.DownFrom(p))
            {
                lastLineOfMethod = significantPoint;
            }
            if (lastLineOfMethod > 0) return snapshot.GetLineNumberFromPosition(lastLineOfMethod);
            return -1;
        }

        private Declaration FindDeclarationNear(ITextSnapshot snapshot, int p, char[] precedingChars, char[] followingChars)
        {
            Declaration result = null;
            int end = p;
            for (; (!followingChars.Contains(snapshot[end]) && end < snapshot.Length); end++) ;
            end++;
            int lineEnd = -1;
            for (int c = end; lineEnd == -1 && Char.IsWhiteSpace(snapshot[c]) && end < snapshot.Length; c++) if (snapshot[c] == '\n') lineEnd = c;

            int start = p;
            int lineStart = -1;
            for (; (!precedingChars.Contains(snapshot[start]) && start > 0); start--)
            {
                if (lineStart == -1 && snapshot[start] == '\n') lineStart = start;
            }
            start--;

            string before = "(" + String.Join("|", precedingChars.Select(c => "\\" + c.ToString())) + ")";
            string after = "(?<terminus>(" + String.Join("|", followingChars.Select(c => "\\" + c.ToString())) + "))";
            Regex declarationPattern = new Regex(before + @"\s*(?<declaration>(?<typename>[A-Za-z0-9_.<>, ]+[\[\] ]*)\s+(?<varname>[A-Za-z0-9_]+))\s*" + after);
            Match match = declarationPattern.Match(snapshot.GetText(start, end - start));
            if (match.Success)
            {
                Span declarationSpan = new Span(start + match.Groups["declaration"].Index, match.Groups["declaration"].Length);
                Span statementSpan;
                if (lineStart > 0 && lineEnd > 0)
                {
                    statementSpan = new Span(lineStart, lineEnd - lineStart);
                }
                else
                {
                    statementSpan = new Span(start + match.Groups["declaration"].Index, (match.Groups["terminus"].Index - match.Groups["declaration"].Index) + 1);
                }
                result = new Declaration(declarationSpan,
                                          statementSpan,
                                          match.Groups["varname"].Value,
                                          match.Groups["typename"].Value,
                                          match.Groups["terminus"].Value == "=");
            }
            return result;
        }

        private string DeduceTypeOfConstant(string selection)
        {
            if (selection.StartsWith("\""))
            {
                return "string";
            }
            else if (selection.StartsWith("'"))
            {
                return "char";
            }
            else if (selection.Contains("."))
            {
                return "double";
            }
            return "int";
        }

        private char NextNonWhiteSpaceCharacter(ITextSnapshot snapshot)
        {
            int p = view.Selection.SelectedSpans[0].End.Position;
            for (int c = p; c < snapshot.Length; c++)
            {
                if (!Char.IsWhiteSpace(snapshot[c])) return snapshot[c];
            }
            return '\0';
        }

        private char PreviousNonWhiteSpaceCharacter(ITextSnapshot snapshot)
        {
            int p = view.Selection.SelectedSpans[0].Start.Position;
            for (int c = p-1; c >=0;  c--)
            {
                if (!Char.IsWhiteSpace(snapshot[c])) return snapshot[c];
            }
            return '\0';
        }



        private bool hasIntegrity(string selection)
        {
            Stack<PairedChar> pairs = new Stack<PairedChar>();
            foreach(char ch in selection)
            {

                if (pairs.Count > 0 && pairs.Peek().IsQuote && !pairs.Peek().IsClosedBy(ch)) continue;

                if (pairs.Count > 0 && pairs.Peek().IsClosedBy(ch))
                {
                    pairs.Pop();
                }
                else if (PairedChar.IsPairOpening(ch))
                {
                    pairs.Push(PairedChar.from(ch));
                }
                else if (PairedChar.IsPairClosing(ch))
                {
                    return false;
                }
            }
            return pairs.Count == 0;
        }

        private bool hasConstantIntegrity(string selection)
        {
            Stack<PairedChar> pairs = new Stack<PairedChar>();
            char lastNonWhitespaceChar = '\0';
            foreach (char ch in selection)
            {

                if (pairs.Count > 0 && pairs.Peek().IsQuote && !pairs.Peek().IsClosedBy(ch)) continue;

                if (pairs.Count > 0 && pairs.Peek().IsClosedBy(ch))
                {
                    pairs.Pop();
                }
                else if (PairedChar.IsPairOpening(ch))
                {
                    pairs.Push(PairedChar.from(ch));
                }
                else if (PairedChar.IsPairClosing(ch))
                {
                    return false;
                }
                else if ((ch == '_') || (Char.IsLetter(ch) && !Char.IsDigit(lastNonWhitespaceChar)))
                {
                    return false;
                }
                if (!Char.IsWhiteSpace(ch)) lastNonWhitespaceChar = ch;
            }
            return pairs.Count == 0;
        }



    }
}
