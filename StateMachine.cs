using System;
using System.Collections.Generic;
using System.Collections;
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
        public static int ID = 0;
        public int id;

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
        
        Boolean isAccpetState;
        public string accept;
        public NFAState()
        {
            edge = (int)EdgeLabel.EPSILON;
            characterSet = null;
            next = null;
            next2 = null;
            isAccpetState = false;
            anchor = (int)AnchorLabel.NONE;
            id = ID++;
            accept = null;
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
        int curIndex;
        int moveStep;
        List<NFAState> StateList;
        
        private char GetCurChar()
        {

            char curChar = innerExpression[curIndex];
            if (curChar == '\\')
            {
                if (curIndex + 1 < innerExpression.Length)
                {
                    char ret = innerExpression[curIndex + 1];
                    moveStep = 2;
                    curIndex += moveStep;
                    if (ret == 'n')
                        ret = '\n';
                    return ret;
                }
                else
                {
                    throw new InvalidCharaterException("after \\ no following character");
                }
            }
            else
            {
                moveStep = 1;
                return innerExpression[curIndex++];
            }
            
        }

        private void RevertIndex()
        {
            curIndex -= moveStep;
        }

        private bool IsEnd()
        {
            return curIndex < innerExpression.Length && innerExpression[curIndex] == '$';
        }
        

        public NFAStateMachine()
        {
            StateList = new List<NFAState>();
            curIndex = 0;
            moveStep = 1;
        }

        public NFAState Machine(string input)
        {
            innerExpression = input;
            NFAState start = new NFAState();
            StateList.Add(start);
            NFAState current = start;
            int index = 0;
            current.next = Rule(ref index);

            /*
            while (index < input.Length)
            {
                   
            }
            */
            return start;
        }

        private NFAState Rule(ref int index)
        {
            NFAState start = null;
            NFAState end = null;
         
            if (GetCurChar() == '^')
            {
                start = new NFAState();
                StateList.Add(start);
                start.edge = '\n';
                Expr(ref start.next, ref end, ref index);
            }
            else
            {
                RevertIndex();
                Expr(ref start, ref end, ref index);
            }

            if (IsEnd())
            {
                end.next = new NFAState();
                StateList.Add(end);
                end.edge = '\n'; //maybe have a bug
                end = end.next;
            }

            end.accept = innerExpression;
            /*
            while (index < innerExpression.Length && Char.IsWhiteSpace(innerExpression[index]))
            {
                index++;
            }
             */
            return start;
        }

        private void Expr(ref NFAState start, ref NFAState end, ref int index)
        {
            NFAState nextStart = null;
            NFAState nextEnd = null;
            NFAState cur = null;
            CatExpr(ref start, ref end, ref index);

            while (curIndex < innerExpression.Length && GetCurChar() == '|')
            {
                index++;
                CatExpr(ref nextStart, ref nextEnd, ref index);
                cur = new NFAState();
                StateList.Add(cur);
                cur.next = start;
                cur.next2 = nextStart;
                start = cur;

                cur = new NFAState();
                StateList.Add(cur);
                end.next = cur;
                nextEnd.next = cur;
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
                StateList.Remove(nextStart);
                end = nextEnd;

                nextStart = null;
                nextEnd = null;
                
            }
        }

        private bool CheckFirstInCatExpr(int index)
        {
            if (curIndex < innerExpression.Length)
            {
                char c = innerExpression[curIndex];
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
            char c = GetCurChar();

            if (c == '*' || c == '+' || c == '?')
            {
                newStart = new NFAState();
                newEnd = new NFAState();
                StateList.Add(newStart);
                StateList.Add(newEnd);

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
            else
            {
                RevertIndex();
            }
        }

        private void term(ref NFAState start, ref NFAState end, ref int index)
        {
            char c = GetCurChar();

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
                StateList.Add(start);
                StateList.Add(end);
                start.next = end;

                if (!(c == '.' || c == '['))
                {
                    if (moveStep == 2)
                    {
                        switch (c)
                        {
                            case 'w':
                                AddLetters(start);
                                break;
                            case 'd':
                                AddDigits(start);
                                break;
                            default:
                                start.edge = c;
                                break;
                        }
                    }
                    else
                    {
                        start.edge = c;
                    }
                }
                else 
                {
                    AddSymbolsFromTable(start, c);
                }
            }
        }

        private void AddSymbolsFromTable(NFAState start, char c)
        {
            start.edge = (int)NFAState.EdgeLabel.CCL;
            start.characterSet = new HashSet<char>();
            if (c == '.')
            {

                for (int i = 0; i < 255; i++)
                {
                    start.characterSet.Add((char)i);
                }
            }
            else
            {
                if (c == '[')
                {
                    if (curIndex < innerExpression.Length)
                    {
                        char curCharater = GetCurChar();
                        do
                        {
                            if (curCharater != ']')
                            {
                                start.characterSet.Add(curCharater);
                            }
                            else
                            {
                                break;
                            }
                            curCharater = GetCurChar();
                        } while (curIndex < innerExpression.Length + 1);

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

        private void AddDigits(NFAState state)
        {
            state.edge = (int)NFAState.EdgeLabel.CCL;
            state.characterSet = new HashSet<char>();
            for (int i = 0; i < 10; i++)
            {
                state.characterSet.Add((char)i);
            }
        }

        private void AddLetters(NFAState state)
        {
            state.edge = (int)NFAState.EdgeLabel.CCL;
            state.characterSet = new HashSet<char>();
            for (char i = 'a'; i <= 'z'; i++)
            {
                state.characterSet.Add(i);
            }

            for (char i = 'A'; i <= 'Z'; i++)
            {
                state.characterSet.Add(i);
            }
        }

        public static HashSet<NFAState> GetEClousre(HashSet<NFAState> inputStates, ref string acceptStr)
        {
          
            if (inputStates.Count == 0)
            {
                return new HashSet<NFAState>();
            }

            HashSet<NFAState> checkSet = new HashSet<NFAState>();
            Stack<NFAState> stateStack = new Stack<NFAState>();
            foreach (var v in inputStates)
            {
                stateStack.Push(v);
                checkSet.Add(v);
            }

            while (stateStack.Count > 0)
            {
                NFAState s = stateStack.Pop();
                checkSet.Add(s);
                if (s.accept != null)
                {
                    acceptStr = s.accept;
                }

                if (s.edge == (int)NFAState.EdgeLabel.EPSILON)
                {
                    if (s.next != null) 
                    {
                        stateStack.Push(s.next);
                    }

                    if (s.next2 != null)
                    {
                        stateStack.Push(s.next2);
                    }
                }
            }
            return checkSet;
        }

        public HashSet<NFAState> Move(List<NFAState> states, char inputChar)
        {
            HashSet<NFAState> ret = new HashSet<NFAState>();
            foreach(var s in states)
            {
                if (s.edge == inputChar || (s.edge == (int)NFAState.EdgeLabel.CCL &&　s.characterSet.Contains(inputChar)))
                {
                    ret.Add(s.next);
                }
            }
            return ret;
        }

        public void DumpAllStates()
        {
            Console.WriteLine("Expression:"+innerExpression);
            foreach (NFAState state in StateList)
            {

                string type = state.edge < 0 ? ((NFAState.EdgeLabel)state.edge).ToString() : ((char)state.edge).ToString(); ;
                Console.WriteLine("State ID:{0} Type:{1}", state.id, type);
                if(state.next!=null)
                {
                    type = state.next.edge < 0 ? ((NFAState.EdgeLabel)state.next.edge).ToString() : ((char)state.next.edge).ToString();
                    Console.WriteLine("---->next ID:{0} Type:{1}", state.next.id, type);
                }
                if(state.next2!=null)
                {
                    type = state.next2.edge < 0 ? ((NFAState.EdgeLabel)state.next2.edge).ToString() : ((char)state.next2.edge).ToString();
                    Console.WriteLine("---->next ID:{0} Type:{1}", state.next2.id, type);
                }
        
            }
        }
    }

    class DFAState
    {
        private static int Count = 0;
        public int ID;
        public bool Mark;
        public HashSet<NFAState> NfaStatesSet;
        Dictionary<char, DFAState> NextStateTable;
        public DFAState()
        {
            ID = Count++;
            Mark = false;
            NfaStatesSet = null;
            NextStateTable = new Dictionary<char,DFAState>();
        }

        public DFAState this[char c]
        {
            get
            {
                return NextStateTable[c];
            }
            set
            {
                if(NextStateTable.ContainsKey(c))
                {
                    throw new Exception("DFA state error");
                }
                else
                {
                    NextStateTable.Add(c, value);
                }
            }
        }
    }

    class DFAStateMachine
    {
        static DFAState GenerateDFAMachine(NFAState state)
        {
            DFAState start = new DFAState();
            DFAState cur = start;
            Queue<DFAState> UnmarkQueue = new Queue<DFAState>();
            UnmarkQueue.Enqueue(cur);
            String acceptStr = null;
            //cur.NfaStatesSet = NFAStateMachine.GetEClousre(new []{state}.ToList(), ref acceptStr);
            return start;
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
 
}
