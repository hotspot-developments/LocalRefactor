using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.VisualStudio.Text.Editor;


namespace HotspotDevelopments.LocalRefactor
{
    [TestClass]
    public class CodeManipulatorTests
    {
        private Mock<IWpfTextView> mockView;
        private Mock<INameProvider> mockNameProvider;
        private CodeManipulator manipulator;

        private const string sampleContext = "\nnamespace LocalRefactor" +
                "\n{{" +
                "\n    public class Test" +
                "\n    {{" +
                "\n        {1}" +
                "\n        public void Foo(string[] arg1, int arg2)" +
                "\n        {{" +
                "\n            {0}" +
                "\n        }}" +
                "\n    }}" +
                "\n}}"
                ;

        [TestInitialize]
        public void SetupTextView() 
        {
            mockView = new Mock<IWpfTextView>();
            mockNameProvider = new Mock<INameProvider>();
            mockView.Setup(m => m.Options.GetOptionValue(DefaultOptions.TabSizeOptionId)).Returns(4);

            manipulator = new CodeManipulator(mockView.Object, mockNameProvider.Object);

        }

        [TestMethod]
        public void ShouldHaveNoExpressionSelectedIfThereIsNoSelection()
        {
            mockView.Setup(m => m.Selection.IsEmpty).Returns(true);
            Assert.IsFalse(manipulator.HasExpressionSelected);
        }

        [TestMethod]
        public void ShouldCorrectlyDetectExpressionFromSelection()
        {
            mockView.Setup(m => m.Selection.IsEmpty).Returns(false);
            Assert.IsTrue(manipulator.HasExpressionSelected);
        }

        [TestMethod]
        public void ShouldExtractVariableAndInsertVariableOnLineBeforeThatContainingTheExpression()
        {
            StringTextBuffer testBuffer = new StringTextBuffer(ExpressionInContext("int x = a - b + c * d / e % 3444;"));

            mockView.Setup(m => m.TextBuffer).Returns(testBuffer);
            mockView.Setup(m => m.Selection).Returns(testBuffer.Select("a - b + c"));
            mockNameProvider.Setup(m => m.GetName()).Returns("expression");

            manipulator.ExtractVariable();

            Console.Out.Write(testBuffer.CurrentSnapshot.GetText());


            Assert.AreEqual("            var expression = a - b + c;", testBuffer.CurrentSnapshot.Lines.ElementAt(8).GetText());
            Assert.AreEqual("            int x = expression * d / e % 3444;", testBuffer.CurrentSnapshot.Lines.ElementAt(9).GetText());
        }

        [TestMethod]
        public void ShouldExtractVariableAndInsertVariableOnLineBeforeThatContainingTheExpressionEvenWhenThatLineContainsABrace()
        {
            StringTextBuffer testBuffer = new StringTextBuffer(ExpressionInContext("int x = new string[] {}.Length + a - b + c * d / e % 3444;"));

            mockView.Setup(m => m.TextBuffer).Returns(testBuffer);
            mockView.Setup(m => m.Selection).Returns(testBuffer.Select("a - b + c"));
            mockNameProvider.Setup(m => m.GetName()).Returns("expression");

            manipulator.ExtractVariable();

            Console.Out.Write(testBuffer.CurrentSnapshot.GetText());


            Assert.AreEqual("            var expression = a - b + c;", testBuffer.CurrentSnapshot.Lines.ElementAt(8).GetText());
            Assert.AreEqual("            int x = new string[] {}.Length + expression * d / e % 3444;", testBuffer.CurrentSnapshot.Lines.ElementAt(9).GetText());
        }

        [TestMethod]
        public void ShouldExtractVariableAndInsertVariableOnLineBeforeThatContainingTheExpressionEvenWhenMultipleExpressionsAreOnTheSameLine()
        {
            StringTextBuffer testBuffer = new StringTextBuffer(ExpressionInContext("int w = 1; int x = a - b + c * d / e % 3444;"));

            mockView.Setup(m => m.TextBuffer).Returns(testBuffer);
            mockView.Setup(m => m.Selection).Returns(testBuffer.Select("a - b + c"));
            mockNameProvider.Setup(m => m.GetName()).Returns("expression");

            manipulator.ExtractVariable();

            Console.Out.Write(testBuffer.CurrentSnapshot.GetText());


            Assert.AreEqual("            var expression = a - b + c;", testBuffer.CurrentSnapshot.Lines.ElementAt(8).GetText());
            Assert.AreEqual("            int w = 1; int x = expression * d / e % 3444;", testBuffer.CurrentSnapshot.Lines.ElementAt(9).GetText());
        }

