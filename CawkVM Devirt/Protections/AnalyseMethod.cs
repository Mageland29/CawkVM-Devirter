using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Collections.Generic;
using System.Linq;
using static Context;
using static Logger;

namespace CawkVM_Devirt.Protections
{
    public static class AnalyseMethod
    {
        public static void AnalysePhase()
        {
			foreach (TypeDef type in (from x in module.Types where x.HasMethods select x).ToArray())
			{
				foreach (MethodDef method in (from x in type.Methods where x.HasBody && x.Body.HasInstructions select x).ToArray())
				{
					IList<Instruction> Instr = method.Body.Instructions;
					for (int i = 0; i < Instr.Count; i++)
					{
						if (Instr[i].IsLdcI4() && Instr[i + 1].IsLdcI4() && Instr[i + 2].IsLdcI4() && Instr[i + 3].IsLdloc() &&
							Instr[i + 4].OpCode == OpCodes.Call &&
							Instr[i + 4].Operand.ToString().Contains(Stores.method_name))
						{
							int Position = Instr[i].GetLdcI4Value();
							int Size = Instr[i + 1].GetLdcI4Value(); 
							int ID = Instr[i + 2].GetLdcI4Value();
							int[] args = new int[] { Position, Size, ID };
							Stores.GrabbedMethod.Add(args, method);
							Write($"Grabbing Method {method.Name} : (Position, Size, ID) [{Position}, {Size}, {ID}]", TypeMessage.Info);
						}
					}
				}
			}
		}
    }
}
