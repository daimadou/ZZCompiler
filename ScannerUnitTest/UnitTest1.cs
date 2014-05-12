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
            Assert.IsTrue(new Token("KEYWORD", "int").Equals(TestSingleRead("int", "KEYWORD", "int")));
            Assert.IsTrue(new Token("KEYWORD", "a").Equals(TestSingleRead(".", "KEYWORD", "ai")));
            Assert.IsTrue(new Token("WORD", "hello").Equals(TestSingleRead("[abcdefghijklmnopqrstuvwxyz]*", "WORD", "hello")));
            Assert.IsTrue("z" == TestSingleRead("[abcdefghijklmnopqrstuvwxyz]+", "WORD", "z").Value);
            Assert.IsTrue(new Token("NAME", "zhuzhichenglan").Equals(TestSingleRead("zhu(zhicheng|lan)*", "NAME", "zhuzhichenglan")));

        }

        [TestMethod]
        private Token TestSingleRead(string regex, string TokenName, string input)
        {
            Scanner scan = new Scanner();
            scan.AddRule(TokenName, regex);
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
            scan.AddRule("num", "$(Digital)*");
            scan.AddRule("WHITESPACES", "$(WHITESPACE)+");
            scan.Generate();
            scan.AddSource("123                    25");
            Token t1 = scan.ReadOneToken();
            Token t2 = scan.ReadOneToken();
            Token t3 = scan.ReadOneToken();
            Assert.IsTrue(new Token("num", "123").Equals(t1));
            Assert.IsTrue(new Token("num", "25").Equals(t3));
            scan = new Scanner();
            scan.AddMacro("LETTER", "[abcdefghijklmnopqrstuvwxyz]");
            scan.AddMacro("DIGITAL", "[1234567890]");
            scan.AddMacro("WORD", "$(LETTER)+");
            scan.AddMacro("NUM", "$(DIGITAL)+");
            scan.AddRule("ID", "$(WORD)($(WORD)|$(DIGITAL))*");
            scan.Generate();
            scan.AddSource("zzhu9");
            t1 = scan.ReadOneToken();
            Assert.IsTrue(new Token("ID", "zzhu9").Equals(t1));
        }
        
        [TestMethod]
        public void ComplexTest()
        {
            string source = "int";
            Scanner scan = new Scanner();
            scan.AddMacro("DIGITIAL", "[0123456789]");
            scan.AddMacro("NUMEMBER", "$(DIGITIAL)*");
            scan.AddMacro("LETTERS", "[abcdefghijklmnopqrstuvwxyzABCEDFGHIJKLMNOPQRSTUVWXYZ]");
            scan.AddMacro("WHITESPACE", "\n| |\r\n");

            scan.AddRule("WHITESPACES", "$(WHITESPACE)+");
            scan.AddRule("INT", "int");
            scan.AddRule("ID", "$(LETTERS)+($(DIGITIAL)+|$(LETTERS)+)*");
            scan.Generate();
            scan.AddSource(source);
            Token t = scan.ReadOneToken();
            Assert.IsTrue("INT".Equals(t.TokenName));
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
