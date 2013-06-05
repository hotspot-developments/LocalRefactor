using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HotspotDevelopments.LocalRefactor;
using Microsoft.VisualStudio.Text;

namespace LocalRefactor_UnitTests
{
    [TestClass]
    public class CodeNavigatorTests
    {
        private ITextSnapshot snapshot;
        private StringTextBuffer buffer;
        private const string sampleContext = "\nnamespace LocalRefactor" +              // 1
        "\n{" +                                                                         // 2
        "\n    public class Test" +                                                     // 3
        "\n    {" +                                                                     // 4
        "\n        private string fred = \"fred\";" +                                   // 5
        "\n        public void Foo(string[] arg1, int arg2)" +                          // 6
        "\n        {" +                                                                 // 7
        "\n             // straight forward comment with brackets" +                    // 8
        "\n             string sample = \"String with { braced content }\";" +          // 9
        "\n             int i; // line comment \" with a stray quote." +                //10
        "\n             string gotcha = \"A string with a // line comment.\"; int j;" + //11
        "\n        }" +                                                                 //12
        "\n    }" +                                                                     //13
        "\n    public class Test2" +                                                    //14
        "\n    {" +                                                                     //15
        "\n        private string fred = \"fred\";" +                                   //16
        "\n        public void Foo(string[] arg1, int arg2)" +                          //17
        "\n        {" +                                                                 //18
        "\n             if (arg2 > 10) { " +                                            //19
        "\n                 foreach(string s in arg1) {" +                              //20
        "\n                     Console.Out.Writeline(\"Stuff\");" +                    //21
        "\n                 }" +                                                        //22
        "\n             }" +                                                            //23
        "\n        }" +                                                                 //24
        "\n    }" +                                                                     //25
        "\n}"                                                                           //26
        ;

        [TestInitialize]
        public void SetupViewWithSnapshot()
        {
            buffer = new StringTextBuffer(sampleContext);
            snapshot = buffer.CurrentSnapshot;
        }


        [TestMethod]
        public void ShouldNavigateUpOneLevel()
        {
            int p = snapshot.GetLineFromLineNumber(3).End - 1;
            CodeNavigator navigator = new CodeNavigator(snapshot);

            Stack<int> positions = new Stack<int>();
            foreach (int c in navigator.UpFrom(p))
            {
                positions.Push(c);
            }

            Assert.AreEqual(2, positions.Count);

            Assert.AreEqual(0, positions.Pop());

            VerifyCharAndLineAtPosition(positions.Pop(), '{', 2);
        }

        [TestMethod]
        public void ShouldNavigateUpThroughCodeIgnoringCommentsAndStrings()
        {
            int p = snapshot.GetLineFromLineNumber(10).End - 1;
            CodeNavigator navigator = new CodeNavigator(snapshot);

            Stack<int> positions = new Stack<int>();
            foreach (int c in navigator.UpFrom(p))
            {
                positions.Push(c);
            }

            Assert.AreEqual(0, positions.Pop());

            VerifyCharAndLineAtPosition(positions.Pop(), '{', 2);
            VerifyCharAndLineAtPosition(positions.Pop(), '{', 4);
            VerifyCharAndLineAtPosition(positions.Pop(), ';', 5);
            VerifyCharAndLineAtPosition(positions.Pop(), '{', 7);
            VerifyCharAndLineAtPosition(positions.Pop(), ';', 9);
            VerifyCharAndLineAtPosition(positions.Pop(), ';', 10);

        }

        [TestMethod]
        public void ShouldNavigateUpThroughCodeCopingWithStringsWithLineComments()
        {
            int p = snapshot.GetLineFromLineNumber(11).End - 1;
            CodeNavigator navigator = new CodeNavigator(snapshot);

            Stack<int> positions = new Stack<int>();
            foreach (int c in navigator.UpFrom(p))
            {
                positions.Push(c);
            }

            Assert.AreEqual(0, positions.Pop());
            VerifyCharAndLineAtPosition(positions.Pop(), '{', 2);
            VerifyCharAndLineAtPosition(positions.Pop(), '{', 4);
            VerifyCharAndLineAtPosition(positions.Pop(), ';', 5);
            VerifyCharAndLineAtPosition(positions.Pop(), '{', 7);
            VerifyCharAndLineAtPosition(positions.Pop(), ';', 9);
            VerifyCharAndLineAtPosition(positions.Pop(), ';', 10);
            VerifyCharAndLineAtPosition(positions.Pop(), ';', 11);
            VerifyCharAndLineAtPosition(positions.Pop(), ';', 11);

        }

