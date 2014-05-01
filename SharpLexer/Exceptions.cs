using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpLexer
{
    class InvalidCharaterException : System.Exception
    {
        public InvalidCharaterException() : base() { }
        public InvalidCharaterException(string message) : base(message) { }
        public InvalidCharaterException(string message, System.Exception inner) : base(message, inner) { }

        // A constructor is needed for serialization when an 
        // exception propagates from a remoting server to the client.  
        protected InvalidCharaterException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { 
        }
    }
}