        [TestMethod]
        [ExpectedException(typeof(FailedInsertionPointException))]
        public void ShouldThrowExceptionExtractingVariableAndWhenNoWhereToPutTheDeclaration()
        {
            StringTextBuffer testBuffer = new StringTextBuffer("int w = 1; int x = a - b + c * d / e % 3444;");

            mockView.Setup(m => m.TextBuffer).Returns(testBuffer);
            mockView.Setup(m => m.Selection).Returns(testBuffer.Select("a - b + c"));
            mockNameProvider.Setup(m => m.GetName()).Returns("expression");

            manipulator.ExtractVariable();

        }

        [TestMethod]
        public void ShouldExtractConstantAndInsertConstantOnLastLineOfClassDeclarations()
        {
            StringTextBuffer testBuffer = new StringTextBuffer(ExpressionInContext("int x = a - b + c * d / e % 3444;", "private const double pi=3.14159;"));

            mockView.Setup(m => m.TextBuffer).Returns(testBuffer);
            mockView.Setup(m => m.Selection).Returns(testBuffer.Select("3444"));
            mockNameProvider.Setup(m => m.GetConstantName()).Returns("constant");

            manipulator.ExtractConstant();

            Console.Out.Write(testBuffer.CurrentSnapshot.GetText());


            Assert.AreEqual("        private const int constant = 3444;", testBuffer.CurrentSnapshot.Lines.ElementAt(6).GetText());
            Assert.AreEqual("            int x = a - b + c * d / e % constant;", testBuffer.CurrentSnapshot.Lines.ElementAt(9).GetText());
        }

        [TestMethod]
        public void ShouldExtractConstantFromIfConditionAndInsertConstantOnLastLineOfClassDeclarations()
        {
            StringTextBuffer testBuffer = new StringTextBuffer(ExpressionInContext("if (y < 219) int x = a - b + c * d / e % 3444;"));

            mockView.Setup(m => m.TextBuffer).Returns(testBuffer);
            mockView.Setup(m => m.Selection).Returns(testBuffer.Select("219"));
            mockNameProvider.Setup(m => m.GetConstantName()).Returns("constant");

            manipulator.ExtractConstant();

            Console.Out.Write(testBuffer.CurrentSnapshot.GetText());


            Assert.AreEqual("        private const int constant = 219;", testBuffer.CurrentSnapshot.Lines.ElementAt(5).GetText());
            Assert.AreEqual("            if (y < constant) int x = a - b + c * d / e % 3444;", testBuffer.CurrentSnapshot.Lines.ElementAt(9).GetText());
        }

        [TestMethod]
        public void ShouldExtractConstantFromInsideIfStatementAndInsertConstantOnLastLineOfClassDeclarations()
        {
            StringTextBuffer testBuffer = new StringTextBuffer(ExpressionInContext("if (y < 219) \n{\n                int x = a - b + c * d / e % 3444;\n            }"));

            mockView.Setup(m => m.TextBuffer).Returns(testBuffer);
            mockView.Setup(m => m.Selection).Returns(testBuffer.Select("3444"));
            mockNameProvider.Setup(m => m.GetConstantName()).Returns("constant");

            manipulator.ExtractConstant();

            Console.Out.Write(testBuffer.CurrentSnapshot.GetText());


            Assert.AreEqual("        private const int constant = 3444;", testBuffer.CurrentSnapshot.Lines.ElementAt(5).GetText());
            Assert.AreEqual("                int x = a - b + c * d / e % constant;", testBuffer.CurrentSnapshot.Lines.ElementAt(11).GetText());
        }

