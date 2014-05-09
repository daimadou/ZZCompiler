using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpLexer
{
    public class Token
    {
        public string TokenName { private set; get; }
        public string Value { private set; get; }
        public Token(string TokenName, string Value)
        {
            this.TokenName = TokenName;
            this.Value = Value;
        }
        public override string ToString()
        {
            return String.Format("Token Name: {0}, String Value:{1}", TokenName, Value);
        }

        public override bool Equals(Object t)
        {
            return this.TokenName.Equals(((Token)t).TokenName) && this.Value.Equals(((Token)t).Value);
        }

        public static bool operator ==(Token x, Token y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(Token x, Token y)
        {
            return !x.Equals(y);
        }
    }

    public class Scanner
    {
        enum Status { START, PROCESS, END };
        State Start;
        NFAStateMachine nfaMachine;
        string Contents;
        int CurIndex;
        Status CurStatus;

        public Scanner()
        {
            nfaMachine = new NFAStateMachine();
            Start = null;
            Contents = null;
            CurIndex = 0;
            CurStatus = Status.START;
        }

        public void AddRule(string regex, string tokenName)
        {
            Start = nfaMachine.AddRule(regex, tokenName);
        }

        public void AddMacro(string macroName, string regex)
        {
            nfaMachine.AddMacro(macroName, regex);
        }

        public void Generate()
        {
            NFAState.RefreshID();
            DFAStateMachine dfaMachine = new DFAStateMachine();
            Start = dfaMachine.GenerateDFAMachine((NFAState)Start);
            State.RefreshID();
            MinDFAStateMachine minMachine = new MinDFAStateMachine();
            minMachine.MininzeDFAStates(dfaMachine.Contents, ref Start);
        }

        public void AddSource(string input)
        {
            this.Contents = input;
            this.CurIndex = 0;
        }

        
        public Token ReadOneToken()
        {
            StateWithJumpTable start = (StateWithJumpTable)this.Start;
            StateWithJumpTable cur = start;
            StateWithJumpTable next = null;
            char c = ' ';
            StringBuilder sb = new StringBuilder();
            while (ReadChar(out c))
            {
                next = cur[c];
                if (next == null)
                {
                    ReverseIndex();
                    break;
                }
                cur = next;
                sb.Append(c);
            }
            
            if (cur != null && cur.Accept != null)
            {
                return new Token(cur.Accept, sb.ToString());
            }
            return null;
        }

        private bool ReadChar(out char c)
        {
            bool ret = false;
            c = (char)0;
            if (CurIndex < Contents.Length)
            {
                c = Contents[CurIndex++];
                ret = true;
            }
            return ret;
        }

        private void ReverseIndex()
        {
            CurIndex--;
        }
    }
}
