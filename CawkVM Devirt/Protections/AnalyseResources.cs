using dnlib.DotNet;
using dnlib.IO;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using static Context;
using static Logger;


namespace CawkVM_Devirt.Protections
{
    public static class AnalyseResources
    {
        public static void InitialiseResources()
        {
            EmbeddedResource resource = (from x in module.Resources where x.Name.Equals(Stores.resource) && x.IsPrivate && x.ResourceType == ResourceType.Embedded select x).First() as EmbeddedResource;
            Stores.Resources_Virt = exclusiveOR(ReadFully(resource.Data.CreateStream()));
            Write($"Grabbing Resource {resource.Name} : Lenght {Stores.Resources_Virt.Length}", TypeMessage.Info);
            OpCode[] array = new OpCode[256];
            OpCode[] array2 = new OpCode[256];
            Stores.oneByteOpCodes = array;
            Stores.twoByteOpCodes = array2;
            Type typeFromHandle = typeof(OpCode);
            Type typeFromHandle2 = typeof(OpCodes);
            foreach (FieldInfo fieldInfo in typeFromHandle2.GetFields())
            {
                if (fieldInfo.FieldType == typeFromHandle)
                {
                    OpCode opCode = (OpCode)fieldInfo.GetValue(null);
                    ushort num = (ushort)opCode.Value;
                    if (opCode.Size == 1)
                    {
                        byte b = (byte)num;
                        Stores.oneByteOpCodes[(int)b] = opCode;
                    }
                    else
                    {
                        byte b2 = (byte)(num | 65024);
                        Stores.twoByteOpCodes[(int)b2] = opCode;
                    }
                }
            }
        }
        public static byte[] ReadFully(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }
        public static byte[] exclusiveOR(byte[] arr1)
        {
            Random rand = new Random(23546654);
            byte[] result = new byte[arr1.Length];
            for (int i = 0; i < arr1.Length; i++)
            {
                result[i] = (byte)(arr1[i] ^ rand.Next(0, 250));
            }
            return result;
        }
    }
}
