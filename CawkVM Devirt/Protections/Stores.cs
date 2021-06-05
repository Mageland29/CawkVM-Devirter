using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace CawkVM_Devirt.Protections
{
    public static class Stores
    {
        public static string resource;
        public static string method_name;
        public static byte[] Resources_Virt; 
        public static object locker = new object();
        public static System.Reflection.Module callingModule;
        public static Dictionary<int[], MethodDef> GrabbedMethod = new Dictionary<int[], MethodDef>();
        public static Dictionary<int, DynamicMethod> GrabbedDynamicMethod = new Dictionary<int, DynamicMethod>();
        public static OpCode[] oneByteOpCodes;
        public static OpCode[] twoByteOpCodes;
    }
}
