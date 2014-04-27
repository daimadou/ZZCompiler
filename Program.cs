using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ZzCompiler
{
    class Program
    {

        //task 1: implement the regexp for string and () and empty, write the unite test for them
        static void Main(string[] args)
        {
            Program p = new Program();
            p.TestGetTokenNameAndExpr();
        }

        void TestGetTokenNameAndExpr()
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
            NFAStateMachine machine = new NFAStateMachine();
            NFAState NFAStart = machine.Machine("d*ad|dad*");
            machine.DumpAllStates();
            DFAStateMachine machine2 = new DFAStateMachine();
            machine2.GenerateDFAMachine(NFAStart);
            machine2.DumpAllStates();
        }
    }
}
