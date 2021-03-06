﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpLexer
{
    using Extension;
    public class DFAStateMachine
    {
        public List<DFAState> Contents{private set; get;}
        public DFAStateMachine()
        {
            Contents = new List<DFAState>();
        }

        public DFAState GenerateDFAMachine(NFAState s, Dictionary<string, int> PriorityTable)
        {
            DFAState start = new DFAState();
            DFAState cur = start;
            Queue<DFAState> UnmarkQueue = new Queue<DFAState>();
            UnmarkQueue.Enqueue(cur);
            String acceptStr = null;
            cur.Contents = new HashSet<NFAState>();
            cur.Contents.Add(s);
            cur.Contents = NFAStateMachine.GetEClousre(cur.Contents, ref acceptStr, PriorityTable);
            Contents.Add(cur);

            while (UnmarkQueue.Count > 0)
            {
                cur = UnmarkQueue.Dequeue();
                for (char c = (char)0; c < 256; c++)
                {
                    String AcceptingStr = null;
                    HashSet<NFAState> baseSet = NFAStateMachine.Move(cur.Contents, c);
                    if(baseSet != null)
                    {
                        HashSet<NFAState> set = NFAStateMachine.GetEClousre(baseSet, ref AcceptingStr, PriorityTable);
                        DFAState ds = Contents.IsNFASetExisted(set);
                        if (ds == null)
                        {
                            ds = new DFAState();
                            ds.Contents = set;
                            ds.Accept = AcceptingStr;
                            Contents.Add(ds);
                            UnmarkQueue.Enqueue(ds);
                        }
                        cur[c] = ds;
                    }
                }
            }
            //DumpAllStates();
            TableMininize();
            return start;
        }

        public void TableMininize()
        {
            HashSet<char> initialGroup  = new HashSet<char>();
            for(char i = (char)0; i < 256; i++)
            {
                initialGroup.Add(i);
            }

            List<HashSet<char>> groups = new List<HashSet<char>>(); 
            groups.Add(initialGroup);
            for (int i = 0; i < groups.Count; i++)
            {
                HashSet<char> g = groups[i];
                List<char> chars = g.ToList();
                HashSet<char> newGroup = new HashSet<char>();
                for (int j = 1; j < chars.Count; j++)
                {
                    for(int k = 0; k < Contents.Count; k++)
                    {
                        StateWithJumpTable s1 = Contents[k][chars[0]];
                        StateWithJumpTable s2 = Contents[k][chars[j]];
                        if(s1!=s2)
                        {
                            newGroup.Add(chars[j]);
                            g.Remove(chars[j]);
                            break;
                        }
                    }
                }
                if(newGroup.Count > 0)
                {
                    groups.Add(newGroup);
                }
            }

            Dictionary<char, int> transtable = new Dictionary<char, int>();
            List<char> charlist = new List<char>();
            for (int i = 0; i < groups.Count; i++)
            {
                List<char> charset = groups[i].ToList();
                charlist.Add(charset[0]);
                for (int j = 0; j < groups[i].Count; j++)
                {
                    transtable.Add(charset[j], i);
                }
            }

            foreach (var s in Contents)
            {
                s.CompressingTable(transtable, charlist);
            }
        }

        public void DumpAllStates()
        {
            foreach(var state in Contents)
            {
                Console.WriteLine(state);
            }
        }
    }




    public class MinDFAStateMachine
    {
        private List<Group> Contents;
        private Dictionary<DFAState, Group> FindGroup;

        public MinDFAStateMachine()
        {
            Contents = new List<Group>();
            FindGroup = new Dictionary<DFAState, Group>();
        }

        private void InitialSplit(List<DFAState> DFAStates)
        {
            Group group1 = new Group();
            Group group2 = new Group();
            foreach (var state in DFAStates)
            {
                if (state.Accept != null)
                {
                    group2.Add(state);
                    FindGroup[state] = group2;
                }
                else
                {
                    group1.Add(state);
                    FindGroup[state] = group1;
                }
            }
            Contents.Add(group1);
            Contents.Add(group2);
        }

        public void MininzeDFAStates(List<DFAState> DFAStates, ref State start)
        {
            InitialSplit(DFAStates);
            for (int i = 0; i < Contents.Count; i++)
            {
                var CurGroup = Contents[i];
                Group NewGroup = new Group();
                List<DFAState> contents = CurGroup.Contents.ToList();
                DFAState first = contents.Count > 0 ? contents[0] : null;
                DFAState next = contents.Count > 1 ? contents[1] : null;
                while (next != null && first != null)
                {
                    for (char c = (char)0; c < 256; c++)
                    {
                        DFAState gotoFirst = (DFAState)first[c];
                        DFAState gotoNext = (DFAState)next[c];
                     
                        if (!((gotoFirst != null && gotoNext != null && FindGroup[gotoFirst] == FindGroup[gotoNext]) || (gotoFirst == null && gotoNext == null))||first.Accept != next.Accept)
                        {
                            NewGroup.Add(next);
                            FindGroup[next] = NewGroup;
                            CurGroup.Contents.Remove(next);
                            break;
                        }
                    }
                    next = contents.IndexOf(next) + 1 < contents.Count ? contents[contents.IndexOf(next) + 1] : null;
                }

                if (NewGroup.Contents.Count > 0)
                {
                    Contents.Add(NewGroup);
                }
            }

            MapGroups();
            //DumpDFAFromGourps();
           // this.DumpAllStates();
            Group.RefreshID();
            NFAState.RefreshID();
            DFAState.RefreshID();
            start = FindGroup[(DFAState)start];
        }

        private void MapGroups()
        {
            foreach (var g in Contents)
            {
                foreach (var s in g.Contents)
                {
                    for (char c = (char)0; c < 256; c++)
                    {
                        DFAState nexestate = (DFAState)s[c];
                        if (nexestate != null)
                        {
                            g[c] = FindGroup[nexestate];
                        }
                    }
                }


                foreach (var state in g.Contents)
                {
                    if (state.Accept != null)
                    {
                        g.Accept = state.Accept;
                    }
                }
            }
        }

        private void DumpDFAFromGourps()
        {
            foreach (var g in Contents)
            {
                Console.WriteLine("Group ID:{0}", g.ID);
                foreach (var s in g.Contents.ToList())
                {
                    Console.WriteLine("----->DFA State:{0}", s.ID);
                }
            }
        }

        public void DumpAllStates()
        {
            foreach (var s in this.Contents)
            {
                Console.WriteLine(s);
            }
        }
    }
}