        [TestMethod]
        public void ShouldExtractConstantIncludingUnaryOperatorAndInsertConstantOnLastLineOfClassDeclarations()
        {
            StringTextBuffer testBuffer = new StringTextBuffer(ExpressionInContext("int x = -1;"));

            mockView.Setup(m => m.TextBuffer).Returns(testBuffer);
            mockView.Setup(m => m.Selection).Returns(testBuffer.Select("-1"));
            mockNameProvider.Setup(m => m.GetConstantName()).Returns("constant");

            manipulator.ExtractConstant();

            Console.Out.Write(testBuffer.CurrentSnapshot.GetText());


            Assert.AreEqual("        private const int constant = -1;", testBuffer.CurrentSnapshot.Lines.ElementAt(5).GetText());
            Assert.AreEqual("            int x = constant;", testBuffer.CurrentSnapshot.Lines.ElementAt(9).GetText());
        }

        [TestMethod]
        public void ShouldExtractConstantAndInsertConstantOnLastLineOfClassDeclarationsCopingWithQuotedExpressionsWithEscapedQuotes()
        {
            StringTextBuffer testBuffer = new StringTextBuffer(ExpressionInContext("char quote= '\\'';\n            int x = 500;"));

            mockView.Setup(m => m.TextBuffer).Returns(testBuffer);
            mockView.Setup(m => m.Selection).Returns(testBuffer.Select("500"));
            mockNameProvider.Setup(m => m.GetConstantName()).Returns("constant");

            manipulator.ExtractConstant();

            Console.Out.Write(testBuffer.CurrentSnapshot.GetText());


            Assert.AreEqual("        private const int constant = 500;", testBuffer.CurrentSnapshot.Lines.ElementAt(5).GetText());
            Assert.AreEqual("            int x = constant;", testBuffer.CurrentSnapshot.Lines.ElementAt(10).GetText());
        }

        [TestMethod]
        public void ShouldExtractConstantAndInsertConstantOnLastLineOfClassDeclarationsCopingWithLineComments()
        {
            StringTextBuffer testBuffer = new StringTextBuffer(ExpressionInContext("int x = 500;", "// comment with bracket )"));

            mockView.Setup(m => m.TextBuffer).Returns(testBuffer);
            mockView.Setup(m => m.Selection).Returns(testBuffer.Select("500"));
            mockNameProvider.Setup(m => m.GetConstantName()).Returns("constant");

            manipulator.ExtractConstant();

            Console.Out.Write(testBuffer.CurrentSnapshot.GetText());


            Assert.AreEqual("        private const int constant = 500;", testBuffer.CurrentSnapshot.Lines.ElementAt(5).GetText());
            Assert.AreEqual("            int x = constant;", testBuffer.CurrentSnapshot.Lines.ElementAt(9).GetText());
        }

        [TestMethod]
        [ExpectedException(typeof(FailedInsertionPointException))]
        public void ShouldThrowExceptionExtractingConstantWhenNoWhereToPutTheDeclaration()
        {
            StringTextBuffer testBuffer = new StringTextBuffer("int x = a - b + c * d / e % 3444;");

            mockView.Setup(m => m.TextBuffer).Returns(testBuffer);
            mockView.Setup(m => m.Selection).Returns(testBuffer.Select("3444"));
            mockNameProvider.Setup(m => m.GetConstantName()).Returns("constant");

            manipulator.ExtractConstant();

        }


        [TestMethod]
        public void ShouldConvertVariableToFieldAndInsertFieldDeclarationOnLastLineOfClassDeclarations()
        {
            StringTextBuffer testBuffer = new StringTextBuffer(ExpressionInContext("int x = a - b + c * d / e % 3444;"));

            mockView.Setup(m => m.TextBuffer).Returns(testBuffer);
            mockView.Setup(m => m.Caret.Position).Returns(testBuffer.GetPositionAtStartOf("x = a - b"));

            manipulator.ConvertVariableToField();

            Console.Out.Write(testBuffer.CurrentSnapshot.GetText());


            Assert.AreEqual("        private int x;", testBuffer.CurrentSnapshot.Lines.ElementAt(5).GetText());
            Assert.AreEqual("            x = a - b + c * d / e % 3444;", testBuffer.CurrentSnapshot.Lines.ElementAt(9).GetText());
        }

