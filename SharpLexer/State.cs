using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpLexer
{
    public abstract class State
    {
        static int IDCounter;
        public int ID { get; private set; }
        public string Accept { set; get; }
        public State()
        {
            ID = IDCounter++;
            Accept = null;
        }

        public static void RefreshID()
        {
            IDCounter = 0;
        }
    }


    public class NFAState:State
    {
        public enum EdgeLabel { CCL, EPSILON };
        
        public EdgeLabel Edge { set; get; }
        public HashSet<char> CharacterSet{set; get;}
        internal NFAState Next;
        internal NFAState Next2;
        
        
        public NFAState()
        {
            Edge = EdgeLabel.EPSILON;
            CharacterSet = null;
            Next = null;
            Next2 = null;
        }

        public bool copy(NFAState nfastate)
        {
            if (nfastate == null)
            {
                return false;
            }
            this.Accept = nfastate.Accept;
            this.Edge = nfastate.Edge;
            this.CharacterSet = nfastate.CharacterSet;
            this.Next = nfastate.Next;
            this.Next2 = nfastate.Next2;
            return true;
        }

        public void AddChar(char c)
        {
            if (CharacterSet == null)
            {
                CharacterSet = new HashSet<char>();
                this.Edge = EdgeLabel.CCL;
            }
            CharacterSet.Add(c);
        }

        public bool Contains(char c)
        {
            if (this.Edge == EdgeLabel.EPSILON)
            {
                return false;
            }
            else
            {
                return CharacterSet.Contains(c);
            }
        }

        public override string ToString()
        {
            string AcceptToken = this.Accept == null ? "NONE" : this.Accept;
            StringBuilder ret = new StringBuilder(string.Format("NFA State, ID:{0} Type:{1} Accept:{2}\n", this.ID, this.Edge, AcceptToken));
            if (this.Next != null)
            {
                if(this.CharacterSet !=null)
                {
                    ret.Append("keys:");
                    foreach(var val in CharacterSet)
                    {
                        ret.Append(String.Format(" {0}", val));
                    }
                }
                ret.Append("\n");
                ret.Append(String.Format("------->Next: {0}\n", this.Next.ID));
            }

            if (this.Next2 != null)
            {
                ret.Append(String.Format("------->Next2: {0}\n", this.Next2.ID));
            }
            return ret.ToString();
        }
    }

    public abstract class StateWithJumpTable:State
    {
        public Dictionary<char, State> Table{private set; get;}
        public StateWithJumpTable()
        {
            Table = new Dictionary<char, State>();
        }

        public ICollection<char> GetKeys
        {
            get { return Table.Keys; }
        }

        public State this[char c]
        {
            get
            {
                State ret = null;
                Table.TryGetValue(c, out ret);
                return ret;
            }
            set
            {
                Table[c] = value;
            }
        }
    }

    public class DFAState: StateWithJumpTable
    {
        public bool Mark;
        internal HashSet<NFAState> Contents{set; get;}
        public DFAState()
        {
            Mark = false;
            Contents = null;
        }

        public override string ToString()
        {
            string AcceptToken = this.Accept == null ? "NONE" : this.Accept;
            StringBuilder ret = new StringBuilder(string.Format("DFA State, ID:{0} Accept:{1}\n", this.ID, AcceptToken));
            foreach (var key in this.GetKeys)
            {
                ret.Append(String.Format("------>key: {0}, next state: {1}\n", key, this[key].ID));
            }
            return ret.ToString();
        }
    }

    public class Group: StateWithJumpTable
    {
 
        public HashSet<DFAState> Contents{private set; get;}
        public void Add(DFAState state)
        {
            Contents.Add(state);
        }

        private void ClearContent()
        {
            Contents = null;
        }

        public Group()
        {
            Contents = new HashSet<DFAState>();
        }

        public override string ToString()
        {
            string AcceptToken = this.Accept == null ? "NONE" : this.Accept;
            StringBuilder ret = new StringBuilder(string.Format("Group State, ID:{0} Accept:{1}\n", this.ID, AcceptToken));
            foreach (var key in this.GetKeys)
            {
                ret.Append(String.Format("------>key: {0}, next state: {1}\n", key, this[key].ID));
            }
            return ret.ToString(); 
        }
    }

}
