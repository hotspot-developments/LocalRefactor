using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HotspotDevelopments.LocalRefactor
{
    internal class PairedChar
    {
        public static readonly PairedChar SingleQuote = new PairedChar('\'', '\'', true);
        public static readonly PairedChar DoubleQuote = new PairedChar('"', '"', true);
        public static readonly PairedChar Bracket = new PairedChar('(', ')', false);
        public static readonly PairedChar Brace = new PairedChar('{', '}', false);
        public static readonly PairedChar Indexer = new PairedChar('[', ']', false);
        private static readonly Dictionary<char, PairedChar> pairs = new Dictionary<char, PairedChar> { { '\'', SingleQuote }, { '"', DoubleQuote }, { '(', Bracket }, { '{', Brace }, { '[', Indexer } };
        private char opening;
        private char closing;
        private bool isQuote;

        private PairedChar(char open, char close, bool isQuote)
        {
            this.opening = open;
            this.closing = close;
            this.isQuote = isQuote;
        }

        public bool IsClosedBy(char ch) { return ch == this.closing; }
        public bool IsOpenedBy(char ch) { return ch == this.opening; }
        public bool IsQuote { get { return this.isQuote; } }

        public static bool IsPairOpening(char ch) { return pairs.Keys.Contains(ch); }
        public static bool IsPairClosing(char ch)
        {
            foreach (PairedChar pair in pairs.Values)
            {
                if (ch == pair.closing) return true;
            }
            return false;
        }

        public static PairedChar from(char ch)
        {
            foreach (PairedChar pair in pairs.Values)
            {
                if (pair.opening == ch || pair.closing == ch) return pair;

            }
            return null;
        }
    }
}