        [TestMethod]
        public void ShouldConvertVariableToFieldAndInsertFieldDeclarationOnLastLineOfClassDeclarationsRemovingSimpleOriginalWhenItDoesntHaveAnAssignment()
        {
            StringTextBuffer testBuffer = new StringTextBuffer(ExpressionInContext("int x;"));

            mockView.Setup(m => m.TextBuffer).Returns(testBuffer);
            mockView.Setup(m => m.Caret.Position).Returns(testBuffer.GetPositionAtStartOf("x;"));

            manipulator.ConvertVariableToField();

            Console.Out.Write(testBuffer.CurrentSnapshot.GetText());


            Assert.AreEqual("        private int x;", testBuffer.CurrentSnapshot.Lines.ElementAt(5).GetText());
            Assert.AreEqual("        {", testBuffer.CurrentSnapshot.Lines.ElementAt(8).GetText());
            Assert.AreEqual("        }", testBuffer.CurrentSnapshot.Lines.ElementAt(9).GetText());
        }

        [TestMethod]
        public void ShouldConvertVariableToFieldAndInsertFieldDeclarationOnLastLineOfClassDeclarationsRemovingSimpleOriginalWhenItDoesntHaveAnAssignmentAndOtherAssignmentsOnTheSameLine()
        {
            StringTextBuffer testBuffer = new StringTextBuffer(ExpressionInContext("string fred=\"fred\"; int x; double jim;"));

            mockView.Setup(m => m.TextBuffer).Returns(testBuffer);
            mockView.Setup(m => m.Caret.Position).Returns(testBuffer.GetPositionAtStartOf("x;"));

            manipulator.ConvertVariableToField();
            Console.Out.Write(testBuffer.CurrentSnapshot.GetText());


            Assert.AreEqual("        private int x;", testBuffer.CurrentSnapshot.Lines.ElementAt(5).GetText());
            Assert.AreEqual("            string fred=\"fred\";  double jim;", testBuffer.CurrentSnapshot.Lines.ElementAt(9).GetText());
        }

        [TestMethod]
        public void ShouldConvertGenericVariableToFieldAndInsertFieldDeclarationOnLastLineOfClassDeclarations()
        {
            StringTextBuffer testBuffer = new StringTextBuffer(ExpressionInContext("List<int> fred = new List<int();"));

            mockView.Setup(m => m.TextBuffer).Returns(testBuffer);
            mockView.Setup(m => m.Caret.Position).Returns(testBuffer.GetPositionAtStartOf("fred"));

            manipulator.ConvertVariableToField();
            Console.Out.Write(testBuffer.CurrentSnapshot.GetText());


            Assert.AreEqual("        private List<int> fred;", testBuffer.CurrentSnapshot.Lines.ElementAt(5).GetText());
            Assert.AreEqual("            fred = new List<int();", testBuffer.CurrentSnapshot.Lines.ElementAt(9).GetText());
        }

        [TestMethod]
        public void ShouldConvertGenericVariableWithMultipleGenericsToFieldAndInsertFieldDeclarationOnLastLineOfClassDeclarations()
        {
            StringTextBuffer testBuffer = new StringTextBuffer(ExpressionInContext("Dictionary<string, int> fred = new Dictionary<String,int>();"));

            mockView.Setup(m => m.TextBuffer).Returns(testBuffer);
            mockView.Setup(m => m.Caret.Position).Returns(testBuffer.GetPositionAtStartOf("fred"));

            manipulator.ConvertVariableToField();
            Console.Out.Write(testBuffer.CurrentSnapshot.GetText());


            Assert.AreEqual("        private Dictionary<string, int> fred;", testBuffer.CurrentSnapshot.Lines.ElementAt(5).GetText());
            Assert.AreEqual("            fred = new Dictionary<String,int>();", testBuffer.CurrentSnapshot.Lines.ElementAt(9).GetText());
        }

