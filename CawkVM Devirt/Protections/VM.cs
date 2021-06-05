using CawkVM_Devirt.Protections.InitialiseMethod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CawkVM_Devirt.Protections
{
    public static class VM
    {
        public static void Execute()
        {
            AnalyseResources.InitialiseResources();
            AnalyseMethod.AnalysePhase();
            InitiliseMethod.InitiliaseMethodage();
            InitialiseReplace.ReplacePhase();
        }
    }
}
