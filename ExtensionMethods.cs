using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension
{
    using ZzCompiler;
    static class ExtensionMethods
    {
        public static DFAState IsNFASetExisted(this List<DFAState> DFASet, HashSet<NFAState> NFASet)
        {
            DFAState ret = null;
            foreach (var s in DFASet)
            {
                if (s.NfaStatesSet.SetEquals(NFASet))
                {
                    ret = s;
                    break;
                }
            }
            return ret;
        }
    }
}
