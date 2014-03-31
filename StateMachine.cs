using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace ZzCompiler
{

    class NFAState
    {
        public enum EdgeLabel { EMPTY = -3, CCL = -2, EPSILON = -1 };
        public enum AnchorLabel { NONE = 0, START, END };
        public int edge{set; get;}
        HashSet<char> characterSet;
        
        public NFAState next;
        public NFAState next2;
        int anchor;
        public int Anchor
        {
            set
            {
                anchor = value;
            }
            get
            {
                return anchor;
            }
        }
        
        int index;
        Boolean isAccpetState;
        public NFAState()
        {
            edge = (int)EdgeLabel.EMPTY;
            characterSet = new HashSet<char>();
            next = null;
            next2 = null;
            index = -1;
            isAccpetState = false;
            anchor = (int)AnchorLabel.NONE;
        }
    }
    

    class NFAStateMachine 
    {
        string innerExpression;
        private NFAState Machine(string input)
        {
            innerExpression = input;
            NFAState start = new NFAState();
            NFAState current = start;
            current.edge = (int)NFAState.EdgeLabel.EPSILON;
            int index = 0;
            current.next = Rule(ref index);
            while (index < input.Length)
            {
                   
            }
            
            return start;
        }

        private NFAState Rule(ref int index)
        {
            NFAState start = null;
            NFAState end = null;

            if (innerExpression[index] == '^')
            {
                start = new NFAState();
                start.edge = '\n';
                index++;
                Expr(ref start.next, ref end, ref index);
            }
            else
            {
                Expr(ref start, ref end, ref index);
            }

            if (index < innerExpression.Length && innerExpression[index] == '$')
            {
                end.next = new NFAState();
                end.edge = '\n';
                end = end.next;
                index++;
            }

            while (index < innerExpression.Length && Char.IsWhiteSpace(innerExpression[index]))
            {
                index++;
            }
            return start;
        }

        private NFAState Expr(ref NFAState start, ref NFAState end, ref int index)
        {
            return null;
        }

        private NFAState CatExpr(ref NFAState start, ref NFAState end)
        {
            return null;
        }
    }
    
    class StateMachine
    {



        static Regex TokenPattern = new Regex(@"(^(TOKENNAME:\s*(?<TokenName>\w+)\s+EXPRESSION:\s*(?<Expr>.*)\s*)$)|(^\s*(\/\/.*)?$)");
        public static void getExpr(out string TokenName, out string Expr, string inputLine, int LineNum)
        {
            Match mc = TokenPattern.Match(inputLine);
            TokenName = mc.Groups["TokenName"].Value;
            Expr = mc.Groups["Expr"].Value;
            if (!mc.Success)
            {
                Console.Error.WriteLine("input formate wrong in Line: " + LineNum);
            }
        }

    }
    class DFAStateMachine : StateMachine
    {
 
    }
}
