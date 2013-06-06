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
For example selecting '\n' in code below

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

choosing _Extract Local Constant_ and supplying the constant name NEWLINE results in the code below

    class CodeNavigator
    {
        private ITextSnapshot snapshot;
        private const char NEWLINE = '\n';

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
