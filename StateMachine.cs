using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace ZzCompiler
{

    class StateMachine
    {
        enum EdgeLabel { EMPTY = -3, CCL = -2, EPSILON = -1 };
        class NFAState 
        {
            int edge;
            HashSet<char> characterSet;
            NFAState next;
            NFAState next2;
            int index;
            Boolean isAccpetState;
            NFAState()    
            {
                edge = (int)EdgeLabel.EMPTY;
                characterSet = new HashSet<char>();
                next = null;
                next2 = null;
                index = -1;
                isAccpetState = false;
            }
        }

        static Regex TokenPattern = new Regex(@"(^(TOKENNAME:\s*(?<TokenName>\w+)\s+EXPRESSION:\s*(?<Expr>.*)\s*)$)|(^\s*(\/\/.*)?$)", RegexOptions.IgnoreCase);
        static Regex CommentPattern = new Regex(@"^\/\/.*$");
        static Regex EmptyLinePattern = new Regex(@"^[ ]*$");
        public static void getExpr(out string TokenName, out string Expr, string inputLine, int LineNum)
        {
            Match mc = TokenPattern.Match(inputLine);
            TokenName = mc.Groups["TokenName"].Value;
            Expr = mc.Groups["expr"].Value;
            if(!mc.Success)
            {
                Console.Error.WriteLine("input formate wrong in Line: " + LineNum);
            }
        }
    }
    class NFAStateMachine : StateMachine
    {
        
    }

    class DFAStateMachine : StateMachine
    {
 
    }
}
