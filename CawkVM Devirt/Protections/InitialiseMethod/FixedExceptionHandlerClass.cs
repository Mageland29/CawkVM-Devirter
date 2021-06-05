using System;
using System.Collections.Generic;
using System.Text;

namespace CawkVM_Devirt.Protections.InitialiseMethod
{
    public class FixedExceptionHandlersClass
    {
        public List<Type> CatchType = new List<Type>();
        public int FilterStart;
        public int HandlerEnd;
        public List<int> HandlerStart = new List<int>();
        public int HandlerType;
        public int TryEnd;
        public int TryStart;
    }
}
