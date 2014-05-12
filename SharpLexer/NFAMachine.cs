using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpLexer
{
    public class NFAStateMachine
    {
        string InnerExpression;
        int CurIndex;
        int MoveSteps;
        List<NFAState> Contents;
        NFAState PreStart;
        
        Dictionary<string, string> Macros;
        public NFAStateMachine()
        {
            Contents = new List<NFAState>();
            CurIndex = 0;
            MoveSteps = 1;
        }

        private char GetCurChar()
        {

            char curChar = InnerExpression[CurIndex];
            if (curChar == '\\')
            {
                if (CurIndex + 1 < InnerExpression.Length)
                {
                    char ret = InnerExpression[CurIndex + 1];
                    MoveSteps = 2;
                    CurIndex += MoveSteps;
                    if (ret == 'n')
                    {
                        ret = '\n';
                    }
                    else if(ret == 'r')
                    {
                        ret = '\r';
                    }
                    return ret;
                }
                else
                {
                    throw new InvalidCharaterException("after \\ no following character");
                }
            }
            else
            {
                MoveSteps = 1;
                return InnerExpression[CurIndex++];
            }

        }

        private void RevertIndex()
        {
            CurIndex -= MoveSteps;
        }

        public void AddMacro(string macroName, string regex)
        {
            if (Macros == null)
            {
                Macros = new Dictionary<string, string>();
            }
            Macros.Add(macroName, regex);
        }
       
        public NFAState AddRule(string regex, string tokenName)
        {
            NFAState start = null;
            NFAState end = null;
            GenerateInnerExpression(regex, 0);
           
            if (GetCurChar() == '^')
            {
                start = new NFAState();
                Contents.Add(start);
                start.AddChar('\n');
                Expr(ref start.Next, ref end);
            }
            else
            {
                RevertIndex();
                Expr(ref start, ref end);
            }
            /*
            if (CurIndex < InnerExpression.Length && InnerExpression[CurIndex] == '$')
            {
                end.Next = new NFAState();
                end.AddChar('\n'); //maybe have a bug
                end = end.Next;
                Contents.Add(end);
            }
            */
            end.Accept = tokenName;
            NFAState ret = new NFAState();
            ret.Next = start;
            ret.Next2 = this.PreStart;
            this.PreStart = ret;
            return ret;
        }

        private void GenerateInnerExpression(string regex, int index)
        {
            this.InnerExpression = regex;
            this.CurIndex = index;
            this.MoveSteps = 1;
        }

        private void Expr(ref NFAState start, ref NFAState end)
        {
            NFAState nextStart = null;
            NFAState nextEnd = null;
            NFAState cur = null;
            CatExpr(ref start, ref end);

            while (CurIndex < InnerExpression.Length && ChurChar == '|')
            {
                GetCurChar();
                CatExpr(ref nextStart, ref nextEnd);
                cur = new NFAState();
                Contents.Add(cur);
                cur.Next = start;
                cur.Next2 = nextStart;
                start = cur;

                cur = new NFAState();
                Contents.Add(cur);
                end.Next = cur;
                nextEnd.Next = cur;
                end = cur;
            }
        }

        private char ChurChar
        {
            get { return InnerExpression[CurIndex]; }
        }

        private void CatExpr(ref NFAState start, ref NFAState end)
        {
            if (CheckFirstInCatExpr())
            {
                factor(ref start, ref end);
            }

            NFAState nextStart = null;
            NFAState nextEnd = null;
            while (CheckFirstInCatExpr())
            {
                factor(ref nextStart, ref nextEnd);
                end.copy(nextStart);
                Contents.Remove(nextStart);
                
                end = nextEnd;
                nextStart = null;
                nextEnd = null;
            }
        }

        private bool CheckFirstInCatExpr()
        {
            if (CurIndex < InnerExpression.Length)
            {
                char c = InnerExpression[CurIndex];
                switch (c)
                {
                    case ')':
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

        private void factor(ref NFAState start, ref NFAState end)
        {
            NFAState newStart = null;
            NFAState newEnd = null;

            term(ref start, ref end);
            if (CurIndex < InnerExpression.Length)
            {
                char c = GetCurChar();
                if (c == '*' || c == '+' || c == '?')
                {
                    newStart = new NFAState();
                    newEnd = new NFAState();
                    Contents.Add(newStart);
                    Contents.Add(newEnd);

                    newStart.Next = start;
                    end.Next = newEnd;

                    if (c == '*' || c == '?')
                    {
                        newStart.Next2 = newEnd;
                    }

                    if (c == '*' || c == '+')
                    {
                        end.Next2 = start;
                    }

                    start = newStart;
                    end = newEnd;
                }
                else
                {
                    RevertIndex();
                }
            }
        }

        private void term(ref NFAState start, ref NFAState end)
        {
            char c = GetCurChar();

            if (c == '('&&MoveSteps==1)
            {
                Expr(ref start, ref end);
                if (this.CurIndex >= InnerExpression.Length)
                {
                    Console.Error.WriteLine("No ) for previous (");
                    throw new InvalidCharaterException("No ) for previous (");
                }
                c = GetCurChar();
                if (c == ')'&& MoveSteps==1)
                {
                    return;
                }
                else
                {
                    Console.Error.WriteLine("{0} is invlaid ", c);
                    throw new InvalidCharaterException(c + " is invlaid");
                }
            }
            else if (c == '$' && MoveSteps == 1 )
            {
                c = GetCurChar();
                if (c == '('&&MoveSteps==1)
                {
                    StringBuilder sb = new StringBuilder();
                    while (this.CurIndex < InnerExpression.Length)
                    {
                        c = GetCurChar();
                        if (c == ')' && MoveSteps == 1)
                        {
                            break;
                        }
                        else
                        {
                            sb.Append(c);
                        }
                    }
                    
                    if(c==')'&& MoveSteps==1 && sb.Length > 0)
                    {
                        int preIndex = CurIndex;
                        string preInnerExpression = this.InnerExpression;
                        string regex = Macros[sb.ToString()];
                        this.GenerateInnerExpression(regex, 0);
                        Expr(ref start, ref end);
                        this.GenerateInnerExpression(preInnerExpression, preIndex);
                    }
                }

            }
            else
            {
                start = new NFAState();
                end = new NFAState();
                Contents.Add(start);
                Contents.Add(end);
                start.Next = end;

                if (!(c == '.' || c == '[')||MoveSteps==2)
                {
                    start.AddChar(c);
                }
                else
                {
                    AddSymbolsFromTable(start, c);
                }
            }
        }

        private void AddSymbolsFromTable(NFAState start, char c)
        {
            if (c == '.' && MoveSteps == 1)
            {

                for (char i = (char)0; i < 256; i++)
                {
                    start.AddChar(i);
                }
            }
            else
            {
                if (c == '[' && MoveSteps == 1)
                {
                    if (CurIndex < InnerExpression.Length)
                    {
                        c = GetCurChar();
                        char firstChar = c;
                        int firstMoveSteps = MoveSteps;
                        HashSet<char> charset = new HashSet<char>();
                        while (CurIndex < InnerExpression.Length + 1)
                        {
                            if (!(c ==']'&& MoveSteps==1))
                            {
                                if (!(c == '^' && MoveSteps == 1))
                                {
                                    charset.Add(c);
                                }
                            }
                            else
                            {
                                break;
                            }
                            c = GetCurChar();
                        } 

                        if (!(c == ']'&& MoveSteps==1))
                        {
                            Console.Error.WriteLine("can't find ]");
                            throw new InvalidCharaterException();
                        }

                        if (firstChar == '^' && firstMoveSteps == 1)
                        {
                            for (char i = (char)0; i < 256; i++)
                            {
                                if (!charset.Contains(i))
                                {
                                    start.AddChar(i);
                                }
                            }
                        }
                        else
                        {
                            charset.Add(firstChar);
                            start.ReplaceCharSet(charset);
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
            for (int i = 0; i < 10; i++)
            {
                state.AddChar((char)('0'+i));
            }
        }

        private void AddLetters(NFAState state)
        {
            for (char i = 'a'; i <= 'z'; i++)
            {
                state.AddChar(i);
            }

            for (char i = 'A'; i <= 'Z'; i++)
            {
                state.AddChar(i);
            }
        }

        public static HashSet<NFAState> GetEClousre(HashSet<NFAState> inputStates, ref string acceptStr, Dictionary<string, int> PriorityTable)
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
                if (s.Accept != null)
                {
                    if (acceptStr != null)
                    {
                        acceptStr = PriorityTable[acceptStr] < PriorityTable[s.Accept] ? acceptStr : s.Accept;
                    }
                    else
                    {
                        acceptStr = s.Accept;
                    }
                }

                if (s.Edge == NFAState.EdgeLabel.EPSILON)
                {
                    if (s.Next != null)
                    {
                        stateStack.Push(s.Next);
                    }

                    if (s.Next2 != null)
                    {
                        stateStack.Push(s.Next2);
                    }
                }
            }
            return checkSet;
        }

        public static HashSet<NFAState> Move(HashSet<NFAState> states, char inputChar)
        {
            HashSet<NFAState> ret = null;
            foreach (var s in states)
            {
                if (s.Contains(inputChar))
                {
                    if (ret == null)
                    {
                        ret = new HashSet<NFAState>();
                    }
                    ret.Add(s.Next);
                }
            }
            return ret;
        }

        public void DumpAllStates()
        {
            //Console.WriteLine("Expression:" + InnerExpression);
            foreach (NFAState state in Contents)
            {
                Console.WriteLine(state);
            }
        }
    }
}
