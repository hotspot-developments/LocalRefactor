using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text;

namespace HotspotDevelopments.LocalRefactor
{
    class CodeNavigator
    {
        private ITextSnapshot snapshot;

        public CodeNavigator(ITextSnapshot snapshot)
        {
            this.snapshot = snapshot;
        }

        public IEnumerable<int> UpFrom(int p)
        {
            Stack<PairedChar> pairs = new Stack<PairedChar>();

            for (int c = p - OffsetCodeFromPosition(p); c >= 0; c--)
            {
                char ch = snapshot[c];

                // if its a new line - get the line and deal with line comments,
                if (ch == '\n')
                {
                    string line = snapshot.GetLineFromPosition(c - 1).GetText();
                    c -= OffsetOfCodeFromEndOfLine(line);
                    if (pairs.Count > 0 && pairs.Peek().IsQuote) pairs.Pop();
                    continue;
                }

                if (pairs.Count > 0 && pairs.Peek().IsQuote && (!pairs.Peek().IsOpenedBy(ch) || (c > 0 && snapshot[c - 1] == '\\')))
                {
                    continue;
                }
                if (pairs.Count == 0 && (ch == '{'))
                {
                    yield return c;
                }
                else if (pairs.Count == 0 && (ch == ';'))
                {
                    yield return c;
                }
                else if (pairs.Count > 0 && pairs.Peek().IsOpenedBy(ch))
                {
                    pairs.Pop();
                }
                else if (PairedChar.IsPairClosing(ch))
                {
                    pairs.Push(PairedChar.from(ch));
                }
            }

            yield return 0;
            yield break;
        }

        public IEnumerable<int> DownFrom(int p)
        {
            Stack<PairedChar> pairs = new Stack<PairedChar>();

            for (int c = p; c < snapshot.Length; c++)
            {
                char ch = snapshot[c];

                if (pairs.Count > 0 && pairs.Peek().IsQuote && (!pairs.Peek().IsClosedBy(ch) || (c > 0 && snapshot[c - 1] == '\\'))) continue;

                if (c > 0 && ch == '/' && snapshot[c - 1] == '/')
                {
                    c = snapshot.GetLineFromPosition(c - 1).End + 1;
                    continue;
                }
                if (ch == '}' && (pairs.Count == 2) && pairs.Peek().IsClosedBy(ch))
                {
                    pairs.Pop();
                    yield return c;
                }
                else if (ch == '}' && (pairs.Count == 1) && pairs.Peek().IsClosedBy(ch))
                {
                    pairs.Pop();
                    yield return c;
                    yield break;
                }
                else if (pairs.Count == 1 && (ch == ';'))
                {
                    yield return c;
                }
                else if (pairs.Count > 0 && pairs.Peek().IsClosedBy(ch))
                {
                    pairs.Pop();
                }
                else if (PairedChar.IsPairOpening(ch))
                {
                    pairs.Push(PairedChar.from(ch));
                }
            }
            yield break;
        }

        public bool IsInClassScope(int p)
        {
            int foundScope = -1;
            Stack<PairedChar> pairs = new Stack<PairedChar>();
            for (int c = p - OffsetCodeFromPosition(p); c >= 0; c--)
            {
                char ch = snapshot[c];

                if (ch == '\n')
                {
                    string line = snapshot.GetLineFromPosition(c - 1).GetText();
                    c -= OffsetOfCodeFromEndOfLine(line);
                    if (pairs.Count > 0 && pairs.Peek().IsQuote) pairs.Pop();
                    continue;
                }

                if (pairs.Count > 0 && pairs.Peek().IsQuote && (!pairs.Peek().IsOpenedBy(ch) || (c > 0 && snapshot[c - 1] == '\\')))
                {
                    continue;
                }
                if (pairs.Count == 0 && (ch == '{') && foundScope == -1)
                {
                    foundScope = c ;
                }
                else if (foundScope > 0 && (ch == '}' || ch == ';' || ch == '{'))
                {
                    string text = snapshot.GetText(c, (foundScope - c) + 1).Replace("\n", " ");

                    return (text.Contains(" class "));
                }
                else if (pairs.Count > 0 && pairs.Peek().IsOpenedBy(ch))
                {
                    pairs.Pop();
                }
                else if (PairedChar.IsPairClosing(ch))
                {
                    pairs.Push(PairedChar.from(ch));
                }
            }
            return false;
        }

        private int OffsetCodeFromPosition(int p)
        {
            ITextSnapshotLine line = snapshot.GetLineFromPosition(p);
            return OffsetOfCodeFromEndOfLine(line.GetText().Substring(0, p - (line.Start.Position - 1)));
        }

        private int OffsetOfCodeFromEndOfLine(string line)
        {
            int p;
            int c=0;
            bool inQuote;
            do
            {
                inQuote = false;
                p = line.IndexOf("//", c);
                for (; c < p; c++)
                {
                    if (line[c] == '"' && (c == 0 || line[c - 1] != '\\')) inQuote = !inQuote;
                }
                for (; inQuote && c < line.Length; c++)
                {
                    if (line[c] == '"' && (c == 0 || line[c - 1] != '\\')) break ;
                }
            }
            while (p != -1 && inQuote && c < line.Length);
            if (p == -1)
            {
                return line.Length - line.TrimEnd().Length;
            }

            return line.Length - (p + 1);
        }
    }
}
