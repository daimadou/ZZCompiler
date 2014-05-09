using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpLexer;
namespace ScannerTest
{
    [TestClass]
  

    public class UnitTest1
    {        
        [TestMethod]
        public void TestRead()
        {
            Assert.IsTrue(new Token("KEYWORD", "int")==TestSingleRead("int", "KEYWORD", "int"));
            Assert.IsTrue(new Token("KEYWORD", "a") == TestSingleRead(".", "KEYWORD", "ai"));
            Assert.IsTrue(new Token("WORD", "hello") == TestSingleRead("[abcdefghijklmnopqrstuvwxyz]*", "WORD", "hello"));
            Assert.IsTrue("z" == TestSingleRead("[abcdefghijklmnopqrstuvwxyz]+", "WORD", "z").Value);
        }

        [TestMethod]
        private Token TestSingleRead(string regex, string TokenName, string input)
        {
            Scanner scan = new Scanner();
            scan.AddRule(regex, TokenName);
            scan.Generate();
            scan.AddSource(input);
            return scan.ReadOneToken();
        }

        [TestMethod]
        public void TestMacro()
        {
            Scanner scan = new Scanner();
            scan.
            scan.AddRule(regex, TokenName);
            scan.Generate();
            scan.AddSource(input);
            return scan.ReadOneToken();
 
        }

        [TestMethod]
        public void TestReadFail()
        {
            Assert.IsNull(TestSingleRead("int", "KEYWORD", "a"));
            Assert.IsNull(TestSingleRead(".", "KEYWORD", ""));
        }
    }
}