        [TestMethod]
        public void ShouldNavigateUpThroughCodeIgnoringCodeAtDeeperScope()
        {
            int p = snapshot.GetLineFromLineNumber(21).End - 1;
            CodeNavigator navigator = new CodeNavigator(snapshot);

            Stack<int> positions = new Stack<int>();
            foreach (int c in navigator.UpFrom(p))
            {
                positions.Push(c);
            }

            Assert.AreEqual(0, positions.Pop());
            VerifyCharAndLineAtPosition(positions.Pop(), '{', 2);
            VerifyCharAndLineAtPosition(positions.Pop(), '{', 15);
            VerifyCharAndLineAtPosition(positions.Pop(), ';', 16);
            VerifyCharAndLineAtPosition(positions.Pop(), '{', 18);
            VerifyCharAndLineAtPosition(positions.Pop(), '{', 19);
            VerifyCharAndLineAtPosition(positions.Pop(), '{', 20);
            VerifyCharAndLineAtPosition(positions.Pop(), ';', 21);

        }

        [TestMethod]
        public void ShouldNavigateDownThroughCodeIgnoringCommentsAndStrings()
        {
            int p = snapshot.GetLineFromLineNumber(17).End - 1;
            CodeNavigator navigator = new CodeNavigator(snapshot);

            List<int> positions = new List<int>();
            foreach (int c in navigator.DownFrom(p))
            {
                positions.Add(c);
            }
            Assert.AreEqual(2, positions.Count);
            VerifyCharAndLineAtPosition(positions[0], '}', 23);
            VerifyCharAndLineAtPosition(positions[1], '}', 24);
        }


        [TestMethod]
        public void ShouldNavigateDownThroughCodeIncludingStatementsInSameScope()
        {
            int p = snapshot.GetLineFromLineNumber(6).End - 1;
            CodeNavigator navigator = new CodeNavigator(snapshot);

            List<int> positions = new List<int>();
            foreach (int c in navigator.DownFrom(p))
            {
                positions.Add(c);
            }
            Assert.AreEqual(5, positions.Count);
            VerifyCharAndLineAtPosition(positions[0], ';', 9);
            VerifyCharAndLineAtPosition(positions[1], ';', 10);
            VerifyCharAndLineAtPosition(positions[2], ';', 11);
            VerifyCharAndLineAtPosition(positions[3], ';', 11);
            VerifyCharAndLineAtPosition(positions[4], '}', 12);
        }



        [TestMethod]
        public void ShouldIdentifyPositionsAtClassScope()
        {
            CodeNavigator navigator = new CodeNavigator(snapshot);
            Assert.IsFalse(navigator.IsInClassScope(snapshot.GetLineFromLineNumber(1).End - 1));
            Assert.IsFalse(navigator.IsInClassScope(snapshot.GetLineFromLineNumber(2).End - 1));
            Assert.IsFalse(navigator.IsInClassScope(snapshot.GetLineFromLineNumber(3).End - 1));
            Assert.IsFalse(navigator.IsInClassScope(snapshot.GetLineFromLineNumber(4).Start ));
 
            Assert.IsTrue(navigator.IsInClassScope(snapshot.GetLineFromLineNumber(4).End - 1));
            Assert.IsTrue(navigator.IsInClassScope(snapshot.GetLineFromLineNumber(5).End - 1));
            Assert.IsTrue(navigator.IsInClassScope(snapshot.GetLineFromLineNumber(6).End - 1));

            Assert.IsFalse(navigator.IsInClassScope(snapshot.GetLineFromLineNumber(7).End - 1));
            Assert.IsFalse(navigator.IsInClassScope(snapshot.GetLineFromLineNumber(8).End - 1));
            Assert.IsFalse(navigator.IsInClassScope(snapshot.GetLineFromLineNumber(9).End - 1));
            Assert.IsFalse(navigator.IsInClassScope(snapshot.GetLineFromLineNumber(10).End - 1));
            Assert.IsFalse(navigator.IsInClassScope(snapshot.GetLineFromLineNumber(11).End - 1));

            Assert.IsTrue(navigator.IsInClassScope(snapshot.GetLineFromLineNumber(12).End - 1));
            Assert.IsTrue(navigator.IsInClassScope(snapshot.GetLineFromLineNumber(13).Start));

            Assert.IsFalse(navigator.IsInClassScope(snapshot.GetLineFromLineNumber(13).End - 1));
            Assert.IsFalse(navigator.IsInClassScope(snapshot.GetLineFromLineNumber(14).End - 1));
            Assert.IsFalse(navigator.IsInClassScope(snapshot.GetLineFromLineNumber(15).Start));

            Assert.IsTrue(navigator.IsInClassScope(snapshot.GetLineFromLineNumber(15).End - 1));
            Assert.IsTrue(navigator.IsInClassScope(snapshot.GetLineFromLineNumber(16).End - 1));
            Assert.IsTrue(navigator.IsInClassScope(snapshot.GetLineFromLineNumber(17).End - 1));
        }

        private void VerifyCharAndLineAtPosition(int p, char ch, int line)
        {
            Assert.AreEqual(ch, snapshot[p]);
            Assert.AreEqual(line, snapshot.GetLineNumberFromPosition(p));
        }
    }
}
