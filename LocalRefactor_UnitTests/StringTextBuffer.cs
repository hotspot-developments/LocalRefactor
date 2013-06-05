using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Formatting;

namespace HotspotDevelopments.LocalRefactor
{
    class StringTextBuffer : ITextBuffer
    {
        private TestSnapshot snapshot;

        internal StringTextBuffer(String sample)
        {
            this.snapshot = new TestSnapshot(this, sample);
        }
        

        public void ChangeContentType(IContentType newContentType, object editTag)
        {
            throw new NotImplementedException();
        }

        #pragma warning disable 0067    // Not used - present only to satisfy the interface ITextBuffer
        public event EventHandler<TextContentChangedEventArgs> Changed;
        public event EventHandler<TextContentChangedEventArgs> ChangedHighPriority;
        public event EventHandler<TextContentChangedEventArgs> ChangedLowPriority;
        public event EventHandler<TextContentChangingEventArgs> Changing;
        public event EventHandler<ContentTypeChangedEventArgs> ContentTypeChanged;
        public event EventHandler PostChanged;
        public event EventHandler<SnapshotSpanEventArgs> ReadOnlyRegionsChanged;
        #pragma warning restore 0067

        public bool CheckEditAccess()
        {
            throw new NotImplementedException();
        }

        public IContentType ContentType
        {
            get { throw new NotImplementedException(); }
        }


        public ITextEdit CreateEdit()
        {
            return new TestEdit(this.snapshot);
        }

        public ITextEdit CreateEdit(EditOptions options, int? reiteratedVersionNumber, object editTag)
        {
            return new TestEdit(this.snapshot);
        }

        public IReadOnlyRegionEdit CreateReadOnlyRegionEdit()
        {
            throw new NotImplementedException();
        }

        public ITextSnapshot CurrentSnapshot
        {
            get { return this.snapshot; }
        }

        public ITextSnapshot Delete(Span deleteSpan)
        {
            throw new NotImplementedException();
        }

        public bool EditInProgress
        {
            get { throw new NotImplementedException(); }
        }

        public NormalizedSpanCollection GetReadOnlyExtents(Span span)
        {
            throw new NotImplementedException();
        }

        public ITextSnapshot Insert(int position, string text)
        {
            throw new NotImplementedException();
        }

        public bool IsReadOnly(Span span, bool isEdit)
        {
            throw new NotImplementedException();
        }

        public bool IsReadOnly(Span span)
        {
            throw new NotImplementedException();
        }

        public bool IsReadOnly(int position, bool isEdit)
        {
            throw new NotImplementedException();
        }

        public bool IsReadOnly(int position)
        {
            throw new NotImplementedException();
        }


        public ITextSnapshot Replace(Span replaceSpan, string replaceWith)
        {
            throw new NotImplementedException();
        }

        public void TakeThreadOwnership()
        {
            throw new NotImplementedException();
        }

        public PropertyCollection Properties
        {
            get { throw new NotImplementedException(); }
        }

        internal ITextSelection Select(string toSelect)
        {
            string alltext = snapshot.GetText();
            int start = alltext.IndexOf(toSelect);

            return new TestSelection(this, alltext, start, toSelect.Length);
        }

        internal CaretPosition GetPositionAtStartOf(string selection)
        {
            string alltext = snapshot.GetText();
            int start = alltext.IndexOf(selection);
            SnapshotPoint point = new SnapshotPoint(snapshot, start);
            return new CaretPosition(new VirtualSnapshotPoint( point), new TestMappingPoint(this, snapshot, point), PositionAffinity.Successor);
        }
    }

    class TestSnapshot : ITextSnapshot
    {
        private StringTextBuffer testTextBuffer;
        private string text;
        private List<ITextSnapshotLine> lines = null;

        public TestSnapshot(StringTextBuffer testTextBuffer, string text)
        {
            this.testTextBuffer = testTextBuffer;
            this.text = text;
        }

        internal void Override(string text) 
        {
            this.text = text;
            this.lines = null;
        }

        public IContentType ContentType
        {
            get { throw new NotImplementedException(); }
        }

        public void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
        {
            throw new NotImplementedException();
        }

        public ITrackingPoint CreateTrackingPoint(int position, PointTrackingMode trackingMode, TrackingFidelityMode trackingFidelity)
        {
            throw new NotImplementedException();
        }

        public ITrackingPoint CreateTrackingPoint(int position, PointTrackingMode trackingMode)
        {
            throw new NotImplementedException();
        }

        public ITrackingSpan CreateTrackingSpan(int start, int length, SpanTrackingMode trackingMode, TrackingFidelityMode trackingFidelity)
        {
            throw new NotImplementedException();
        }

        public ITrackingSpan CreateTrackingSpan(int start, int length, SpanTrackingMode trackingMode)
        {
            throw new NotImplementedException();
        }