        [TestMethod]
        public void ShouldAssignParameterToFieldAndInsertFieldDeclarationOnLastLineOfClassDeclarationsPlusAssignmentInMethod()
        {
            StringTextBuffer testBuffer = new StringTextBuffer(ExpressionInContext("int x = a - b + c * d / e % 3444;"));

            mockView.Setup(m => m.TextBuffer).Returns(testBuffer);
            mockView.Setup(m => m.Caret.Position).Returns(testBuffer.GetPositionAtStartOf("arg1,"));

            manipulator.AssignParameterToField();

            Console.Out.Write(testBuffer.CurrentSnapshot.GetText());


            Assert.AreEqual("        private string[] arg1;", testBuffer.CurrentSnapshot.Lines.ElementAt(5).GetText());
            Assert.AreEqual("            this.arg1 = arg1;", testBuffer.CurrentSnapshot.Lines.ElementAt(10).GetText());
        }

        [TestMethod]
        public void ShouldAssignFinalParameterToFieldAndInsertFieldDeclarationOnLastLineOfClassDeclarationsPlusAssignmentInMethod()
        {
            StringTextBuffer testBuffer = new StringTextBuffer(ExpressionInContext("int x = a - b + c * d / e % 3444;"));

            mockView.Setup(m => m.TextBuffer).Returns(testBuffer);
            mockView.Setup(m => m.Caret.Position).Returns(testBuffer.GetPositionAtStartOf("arg2)"));

            manipulator.AssignParameterToField();

            Console.Out.Write(testBuffer.CurrentSnapshot.GetText());


            Assert.AreEqual("        private int arg2;", testBuffer.CurrentSnapshot.Lines.ElementAt(5).GetText());
            Assert.AreEqual("            this.arg2 = arg2;", testBuffer.CurrentSnapshot.Lines.ElementAt(10).GetText());
        }


        [TestMethod]
        [ExpectedException(typeof(UnrecognisedExpressionException))]
        public void ShouldRejectExpressionSelectionsContainingMismatchedQuotes()
        {
            StringTextBuffer testBuffer = new StringTextBuffer(ExpressionInContext("int x = \" george\".Length * a - b + c * d / e % 3444;"));

            mockView.Setup(m => m.TextBuffer).Returns(testBuffer);
            mockView.Setup(m => m.Selection).Returns(testBuffer.Select("george\".Length * a"));

            manipulator.ExtractVariable();
        }

        [TestMethod]
        [ExpectedException(typeof(UnrecognisedExpressionException))]
        public void ShouldRejectExpressionSelectionsContainingMismatchedBrackets()
        {
            StringTextBuffer testBuffer = new StringTextBuffer(ExpressionInContext("int x = ((y * a) - (b + c)) * d / e % 3444;"));

            mockView.Setup(m => m.TextBuffer).Returns(testBuffer);
            mockView.Setup(m => m.Selection).Returns(testBuffer.Select("(y * a) - (b + c))"));

            manipulator.ExtractVariable();
        }

        [TestMethod]
        [ExpectedException(typeof(UnrecognisedExpressionException))]
        public void ShouldRejectUnfinishedExpressionSelections()
        {
            StringTextBuffer testBuffer = new StringTextBuffer(ExpressionInContext("int x = ((yacht * ant) - (bucket + car)) * d / e % 3444;"));

            mockView.Setup(m => m.TextBuffer).Returns(testBuffer);
            mockView.Setup(m => m.Selection).Returns(testBuffer.Select("bucket + ca"));

            manipulator.ExtractVariable();
        }

        [TestMethod]
        [ExpectedException(typeof(UnrecognisedExpressionException))]
        public void ShouldRejectExpressionSelectionsIncludingAssignment()
        {
            StringTextBuffer testBuffer = new StringTextBuffer(ExpressionInContext("int x = ((yacht * ant) - (bucket + car)) * d / e % 3444;"));

            mockView.Setup(m => m.TextBuffer).Returns(testBuffer);
            mockView.Setup(m => m.Selection).Returns(testBuffer.Select("x = ((yacht * ant) - (bucket + car))"));

            manipulator.ExtractVariable();
        }



        private string ExpressionInContext(string expression, string methodScope = "")
        {
            return String.Format(sampleContext, expression, methodScope);
        }
    
    }
}
