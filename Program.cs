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
            Program p = new Program();
            p.TestGetTokenNameAndExpr("d*ad|dad*");
            p.TestGetTokenNameAndExpr("a");
        }

        void TestGetTokenNameAndExpr(string s)
        {
            /*
            string[] lines = System.IO.File.ReadAllLines(@".\TestData\TokenNameAndExpr.txt");
            for(int i = 0; i < lines.Length; i++)
            {
                string tokenName = null;
                string expr = null;
                StateMachine.getExpr(out tokenName, out expr, lines[i], i + 1);
                Console.WriteLine("TokenName:{0} Expr:{1}", tokenName, expr);
            }
             */
            NFAMachine nfaMachine = new NFAMachine();

            NFAState NFAStart = nfaMachine.AddRule(s, "test"); ;
            nfaMachine.DumpAllStates();
            NFAState.RefreshID();
            DFAMachine machine2 = new DFAMachine();
            machine2.GenerateDFAMachine(NFAStart);
            machine2.DumpAllStates();
            NFAState.RefreshID();
            DFAMachineMin minMahine = new DFAMachineMin();
            minMahine.MininzeDFAStates(machine2.Contents);
            minMahine.DumpAllStates();
        }
    }
}