        public ITrackingSpan CreateTrackingSpan(Span span, SpanTrackingMode trackingMode, TrackingFidelityMode trackingFidelity)
        {
            throw new NotImplementedException();
        }

        public ITrackingSpan CreateTrackingSpan(Span span, SpanTrackingMode trackingMode)
        {
            throw new NotImplementedException();
        }

        public ITextSnapshotLine GetLineFromLineNumber(int lineNumber)
        {
            if (lines == null)
            {
                GenerateLines();
            }
            return lines[lineNumber];
        }

        public ITextSnapshotLine GetLineFromPosition(int position)
        {
            foreach (ITextSnapshotLine l in Lines)
            {
                if (position < l.End.Position) return l;

            }
            throw new IndexOutOfRangeException();
        }

        public int GetLineNumberFromPosition(int position)
        {
            foreach (ITextSnapshotLine l in Lines)
            {
                if (position < l.End.Position) return l.LineNumber;

            }
            throw new IndexOutOfRangeException();
        }

        public string GetText()
        {
            return text;
        }

        public string GetText(int startIndex, int length)
        {
            return text.Substring(startIndex, length);
        }

        public string GetText(Span span)
        {
            return text.Substring(span.Start, span.End - span.Start);
        }

        public int Length
        {
            get { return text.Length; }
        }

        public int LineCount
        {
            get { return text.Split('\n').Count(); }
        }

        public IEnumerable<ITextSnapshotLine> Lines
        {
            get 
            {
                if (lines == null)
                {
                    GenerateLines();
                }
                return lines; 
            }
        }

        private void GenerateLines()
        {
            lines = new List<ITextSnapshotLine>();
            string[] splitLines = text.Split('\n');
            int start = 0;
            for (int l = 0; l < splitLines.Length; l++)
            {
                string currentLine = splitLines[l];
                lines.Add(new TestSnapshotLine(testTextBuffer, this, l, start, currentLine));
                start += currentLine.Length + 1;
            }
        }

        public ITextBuffer TextBuffer
        {
            get { return this.testTextBuffer; }
        }

        public char[] ToCharArray(int startIndex, int length)
        {
            return text.Substring(startIndex, length).ToCharArray();
        }

        public ITextVersion Version
        {
            get { throw new NotImplementedException(); }
        }

        public void Write(System.IO.TextWriter writer)
        {
            throw new NotImplementedException();
        }

        public void Write(System.IO.TextWriter writer, Span span)
        {
            throw new NotImplementedException();
        }

        public char this[int position]
        {
            get { return text[position]; }
        }
    }

    class TestSnapshotLine : ITextSnapshotLine
    {
        int lineNumber;
        int start;
        int end;
        string line;
        private StringTextBuffer testTextBuffer;
        TestSnapshot parent;

        internal TestSnapshotLine(StringTextBuffer testTextBuffer, TestSnapshot parent, int linenumber, int start, string linetext)
        {
            this.start = start;
            this.line = linetext;
            this.end = this.start + linetext.Length;
            this.testTextBuffer = testTextBuffer;
            this.parent = parent;
            this.lineNumber = linenumber;
        }

        public SnapshotPoint End
        {
            get { return new SnapshotPoint(parent, end); }
        }

        public SnapshotPoint EndIncludingLineBreak
        {
            get { return new SnapshotPoint(parent, end + 1); }
        }

        public SnapshotSpan Extent
        {
            get { return new SnapshotSpan(Start, line.Length); }
        }

        public SnapshotSpan ExtentIncludingLineBreak
        {
            get { return new SnapshotSpan(Start, line.Length + 1); }
        }

        public string GetLineBreakText()
        {
            return "\\n";
        }

        public string GetText()
        {
            return line;
        }

        public string GetTextIncludingLineBreak()
        {
            return line + "\\n";
        }

        public int Length
        {
            get { return line.Length; }
        }

        public int LengthIncludingLineBreak
        {
            get { return line.Length + 1; }
        }

        public int LineBreakLength
        {
            get { return 1; }
        }

        public int LineNumber
        {
            get { return lineNumber; }
        }

        public ITextSnapshot Snapshot
        {
            get { return parent; }
        }

        public SnapshotPoint Start
        {
            get { return new SnapshotPoint(parent, start); }
        }
    }

    class TestSelection : ITextSelection
    {

        private StringTextBuffer testTextBuffer;
        private int start;
        private int length;
        private SnapshotSpan selectionSpan;
        

        public TestSelection(StringTextBuffer testTextBuffer, string text, int start, int length)
        {
            this.testTextBuffer = testTextBuffer;
            this.start = start;
            this.length = length;
            this.selectionSpan = new SnapshotSpan(testTextBuffer.CurrentSnapshot, new Span(start, length));
        }


