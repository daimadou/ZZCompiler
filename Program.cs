using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ZzCompiler
{
    using SharpLexer;
    class Program
    {

        //task 1: implement the regexp for string and () and empty, write the unite test for them
        static void Main(string[] args)
        {
            //Test1();
            Test2();
            //Test3();
        }

        static void Test1()
        {
            string source = "int";
            Scanner scan = new Scanner();
            //scan.AddMacro("DIGITIAL", "[0123456789]");
            //scan.AddMacro("NUMEMBER", "%DIGITIAL%*");
            //scan.AddMacro("WHITESPACE", "\n| |\r\n");
            //scan.AddRule("%WHITESPACE%+", "WHITESPACES");
            scan.AddMacro("LETTERS", "[abcdefghijklmnopqrstuvwxyzABCEDFGHIJKLMNOPQRSTUVWXYZ]");
            scan.AddRule("INT", "int");
            scan.AddRule("ID", "$(LETTERS)+");
            scan.Generate();
            scan.AddSource(source);
            Token t = scan.ReadOneToken();
            if(t!=null)
                Console.WriteLine(t);
        }

        static void Test3()
        {
            Scanner scan = new Scanner();
            scan.ReadDefFile("./TestData/TokenNameAndExpr.txt");
        }

        static void Test2()
        {
            string source =
@"
int main()
{
    int a;
    int b;
    a = 1 + 2;
    b = a + 2;
    string s = 'Hello World';
    print s;
    return 0;
}
";
            Scanner scan = new Scanner();
            /*
            scan.AddMacro("DIGITIAL", "[0123456789]");
            scan.AddMacro("NUMEMBER", "$(DIGITIAL)*");
            scan.AddMacro("LETTERS", "[abcdefghijklmnopqrstuvwxyzABCEDFGHIJKLMNOPQRSTUVWXYZ]");
            scan.AddMacro("WHITESPACE", "\n| |\r\n");

            scan.AddRule("$(WHITESPACE)+", "WHITESPACES");
            scan.AddRule("int", "INT");
            scan.AddRule("string", "STRING");
            scan.AddRule("return", "RETURN");
            scan.AddRule("$(DIGITIAL)+", "NUM");
            scan.AddRule("\\(", "LPAREN");
            scan.AddRule("\\)", "RPAREN");
            scan.AddRule("{", "LBRACE");
            scan.AddRule("}", "RBRACE");
            scan.AddRule(";", "SEMICOLON");
            scan.AddRule("\\+", "PLUS");
            scan.AddRule("=", "EQ");
            scan.AddRule("'[^'\n\r]*'", "STRING_BODY");
            scan.AddRule("$(LETTERS)+($(DIGITIAL)+|$(LETTERS)+)*", "ID");
             */
            scan.ReadDefFile("./TestData/TokenNameAndExpr.txt");
            scan.Generate();
            scan.AddSource(source);

            Token t = null;
            do
            {
                t = scan.ReadOneTokenWithIgnore("WHITESPACES");
                Console.WriteLine(t);
            } while (t != null);
        }
    }
}
