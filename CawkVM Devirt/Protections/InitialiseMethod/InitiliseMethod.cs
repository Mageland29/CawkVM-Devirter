using dnlib.DotNet;
using dnlib.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using static Context;
using static Logger;

namespace CawkVM_Devirt.Protections.InitialiseMethod
{
    public static class InitiliseMethod
	{
        private static string tempFolder;
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, EntryPoint = "GetProcAddress", ExactSpelling = true)]
        public static extern IntPtr e(IntPtr intptr, string str);
        public static a bc;
        public delegate void a(byte[] bytes, int len, byte[] key, int keylen);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LoadLibraryEx(string dllToLoad, IntPtr hFile, uint flags);

        public static void InitiliaseMethodage()
		{
            bool flag = IntPtr.Size == 4;
            IntPtr ptr = default(IntPtr);
            if (flag)
            {
                EmbeddedResource resource = (from x in module.Resources where x.Name.Equals("X86") && x.IsPrivate && x.ResourceType == ResourceType.Embedded select x).First() as EmbeddedResource;           
                ExtractEmbeddedDlls("NativePRo.dll", ReadFully(resource.Data.CreateStream()));
                IntPtr intptr = LoadDll("NativePRo.dll");
                ptr = e(intptr, "_a@16");
            }
            else
            {
                EmbeddedResource resource = (from x in module.Resources where x.Name.Equals("X64") && x.IsPrivate && x.ResourceType == ResourceType.Embedded select x).First() as EmbeddedResource;
                ExtractEmbeddedDlls("NativePRo.dll", ReadFully(resource.Data.CreateStream()));
                IntPtr intptr = LoadDll("NativePRo.dll");
                ptr = e(intptr, "a");
            }
            bc = (a)Marshal.GetDelegateForFunctionPointer(ptr, typeof(a));


            foreach (var _grabbedmethod in Stores.GrabbedMethod)
            {
                try
                {
                    MethodBase method = Stores.callingModule.ResolveMethod(_grabbedmethod.Value.MDToken.ToInt32());
                    byte[] array = byteArrayGrabber(Stores.Resources_Virt, _grabbedmethod.Key[0], _grabbedmethod.Key[1]);
                    byte[] key = MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(method.Name));
                    byte[] ilasByteArray = method.GetMethodBody().GetILAsByteArray();
                    bc(array, array.Length, ilasByteArray, ilasByteArray.Length);
                    byte[] bytes = Decrypt(key, array);
                    ConversionBack(bytes, _grabbedmethod.Key[2], method);
                    Write($"Translation Method {method.Name} : (GetILAsByte_Size, Key_Lenght, ID) [{ilasByteArray.Length}, {key.Length}, {_grabbedmethod.Key[2]}]", TypeMessage.Info);
                }
                catch
                {

                }
            }
        }
        private static byte[] DecryptBytes(SymmetricAlgorithm alg, byte[] message)
        {
            if (message == null || message.Length == 0) return message;
            if (alg == null) throw new ArgumentNullException("alg is null");
            using (var stream = new MemoryStream())
            using (var decryptor = alg.CreateDecryptor())
            using (var encrypt = new CryptoStream(stream, decryptor, CryptoStreamMode.Write))
            {
                encrypt.Write(message, 0, message.Length);
                encrypt.FlushFinalBlock();
                return stream.ToArray();
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
        public static void ExtractEmbeddedDlls(string dllName, byte[] resourceBytes)
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            string[] manifestResourceNames = executingAssembly.GetManifestResourceNames();
            AssemblyName name = executingAssembly.GetName();
            tempFolder = string.Format("{0}.{1}.{2}", name.Name, name.ProcessorArchitecture, name.Version);
            string text = Path.Combine(Path.GetTempPath(), tempFolder);
            bool flag = !Directory.Exists(text);
            if (flag)
            {
                Directory.CreateDirectory(text);
            }
            string environmentVariable = Environment.GetEnvironmentVariable("PATH");
            string[] array = environmentVariable.Split(new char[]
            {
                ';'
            });
            bool flag2 = false;
            foreach (string a in array)
            {
                bool flag3 = a == text;
                if (flag3)
                {
                    flag2 = true;
                    break;
                }
            }
            bool flag4 = !flag2;
            if (flag4)
            {
                Environment.SetEnvironmentVariable("PATH", text + ";" + environmentVariable);
            }
            string path = Path.Combine(text, dllName);
            bool flag5 = true;
            bool flag6 = File.Exists(path);
            if (flag6)
            {
                byte[] b = File.ReadAllBytes(path);
                bool flag7 = Equality(resourceBytes, b);
                if (flag7)
                {
                    flag5 = false;
                }
            }
            bool flag8 = flag5;
            if (flag8)
            {
                File.WriteAllBytes(path, resourceBytes);
            }
        }
        public static bool Equality(byte[] a1, byte[] b1)
        {
            bool flag = a1.Length == b1.Length;
            if (flag)
            {
                int num = 0;
                while (num < a1.Length && a1[num] == b1[num])
                {
                    num++;
                }
                bool flag2 = num == a1.Length;
                if (flag2)
                {
                    return true;
                }
            }
            return false;
        }
        public static byte[] Decrypt(byte[] key, byte[] message)
        {
            using (var rijndael = new RijndaelManaged())
            {
                rijndael.Key = key;
                rijndael.IV = key;
                return DecryptBytes(rijndael, message);
            }
        }
        public static byte[] byteArrayGrabber(byte[] bytes, int skip, int take)
        {
            byte[] array = new byte[take];
            int num = 0;
            int i = 0;
            while (i < take)
            {
                byte b = bytes[skip + i];
                array[num] = b;
                i++;
                num++;
            }
            return array;
        }
        public static void ConversionBack(byte[] bytes, int ID, MethodBase callingMethod)
        {
            MethodBody methodBody = callingMethod.GetMethodBody();
            BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes));
            var methodParameters = callingMethod.GetParameters();
            var allLocals = new List<LocalBuilder>();
            var _allExceptionHandlerses = new List<ExceptionHandlerClass>();
            Type[] parametersArray;
            int start = 0;
            if (callingMethod.IsStatic)
            {
                parametersArray = new Type[methodParameters.Length];
            }
            else
            {
                parametersArray = new Type[methodParameters.Length + 1];
                parametersArray[0] = callingMethod.DeclaringType;
                start = 1;
            }
            for (var i = 0; i < methodParameters.Length; i++)
            {
                var parameterInfo = methodParameters[i];
                parametersArray[start + i] = parameterInfo.ParameterType;
            }
            DynamicMethod dynamicMethod = new DynamicMethod("", callingMethod.MemberType == MemberTypes.Constructor ? null : ((MethodInfo)callingMethod).ReturnParameter.ParameterType, parametersArray, Stores.callingModule, true);
            ILGenerator ilGenerator = dynamicMethod.GetILGenerator();
            var locs = methodBody.LocalVariables;
            var locals = new Type[locs.Count];
            foreach (var localVariableInfo in locs)
            {
                allLocals.Add(ilGenerator.DeclareLocal(localVariableInfo.LocalType));
            }
            var exceptionHandlersCount = binaryReader.ReadInt32();
            processExceptionHandler(binaryReader, exceptionHandlersCount, callingMethod, _allExceptionHandlerses);
            var sortedExceptionHandlers = fixAndSortExceptionHandlers(_allExceptionHandlerses);
            var instructionCount = binaryReader.ReadInt32();
            var _allLabelsDictionary = new Dictionary<int, Label>();
            for (var u = 0; u < instructionCount; u++)
            {
                var label = ilGenerator.DefineLabel();
                _allLabelsDictionary.Add(u, label);
            }
            for (var i = 0; i < instructionCount; i++)
            {
                checkAndSetExceptionHandler(sortedExceptionHandlers, i, ilGenerator);
                var opcode = binaryReader.ReadInt16();
                OpCode opc;
                if (opcode >= 0 && opcode < Stores.oneByteOpCodes.Length)
                {
                    opc = Stores.oneByteOpCodes[opcode];
                }
                else
                {
                    var b2 = (byte)(opcode | 0xFE00);
                    opc = Stores.twoByteOpCodes[b2];
                }
                ilGenerator.MarkLabel(_allLabelsDictionary[i]);
                var operandType = binaryReader.ReadByte();
                HandleOpType(operandType, opc, ilGenerator, binaryReader, _allLabelsDictionary, allLocals);
            }
            lock (Stores.locker)
            {
                if (!Stores.GrabbedDynamicMethod.ContainsKey(ID))
                {
                    Stores.GrabbedDynamicMethod.Add(ID, dynamicMethod);
                }
            }
        }
        private static void HandleOpType(int opType, OpCode opcode, ILGenerator ilGenerator, BinaryReader binaryReader, Dictionary<int, Label> _allLabelsDictionary, List<LocalBuilder> allLocals)
        {
            switch (opType)
            {
                case 0:
                    InlineNoneEmitter(ilGenerator, opcode, binaryReader);
                    break;
                case 1:
                    InlineMethodEmitter(ilGenerator, opcode, binaryReader);
                    break;
                case 2:
                    InlineStringEmitter(ilGenerator, opcode, binaryReader);
                    break;
                case 3:
                    InlineIEmitter(ilGenerator, opcode, binaryReader);
                    break;
                case 5:
                    InlineFieldEmitter(ilGenerator, opcode, binaryReader);
                    break;
                case 6:
                    InlineTypeEmitter(ilGenerator, opcode, binaryReader);
                    break;
                case 7:
                    ShortInlineBrTargetEmitter(ilGenerator, opcode, binaryReader, _allLabelsDictionary);
                    break;
                case 8:
                    ShortInlineIEmitter(ilGenerator, opcode, binaryReader);
                    break;
                case 9:
                    InlineSwitchEmitter(ilGenerator, opcode, binaryReader, _allLabelsDictionary);
                    break;
                case 10:
                    InlineBrTargetEmitter(ilGenerator, opcode, binaryReader, _allLabelsDictionary);
                    break;
                case 11:
                    InlineTokEmitter(ilGenerator, opcode, binaryReader);
                    break;
                case 12:
                case 4:
                    InlineVarEmitter(ilGenerator, opcode, binaryReader, allLocals);
                    break;
                case 13:
                    ShortInlineREmitter(ilGenerator, opcode, binaryReader);
                    break;
                case 14:
                    InlineREmitter(ilGenerator, opcode, binaryReader);
                    break;
                case 15:
                    InlineI8Emitter(ilGenerator, opcode, binaryReader);
                    break;
                default:
                    throw new Exception("Operand Type Unknown " + opType);
            }
        }
        private static void InlineNoneEmitter(ILGenerator ilGenerator, OpCode opcode, BinaryReader binaryReader)
        {
            ilGenerator.Emit(opcode);
        }
        private static void InlineMethodEmitter(ILGenerator ilGenerator, OpCode opcode, BinaryReader binaryReader)
        {
            try
            {
                var mdtoken = binaryReader.ReadInt32();
                var resolvedMethodBase = Stores.callingModule.ResolveMethod(mdtoken);
                if (resolvedMethodBase is MethodInfo)
                    ilGenerator.Emit(opcode, (MethodInfo)resolvedMethodBase);
                else if (resolvedMethodBase is ConstructorInfo)
                    ilGenerator.Emit(opcode, (ConstructorInfo)resolvedMethodBase);
                else
                    throw new Exception("Check resolvedMethodBase Type");
            }
            catch
            {

            }
        }
        private static void InlineVarEmitter(ILGenerator ilGenerator, OpCode opcode, BinaryReader binaryReader, List<LocalBuilder> allLocals)
        {
            var index = binaryReader.ReadInt32();
            var parOrloc = binaryReader.ReadByte();
            if (parOrloc == 0)
            {
                var label = allLocals[index];
                ilGenerator.Emit(opcode, label);
            }
            else
            {
                ilGenerator.Emit(opcode, index);
            }

        }
        private static void InlineStringEmitter(ILGenerator ilGenerator, OpCode opcode, BinaryReader binaryReader)
        {
            var readString = binaryReader.ReadString();
            ilGenerator.Emit(opcode, readString);
        }
        private static void InlineIEmitter(ILGenerator ilGenerator, OpCode opcode, BinaryReader binaryReader)
        {
            var readInt32 = binaryReader.ReadInt32();
            ilGenerator.Emit(opcode, readInt32);
        }

        private static void InlineFieldEmitter(ILGenerator ilGenerator, OpCode opcode, BinaryReader binaryReader)
        {
            int mdtoken = binaryReader.ReadInt32();
            FieldInfo fieldInfo = Stores.callingModule.ResolveField(mdtoken);
            ilGenerator.Emit(opcode, fieldInfo);
        }

        private static void InlineTypeEmitter(ILGenerator ilGenerator, OpCode opcode, BinaryReader binaryReader)
        {
            int mdtoken = binaryReader.ReadInt32();
            Type type = Stores.callingModule.ResolveType(mdtoken);
            ilGenerator.Emit(opcode, type);
        }
        private static void ShortInlineBrTargetEmitter(ILGenerator ilGenerator, OpCode opcode, BinaryReader binaryReader, Dictionary<int, Label> _allLabelsDictionary)
        {
            int index = binaryReader.ReadInt32();
            var location = _allLabelsDictionary[index];
            ilGenerator.Emit(opcode, location);
        }
        private static void ShortInlineIEmitter(ILGenerator ilGenerator, OpCode opCode, BinaryReader binaryReader)
        {
            byte b = binaryReader.ReadByte();
            ilGenerator.Emit(opCode, b);
        }
        private static void ShortInlineREmitter(ILGenerator ilGenerator, OpCode opCode, BinaryReader binaryReader)
        {
            var value = binaryReader.ReadBytes(4);
            var myFloat = BitConverter.ToSingle(value, 0);
            ilGenerator.Emit(opCode, myFloat);
        }
        private static void InlineREmitter(ILGenerator ilGenerator, OpCode opCode, BinaryReader binaryReader)
        {
            var value = binaryReader.ReadDouble();
            ilGenerator.Emit(opCode, value);
        }
        private static void InlineI8Emitter(ILGenerator ilGenerator, OpCode opCode, BinaryReader binaryReader)
        {
            var value = binaryReader.ReadInt64();
            ilGenerator.Emit(opCode, value);
        }

        private static void InlineSwitchEmitter(ILGenerator ilGenerator, OpCode opCode, BinaryReader binaryReader, Dictionary<int, Label> _allLabelsDictionary)
        {
            int count = binaryReader.ReadInt32();
            Label[] allLabels = new Label[count];
            for (int i = 0; i < count; i++)
            {
                allLabels[i] = _allLabelsDictionary[binaryReader.ReadInt32()];
            }
            ilGenerator.Emit(opCode, allLabels);
        }
        private static void InlineBrTargetEmitter(ILGenerator ilGenerator, OpCode opcode, BinaryReader binaryReader, Dictionary<int, Label> _allLabelsDictionary)
        {
            int index = binaryReader.ReadInt32();
            var location = _allLabelsDictionary[index];
            ilGenerator.Emit(opcode, location);
        }

        private static void InlineTokEmitter(ILGenerator ilGenerator, OpCode opcode, BinaryReader binaryReader)
        {
            int mdtoken = binaryReader.ReadInt32();
            byte type = binaryReader.ReadByte();
            if (type == 0)
            {
                var fieldinfo = Stores.callingModule.ResolveField(mdtoken);
                ilGenerator.Emit(opcode, fieldinfo);
            }
            else if (type == 1)
            {
                var typeInfo = Stores.callingModule.ResolveType(mdtoken);
                ilGenerator.Emit(opcode, typeInfo);
            }
            else if (type == 2)
            {
                var methodinfo = Stores.callingModule.ResolveMethod(mdtoken);
                if (methodinfo is MethodInfo)
                    ilGenerator.Emit(opcode, (MethodInfo)methodinfo);
                else if (methodinfo is ConstructorInfo)
                    ilGenerator.Emit(opcode, (ConstructorInfo)methodinfo);
            }
        }

        public static void checkAndSetExceptionHandler(List<FixedExceptionHandlersClass> sorted, int i, ILGenerator ilGenerator)
        {
            foreach (var allExceptionHandlerse in sorted)
                if (allExceptionHandlerse.HandlerType == 1)
                {
                    if (allExceptionHandlerse.TryStart == i)
                    {
                        ilGenerator.BeginExceptionBlock();
                    }
                    if (allExceptionHandlerse.HandlerEnd == i)
                    {
                        ilGenerator.EndExceptionBlock();
                    }
                    if (allExceptionHandlerse.HandlerStart.Contains(i))
                    {
                        var indes = allExceptionHandlerse.HandlerStart.IndexOf(i);
                        ilGenerator.BeginCatchBlock(allExceptionHandlerse.CatchType[indes]);
                    }
                }
                else if (allExceptionHandlerse.HandlerType == 5)
                {
                    if (allExceptionHandlerse.TryStart == i)
                        ilGenerator.BeginExceptionBlock();
                    else if (allExceptionHandlerse.HandlerEnd == i)
                        ilGenerator.EndExceptionBlock();
                    else if (allExceptionHandlerse.TryEnd == i)
                        ilGenerator.BeginFinallyBlock();
                }
        }

        public static void processExceptionHandler(BinaryReader bin, int count, MethodBase method, List<ExceptionHandlerClass> _allExceptionHandlerses)
        {
            for (var i = 0; i < count; i++)
            {
                var expExceptionHandlers = new ExceptionHandlerClass();
                var catchTypeMdToken = bin.ReadInt32();
                if (catchTypeMdToken == -1)
                {
                    expExceptionHandlers.CatchType = null;
                }
                else
                {
                    var catchType = method.Module.ResolveType(catchTypeMdToken);
                    expExceptionHandlers.CatchType = catchType;
                }
                var filterStartIndex = bin.ReadInt32();
                expExceptionHandlers.FilterStart = filterStartIndex;
                var handlerEnd = bin.ReadInt32();
                expExceptionHandlers.HandlerEnd = handlerEnd;
                var handlerStart = bin.ReadInt32();
                expExceptionHandlers.HandlerStart = handlerStart;
                var handlerType = bin.ReadByte();
                switch (handlerType)
                {
                    case 1:
                        expExceptionHandlers.HandlerType = 1;
                        break;
                    case 2:
                        expExceptionHandlers.HandlerType = 2;
                        break;
                    case 3:
                        expExceptionHandlers.HandlerType = 3;
                        break;
                    case 4:
                        expExceptionHandlers.HandlerType = 4;
                        break;
                    case 5:
                        expExceptionHandlers.HandlerType = 5;
                        break;
                    default:
                        throw new Exception("Out of Range");
                }
                var tryEnd = bin.ReadInt32();
                expExceptionHandlers.TryEnd = tryEnd;
                var tryStart = bin.ReadInt32();
                expExceptionHandlers.TryStart = tryStart;
                _allExceptionHandlerses.Add(expExceptionHandlers);
            }
        }

        public static List<FixedExceptionHandlersClass> fixAndSortExceptionHandlers(List<ExceptionHandlerClass> expHandlers)
        {
            var multiExp = new List<ExceptionHandlerClass>();
            var exceptionDictionary = new Dictionary<ExceptionHandlerClass, int>();
            foreach (var handler in expHandlers)
                if (handler.HandlerType == 5)
                {
                    exceptionDictionary.Add(handler, handler.TryStart);
                }
                else
                {
                    if (exceptionDictionary.ContainsValue(handler.TryStart))
                        if (handler.CatchType != null)
                            exceptionDictionary.Add(handler, handler.TryStart);
                        else
                            multiExp.Add(handler);
                    else
                        exceptionDictionary.Add(handler, handler.TryStart);
                }
            var sorted = new List<FixedExceptionHandlersClass>();
            foreach (var keyValuePair in exceptionDictionary)
            {
                if (keyValuePair.Key.HandlerType == 5)
                {
                    var fixedExceptionHandlers = new FixedExceptionHandlersClass();
                    fixedExceptionHandlers.TryStart = keyValuePair.Key.TryStart;
                    fixedExceptionHandlers.TryEnd = keyValuePair.Key.TryEnd;
                    fixedExceptionHandlers.FilterStart = keyValuePair.Key.FilterStart;
                    fixedExceptionHandlers.HandlerEnd = keyValuePair.Key.HandlerEnd;
                    fixedExceptionHandlers.HandlerType = keyValuePair.Key.HandlerType;
                    fixedExceptionHandlers.HandlerStart.Add(keyValuePair.Key.HandlerStart);
                    fixedExceptionHandlers.CatchType.Add(keyValuePair.Key.CatchType);
                    sorted.Add(fixedExceptionHandlers);
                    continue;
                }
                var rrr = WhereAlternate(multiExp, keyValuePair.Value);
                if (rrr.Count == 0)
                {
                    var fixedExceptionHandlers = new FixedExceptionHandlersClass();
                    fixedExceptionHandlers.TryStart = keyValuePair.Key.TryStart;
                    fixedExceptionHandlers.TryEnd = keyValuePair.Key.TryEnd;
                    fixedExceptionHandlers.FilterStart = keyValuePair.Key.FilterStart;
                    fixedExceptionHandlers.HandlerEnd = keyValuePair.Key.HandlerEnd;
                    fixedExceptionHandlers.HandlerType = keyValuePair.Key.HandlerType;
                    fixedExceptionHandlers.HandlerStart.Add(keyValuePair.Key.HandlerStart);
                    fixedExceptionHandlers.CatchType.Add(keyValuePair.Key.CatchType);
                    sorted.Add(fixedExceptionHandlers);
                }
                else
                {
                    var fixedExceptionHandlers = new FixedExceptionHandlersClass();
                    fixedExceptionHandlers.TryStart = keyValuePair.Key.TryStart;
                    fixedExceptionHandlers.TryEnd = keyValuePair.Key.TryEnd;
                    fixedExceptionHandlers.FilterStart = keyValuePair.Key.FilterStart;
                    fixedExceptionHandlers.HandlerEnd = rrr[rrr.Count - 1].HandlerEnd;
                    fixedExceptionHandlers.HandlerType = keyValuePair.Key.HandlerType;
                    fixedExceptionHandlers.HandlerStart.Add(keyValuePair.Key.HandlerStart);
                    fixedExceptionHandlers.CatchType.Add(keyValuePair.Key.CatchType);
                    foreach (var exceptionHandlerse in rrr)
                    {
                        fixedExceptionHandlers.HandlerStart.Add(exceptionHandlerse.HandlerStart);
                        fixedExceptionHandlers.CatchType.Add(exceptionHandlerse.CatchType);
                    }
                    sorted.Add(fixedExceptionHandlers);
                }
            }
            return sorted;
        }
        public static IntPtr LoadDll(string dllName)
        {
            bool flag = tempFolder == "";
            if (flag)
            {
                throw new Exception("Please call ExtractEmbeddedDlls before LoadDll");
            }
            IntPtr intPtr = LoadLibraryEx(dllName, IntPtr.Zero, 0U);
            bool flag2 = intPtr == IntPtr.Zero;
            if (flag2)
            {
                throw new DllNotFoundException("Unable to load library: " + dllName + " from " + tempFolder);
            }
            return intPtr;
        }
        public static List<ExceptionHandlerClass> WhereAlternate(List<ExceptionHandlerClass> exp, int val)
        {
            var returnList = new List<ExceptionHandlerClass>();
            foreach (var handlers2 in exp)
                if (handlers2.TryStart == val && handlers2.HandlerType != 5)
                    returnList.Add(handlers2);
            return returnList;
        }
    }
}
