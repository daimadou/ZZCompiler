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
        public HashSet<char> characterSet;
        
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
            edge = (int)EdgeLabel.EPSILON;
            characterSet = null;
            next = null;
            next2 = null;
            index = -1;
            isAccpetState = false;
            anchor = (int)AnchorLabel.NONE;
        }

        public bool copy(NFAState nfastate)
        {
            if (nfastate == null)
            {
                return false;
            }

            this.edge = nfastate.edge;
            this.characterSet = nfastate.characterSet;
            this.next = nfastate.next;
            this.next2 = nfastate.next2;
            this.isAccpetState = nfastate.isAccpetState;
            this.anchor = nfastate.anchor;
            return true;
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

        private void Expr(ref NFAState start, ref NFAState end, ref int index)
        {
            NFAState nextStart = null;
            NFAState nextEnd = null;
            NFAState cur = null;
            CatExpr(ref start, ref end, ref index);
            while(innerExpression[index]=='|')
            {
                index++;
                CatExpr(ref nextStart, ref nextEnd, ref index);
                cur = new NFAState();
                cur.next = start;
                cur.next2 = nextStart;
                start = cur;

                cur = new NFAState();
                cur.next = end;
                cur.next2 = nextEnd;
                end = cur;
            }
        }

        private void CatExpr(ref NFAState start, ref NFAState end, ref int index)
        {
            if(CheckFirstInCatExpr(index))
            {
                factor(ref start, ref end, ref index);    
            }

            NFAState nextStart = null;
            NFAState nextEnd = null;
            while (CheckFirstInCatExpr(index))
            {
                factor(ref nextStart, ref nextEnd, ref index);
                end.copy(nextStart);
                end = nextEnd;

                nextStart = null;
                nextEnd = null;
                
            }
        }

        private bool CheckFirstInCatExpr(int index)
        {
            if (index < innerExpression.Length)
            {
                char c = innerExpression[index];
                switch (c)
                {
                    case ')':
                    case '$':
                    case '|': 
                        return false;
                    case '*':
                    case '+':
                    case '?':
                    case ']':
                    case '^':
                        Console.Error.WriteLine("{0} should not appear here.", c);
                        return false;
                    default:
                        return true;
                }
            }
            else
            {
                return false;
            }

        }

        private void factor(ref NFAState start, ref NFAState end, ref int index)
        {
            NFAState newStart = null;
            NFAState newEnd = null;

            term(ref start, ref end, ref index);
            char c = innerExpression[index++];

            if (c == '*' || c == '+' || c == '?')
            {
                newStart = new NFAState();
                newEnd = new NFAState();

                newStart.next = start;
                end.next = newEnd;

                if (c == '*' || c == '?')
                {
                    newStart.next2 = newEnd;
                }

                if (c == '*' || c == '+')
                {
                    end.next2 = start;
                }

                start = newStart;
                end = newEnd;
            }
        }

        private void term(ref NFAState start, ref NFAState end, ref int index)
        {
            char c = innerExpression[index++];

            if (c == '(')
            {
                Expr(ref start, ref end, ref index);
                char curChar = innerExpression[index++];
                if (innerExpression[index] == ')')
                {
                    return;
                }
                else
                {
                    Console.Error.WriteLine("{0} is invlaid ", curChar);
                    throw new InvalidCharaterException(curChar + " is invlaid");
                }
            }
            else
            {
                start = new NFAState();
                end = new NFAState();
                start.next = end;

                if (!(c == '.' || c == '['))
                {
                    start.edge = c;
                }
                else 
                {
                    start.edge = (int)NFAState.EdgeLabel.CCL;
                    if (c == '.')
                    {
                        start.characterSet = new HashSet<char>();
                        for (int i = 0; i < ' '; i++)
                        {
                            start.characterSet.Add((char)i);
                        }
                    }
                    else
                    {
                        if(c == '[')
                        {
                            if (index < innerExpression.Length)
                            {
                                char curCharater = innerExpression[index++];
                                while (curCharater != ']' || index < innerExpression.Length)
                                {
                                    curCharater = innerExpression[index++]; 
                                }

                                if (curCharater != ']')
                                {
                                    Console.Error.WriteLine("can't find ]");
                                    throw new InvalidCharaterException();
                                }
                            }
                            else 
                            {
                                Console.Error.WriteLine("can't find ]");
                                throw new InvalidCharaterException();
                            }

                        }
                    }
                }
            }
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