        public bool ActivationTracksFocus
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public VirtualSnapshotPoint ActivePoint
        {
            get { throw new NotImplementedException(); }
        }

        public VirtualSnapshotPoint AnchorPoint
        {
            get { throw new NotImplementedException(); }
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public VirtualSnapshotPoint End
        {
            get { return new VirtualSnapshotPoint(selectionSpan.End); }
        }

        public VirtualSnapshotSpan? GetSelectionOnTextViewLine(Microsoft.VisualStudio.Text.Formatting.ITextViewLine line)
        {
            throw new NotImplementedException();
        }

        public bool IsActive
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool IsEmpty
        {
            get { return this.selectionSpan == null || this.selectionSpan.Length == 0; }
        }

        public bool IsReversed
        {
            get {  throw new NotImplementedException(); }
        }

        public TextSelectionMode Mode
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void Select(VirtualSnapshotPoint anchorPoint, VirtualSnapshotPoint activePoint)
        {
            throw new NotImplementedException();
        }

        public void Select(SnapshotSpan selectionSpan, bool isReversed)
        {
            throw new NotImplementedException();
        }

        public NormalizedSnapshotSpanCollection SelectedSpans
        {
            get { return new NormalizedSnapshotSpanCollection(selectionSpan); }
        }

        #pragma warning disable 0067
        public event EventHandler SelectionChanged;
        #pragma warning restore 0067

        public VirtualSnapshotPoint Start
        {
            get { return new VirtualSnapshotPoint(selectionSpan.Start); }
        }

        public VirtualSnapshotSpan StreamSelectionSpan
        {
            get { throw new NotImplementedException(); }
        }

        public ITextView TextView
        {
            get { throw new NotImplementedException(); }
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<VirtualSnapshotSpan> VirtualSelectedSpans
        {
            get { throw new NotImplementedException(); }
        }
    }

    class TestEdit : ITextEdit
    {
        private TestSnapshot snapshot;
        private StringBuilder text;
        
        public TestEdit(TestSnapshot snapshot)
        {
            this.snapshot = snapshot;
            this.text = new StringBuilder(snapshot.GetText());
        }

        public bool Delete(int startPosition, int charsToDelete)
        {
            text.Remove(startPosition, charsToDelete);
            return true;
        }

        public bool Delete(Span deleteSpan)
        {
            return Delete(deleteSpan.Start, deleteSpan.Length);
        }

        public bool HasEffectiveChanges
        {
            get { throw new NotImplementedException(); }
        }

        public bool HasFailedChanges
        {
            get { throw new NotImplementedException(); }
        }

        public bool Insert(int position, char[] characterBuffer, int startIndex, int length)
        {
            text.Insert(position, characterBuffer, startIndex, length);
            return true;
        }

        public bool Insert(int position, string textToInsert)
        {
            this.text.Insert(position, textToInsert);
            return true;
        }

        public bool Replace(int startPosition, int charsToReplace, string replaceWith)
        {
            text.Remove(startPosition, charsToReplace);
            text.Insert(startPosition, replaceWith);
            return true;
        }

        public bool Replace(Span replaceSpan, string replaceWith)
        {
            return Replace(replaceSpan.Start, replaceSpan.Length, replaceWith);
        }

        public ITextSnapshot Apply()
        {
            snapshot.Override(text.ToString());
            return snapshot;
        }

        public void Cancel()
        {
        }

        public bool Canceled
        {
            get { throw new NotImplementedException(); }
        }

        public ITextSnapshot Snapshot
        {
            get { return snapshot; }
        }

        public void Dispose()
        {
        }
    }

    class TestMappingPoint : IMappingPoint
    {
        private TestSnapshot snapshot;
        private StringTextBuffer textBuffer;
        private SnapshotPoint point;

        internal TestMappingPoint(StringTextBuffer textBuffer, TestSnapshot snapshot, SnapshotPoint point)
        {
            this.snapshot = snapshot;
            this.textBuffer = textBuffer;
            this.point = point;
        }

        public ITextBuffer AnchorBuffer
        {
            get { return textBuffer; }
        }

        public Microsoft.VisualStudio.Text.Projection.IBufferGraph BufferGraph
        {
            get { return null; }
        }

        public SnapshotPoint? GetInsertionPoint(Predicate<ITextBuffer> match)
        {
            return point;
        }

        public SnapshotPoint? GetPoint(Predicate<ITextBuffer> match, PositionAffinity affinity)
        {
            return point;
        }

        public SnapshotPoint? GetPoint(ITextSnapshot targetSnapshot, PositionAffinity affinity)
        {
            return point;
        }

        public SnapshotPoint? GetPoint(ITextBuffer targetBuffer, PositionAffinity affinity)
        {
            return point;
        }

    }

    
}
