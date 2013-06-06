# Visual Studio Local Refactor
This code is a Visual Studio extention (VSIX) for adding 4 options to the refactor context menu for C# files.

- Extract Local Variable
- Extract Local Constant
- Assign Parameter to Field
- Convert Variable to Field

These were among the more frequently used operations I enjoyed when developing Java code in eclipse. 
I'm sure there are several commercially available tools that add this functionality and much, much more. 
However I wanted something lightweight that didn't add more cross-referencing files, and since these operations 
can be performed using simple pattern matching within the file (hence Local) I thought it would constitute a nice project.

## Extract Local Variable
Given that an expression is selected, replace the expression with a variable (prompting for its name) and declare the variable on the closest preceeding line.

For example selecting *lineNumber + 1* in the line below

	snapshot.GetLineFromLineNumber(lineNumber + 1);

choosing _Extract Local Variable_ and supplying the variable name "nextLine" yields the code below

	var nextLine = lineNumber + 1;
	snapshot.GetLineFromLineNumber(nextLine);

## Extract Local Constant
Given that a literal expression is selected, replace the expression with a constant (prompting for its name) and declare the constant after the closest field declaration.
For example selecting '\\n' in the code below

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
                if (ch == '\\n')
                {

choosing _Extract Local Constant_ and supplying the constant name NEWLINE results in the code below

    class CodeNavigator
    {
        private ITextSnapshot snapshot;
        private const char NEWLINE = '\\n';

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
                if (ch == NEWLINE)
                {

## Assign Parameter to Field
Given a parameter to a constructor or other method, declares a field of the same name in the class and assigns the parameter to the field in the method.

For example in the code below, choosing _Assign Parameter to Field_ on the stream parameter

    public class Timer : IDisposable
    {
        string logMessage;
        Stopwatch timer;

        public Timer(String logMessage, TextWriter stream = null)
        {
            if (stream == null)
            {
                stream = Console.Out;
            }
            this.logMessage = logMessage;
            timer = new Stopwatch();
            timer.Start();
        }

will ammend to code to the following

    public class Timer : IDisposable
    {
        string logMessage;
        Stopwatch timer;
        private TextWriter stream;

        public Timer(String logMessage, TextWriter stream = null)
        {
            if (stream == null)
            {
                stream = Console.Out;
            }
            this.logMessage = logMessage;
            timer = new Stopwatch();
            timer.Start();
            this.stream = stream;
        }

## Convert Variable to Field
Given a variable declaration in a method, declares a field of the same name, removes the local declaration but retains any assignment.

For example in the code below, clicking on the "pairs" variable and choosing _Convert Variable to Field_

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

will result in the modified code below.

    class CodeNavigator
    {
        private ITextSnapshot snapshot;
        private Stack<PairedChar> pairs;

        public CodeNavigator(ITextSnapshot snapshot)
        {
            this.snapshot = snapshot;
        }

        public IEnumerable<int> UpFrom(int p)
        {
            pairs = new Stack<PairedChar>();

# Local Refactor Tests
There is a unit testing project included in the source code, although I found that unit testing the CodeManipulator was tricky to do in the way that I wanted. 
I ended up writing a stub implementation of the VisualStudio ITextBuffer, based around simple strings that was a bit more involved than I had wanted but which seems to do the job.
Not all the methods are implemented (just the ones I was using) but it might be useful for another extension writer.

#Acknowledgements
The project that helped me the most was Noah Richards' [Align Assignments](http://visualstudiogallery.msdn.microsoft.com/0cc34d69-c6f1-41e3-ac6e-5de071b3edc8). Thanks!
