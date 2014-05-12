using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            return String.Format("Token:\n Name: {0}, Value: {1} ", TokenName, Value);
        }

        public override bool Equals(Object t)
        {
            return this.TokenName.Equals(((Token)t).TokenName) && this.Value.Equals(((Token)t).Value);
        }
    }

    public class Scanner
    {
        enum Status { START, PROCESS, END };
        State Start;
        NFAStateMachine nfaMachine;
        string Contents;
        int CurIndex;
        Dictionary<string, int> PriorityTable;
        static int Priority = 0;
        public Scanner()
        {
            nfaMachine = new NFAStateMachine();
            Start = null;
            Contents = null;
            CurIndex = 0;
            PriorityTable = new Dictionary<string,int>();
        }

        public void ReadDefFile(string FileName)
        {
            //Regex TokenPattern = new Regex(@"(^(TOKENNAME:\s*(?<TokenName>\w+)\s+EXPRESSION:\s*(?<Expr>.*)\s*)$)|(^\s*(\/\/.*)?$)");
            Regex TokenPattern = new Regex(@"^(\$(?<MacroName>\w+)|(?<TokenName>\w+))\s*=\s*'(?<Regex>.+)'");
            foreach (var line in System.IO.File.ReadLines(FileName))
            {
                Match token = TokenPattern.Match(line);
                if (token.Success)
                {
                    string exp = token.Groups["Regex"].Value; 
                    if (token.Groups["MacroName"].Success)
                    {
                        string macroName = token.Groups["MacroName"].Value;
                        Console.WriteLine("MacroName:{0} Regex:{1}", macroName, exp);
                        AddMacro(macroName, exp);
                    }

                    if (token.Groups["TokenName"].Success)
                    {
                        string tokenName = token.Groups["TokenName"].Value;
                        Console.WriteLine("TokenName:{0} Regex:{1}", tokenName, exp);
                        AddRule(tokenName, exp);
                    }
                    
                }
            }
        }

        public void AddRule(string tokenName, string regex)
        {
            PriorityTable.Add(tokenName, Priority++);
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
            Start = dfaMachine.GenerateDFAMachine((NFAState)Start, this.PriorityTable);
            State.RefreshID();
            //MinDFAStateMachine minMachine = new MinDFAStateMachine();
            //minMachine.MininzeDFAStates(dfaMachine.Contents, ref Start);
        }

        public void AddSource(string input)
        {
            this.Contents = input;
            this.CurIndex = 0;
        }

        public Token ReadOneTokenWithIgnore(String TokenName)
        {
            Token ret = null;
            string tname = null;
            bool isToken = false;
            do
            {
                ret = ReadOneToken();
                if (ret != null)
                {
                    tname = ret.TokenName;
                    isToken = tname.Equals(TokenName);
                }
                else
                {
                    isToken = false;
                }
                
            } while (ret != null && isToken);
            return ret;
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
