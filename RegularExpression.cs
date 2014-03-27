using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZzCompiler
{
    abstract interface RegularExpression
    {
        public StateMachine stateMachine;
        public void Accept();
        void NFAStateMachine();
    }
}
