using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpLexer;

namespace ScannerUnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestRead()
        {
            Assert.IsTrue(new Token("KEYWORD", "int") == TestSingleRead("int", "KEYWORD", "int"));
            Assert.IsTrue(new Token("KEYWORD", "a") == TestSingleRead(".", "KEYWORD", "ai"));
            Assert.IsTrue(new Token("WORD", "hello") == TestSingleRead("[abcdefghijklmnopqrstuvwxyz]*", "WORD", "hello"));
            Assert.IsTrue("z" == TestSingleRead("[abcdefghijklmnopqrstuvwxyz]+", "WORD", "z").Value);
            Assert.IsTrue(new Token("NAME", "zhuzhichenglan") == TestSingleRead("zhu(zhicheng|lan)*", "NAME", "zhuzhichenglan"));

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
            scan.AddMacro("Digital", "[1234567890]");
            scan.AddMacro("WHITESPACE", "\n| |(\r\n)");
            scan.AddRule("%Digital%*", "num");
            scan.AddRule("%WHITESPACE%+", "WHITESPACES");
            scan.Generate();
            scan.AddSource("123                    25");
            Token t1 = scan.ReadOneToken();
            Token t2 = scan.ReadOneToken();
            Token t3 = scan.ReadOneToken();
            Assert.IsTrue(new Token("num", "123") == t1);
            Assert.IsTrue(new Token("num", "25") == t3);
            scan = new Scanner();
            scan.AddMacro("LETTER", "[abcdefghijklmnopqrstuvwxyz]");
            scan.AddMacro("DIGITAL", "[1234567890]");
            scan.AddMacro("WORD", "%LETTER%+");
            scan.AddMacro("NUM", "%DIGITAL%+");
            scan.AddRule( "%WORD%(%WORD%|%DIGITAL%)*", "ID");
            scan.Generate();
            scan.AddSource("zzhu9");
            t1 = scan.ReadOneToken();
            Assert.IsTrue(new Token("ID", "zzhu9")==t1);
        }


        

        [TestMethod]
        public void TestReadFail()
        {
            Assert.IsNull(TestSingleRead("int", "KEYWORD", "a"));
            Assert.IsNull(TestSingleRead(".", "KEYWORD", ""));
            Assert.IsNull(TestSingleRead("[^\\^]", "s", "^"));
            Assert.IsNull(TestSingleRead("[^\\]]", "s", "]"));
        }
    }

}
