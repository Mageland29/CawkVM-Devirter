using System;
using System.Collections.Generic;
using System.Text;

namespace CawkVM_Devirt.Protections.InitialiseMethod
{
    public class ExceptionHandlerClass
    {
        public Type CatchType;
        public int FilterStart;
        public int HandlerEnd;
        public int HandlerStart;
        public int HandlerType;
        public int TryEnd;
        public int TryStart;
    }
}
