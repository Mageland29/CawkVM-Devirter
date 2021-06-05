using dnlib.DotNet.Emit;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using static Context;
using static Logger;

namespace CawkVM_Devirt.Protections
{
    public static class InitialiseReplace
    {
        public static void ReplacePhase()
        {
            foreach (var store_method in Stores.GrabbedMethod)
            {
                try
                {
				    int ID = store_method.Key[2];
				    DynamicMethod dynamic_method = Stores.GrabbedDynamicMethod[ID];
                    var dynamicReader = Activator.CreateInstance(typeof(DynamicMethod).Module.GetTypes().FirstOrDefault(t => t.Name == "DynamicResolver"), (BindingFlags)(-1), null, new object[] { dynamic_method.GetILGenerator() }, null);
                    var reader = new DynamicMethodBodyReader(module, dynamicReader); reader.Read();
                    store_method.Value.Body = reader.GetMethod().Body;
                    Write($"Replace Method {store_method.Value.Name} : ", TypeMessage.Done);
                    foreach(Instruction instruction in store_method.Value.Body.Instructions.ToArray())
                    {
                        Write($"Instructions : {instruction}", TypeMessage.Debug);
                    }
                }
                catch
                {

                }

            }
        }
    }
}
