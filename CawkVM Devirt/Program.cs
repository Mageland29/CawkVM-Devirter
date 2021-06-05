using CawkVM_Devirt.Protections;
using static Context;
using static Logger;

namespace CawkVM_Devirt
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                Welcome();
                LoadModule(args[0]);
                VM.Execute();
                SaveModule();
            }
            else
            {  
                Write($"Please drag and drop your file\n\n", TypeMessage.Error);
            }
        }
    }
}
