﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpLexer
{
    public class NFAMachine
    {
        string InnerExpression;
        int CurIndex;
        int MoveSteps;
        List<NFAState> Contents;
        NFAState PreStart;
        public NFAMachine()
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
                MoveSteps = 1;
                return InnerExpression[CurIndex++];
            }

        }

        private void RevertIndex()
        {
            CurIndex -= MoveSteps;
        }

 
       
        public NFAState AddRule(string regex, string tokenName)
        {
            NFAState start = null;
            NFAState end = null;
            this.InnerExpression = regex;
            this.CurIndex = 0;
            this.MoveSteps = 1;
           
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

            if (CurIndex < InnerExpression.Length && InnerExpression[CurIndex] == '$')
            {
                end.Next = new NFAState();
                end.AddChar('\n'); //maybe have a bug
                end = end.Next;
                Contents.Add(end);
            }

            end.Accept = tokenName;
            NFAState ret = new NFAState();
            ret.Next = start;
            ret.Next2 = this.PreStart;
            this.PreStart = ret;
            return ret;
        }

        private void Expr(ref NFAState start, ref NFAState end)
        {
            NFAState nextStart = null;
            NFAState nextEnd = null;
            NFAState cur = null;
            CatExpr(ref start, ref end);

            while (CurIndex < InnerExpression.Length && GetCurChar() == '|')
            {
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
            else
            {
                Console.WriteLine("Something wrong");
            }

        }

        private void term(ref NFAState start, ref NFAState end)
        {
            char c = GetCurChar();

            if (c == '(')
            {
                Expr(ref start, ref end);
                if (this.CurIndex >= InnerExpression.Length)
                {
                    Console.Error.WriteLine("No ) for previous (");
                    throw new InvalidCharaterException("No ) for previous (");
                }
                c = GetCurChar();
                if (c == ')')
                {
                    return;
                }
                else
                {
                    Console.Error.WriteLine("{0} is invlaid ", c);
                    throw new InvalidCharaterException(c + " is invlaid");
                }
            }
            else
            {
                start = new NFAState();
                end = new NFAState();
                Contents.Add(start);
                Contents.Add(end);
                start.Next = end;

                if (!(c == '.' || c == '['))
                {
                    if (MoveSteps == 2)
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
                                start.AddChar(c);
                                break;
                        }
                    }
                    else
                    {
                        start.AddChar(c);
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
            if (c == '.')
            {

                for (char i = (char)0; i < 255; i++)
                {
                    start.AddChar(i);
                }
            }
            else
            {
                if (c == '[')
                {
                    if (CurIndex < InnerExpression.Length)
                    {
                        c = GetCurChar();
                        while (CurIndex < InnerExpression.Length + 1)
                        {
                            if (c != ']')
                            {
                                start.AddChar(c);
                            }
                            else
                            {
                                break;
                            }
                            c = GetCurChar();
                        } 

                        if (c != ']')
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
                if (s.Accept != null)
                {
                    acceptStr = s.Accept;
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
            Console.WriteLine("Expression:" + InnerExpression);
            foreach (NFAState state in Contents)
            {
                Console.WriteLine(state);
            }
        }
    }
}