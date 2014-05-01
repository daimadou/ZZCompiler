﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpLexer
{
    using Extension;
    class DFAMachine
    {
         List<DFAState> StatesSet;
        public DFAMachine()
        {
            StatesSet = new List<DFAState>();
        }

        public DFAState GenerateDFAMachine(NFAState s)
        {
            DFAState start = new DFAState();
            DFAState cur = start;
            Queue<DFAState> UnmarkQueue = new Queue<DFAState>();
            UnmarkQueue.Enqueue(cur);
            String acceptStr = null;
            cur.Contents = new HashSet<NFAState>();
            cur.Contents.Add(s);
            cur.Contents = NFAMachine.GetEClousre(cur.Contents, ref acceptStr);
            StatesSet.Add(cur);

            while (UnmarkQueue.Count > 0)
            {
                cur = UnmarkQueue.Dequeue();
                for (char c = (char)0; c < 255; c++)
                {
                    String AcceptingStr = null;
                    HashSet<NFAState> baseSet = NFAMachine.Move(cur.Contents, c);
                    if(baseSet != null)
                    {
                        HashSet<NFAState> set = NFAMachine.GetEClousre(baseSet, ref AcceptingStr);
                        DFAState ds = StatesSet.IsNFASetExisted(set);
                        if (ds == null)
                        {
                            ds = new DFAState();
                            ds.Contents = set;
                            ds.Accept = AcceptingStr;
                            StatesSet.Add(ds);
                            UnmarkQueue.Enqueue(ds);
                        }
                        cur[c] = ds;
                    }
                }
            }
            
            return start;
        }

        public void DumpAllStates()
        {
            foreach(var state in StatesSet)
            {
                Console.WriteLine(state);
            }
        }
    }

    class DFAMachineMin
    {
        private List<Group> Contents;
        private Dictionary<DFAState, Group> FindGroup;

        public DFAMachineMin()
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

        public void MininzeDFAStates(List<DFAState> DFAStates)
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
                    for (char c = (char)0; c < 255; c++)
                    {
                        DFAState gotoFirst = (DFAState)first[c];
                        DFAState gotoNext = (DFAState)next[c];
                     
                        if (!((gotoFirst != null && gotoNext != null && FindGroup[gotoFirst] == FindGroup[gotoNext]) || (gotoFirst == null && gotoNext == null)))
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
            Group.RefreshID();
            NFAState.RefreshID();
            DFAState.RefreshID();
        }

        private void MapGroups()
        {
            foreach (var g in Contents)
            {
                foreach (var s in g.Contents)
                {
                    for (char c = (char)0; c < 255; c++)
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
    }
}