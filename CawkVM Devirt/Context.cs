using CawkVM_Devirt.Protections;
using dnlib.DotNet;
using dnlib.DotNet.Writer;
using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using static Logger;

public static class Context
{
    public static ModuleDefMD module = null;
    public static string FileName = null;
    public static void LoadModule(string filename)
    {
        try
        {
            FileName = filename;
            byte[] data = File.ReadAllBytes(filename);
            ModuleContext modCtx = ModuleDef.CreateModuleContext();
            module = ModuleDefMD.Load(data, modCtx);
            try
            {
                Stores.callingModule = Assembly.LoadFrom(filename).ManifestModule; 
            }
            catch
            {
                Stores.callingModule = Assembly.Load(data).ManifestModule;
            }
            Write("Module Loaded : " + module.Name, TypeMessage.Info);
            foreach (AssemblyRef dependance in module.GetAssemblyRefs())
            {
                Write($"Dependance : {dependance.Name}", TypeMessage.Info);
            }
            Console.WriteLine(Environment.NewLine);
        }
        catch
        {
            Write("Error for Loade Module", TypeMessage.Error);
        }
    }
    public static void SaveModule()
    {
        try
        {
            string filename = string.Concat(new string[] { Path.GetDirectoryName(FileName), "\\", Path.GetFileNameWithoutExtension(FileName), "_Devirt", Path.GetExtension(FileName) });
            if (module.IsILOnly)
            {
                ModuleWriterOptions writer = new ModuleWriterOptions(module);
                writer.MetaDataOptions.Flags = MetaDataFlags.PreserveAll;
                writer.MetaDataLogger = DummyLogger.NoThrowInstance;
                module.Write(filename, writer);
            }
            else
            {
                NativeModuleWriterOptions writer = new NativeModuleWriterOptions(module);
                writer.MetaDataOptions.Flags = MetaDataFlags.PreserveAll;
                writer.MetaDataLogger = DummyLogger.NoThrowInstance;
                module.NativeWrite(filename, writer);
            }
            Write("File Devirt and Saved : " + filename, TypeMessage.Done);
        }
        catch (ModuleWriterException ex)
        {
            Write("Fail to save current module\n" + ex.ToString(), TypeMessage.Error);
        }
        Console.ReadLine();
    }
    public static byte[] Compress(byte[] data)
    {
        MemoryStream output = new MemoryStream();
        using (DeflateStream dstream = new DeflateStream(output, CompressionLevel.Optimal))
        {
            dstream.Write(data, 0, data.Length);
        }
        return output.ToArray();
    }
    public static byte[] GetCurrentModule(ModuleDefMD module)
    {
        MemoryStream memorystream = new MemoryStream();
        if (module.IsILOnly)
        {
            ModuleWriterOptions writer = new ModuleWriterOptions(module);
            writer.MetaDataOptions.Flags = MetaDataFlags.PreserveAll;
            writer.MetaDataLogger = DummyLogger.NoThrowInstance;
            module.Write(memorystream, writer);
        }
        else
        {
            NativeModuleWriterOptions writer = new NativeModuleWriterOptions(module);
            writer.MetaDataOptions.Flags = MetaDataFlags.PreserveAll;
            writer.MetaDataLogger = DummyLogger.NoThrowInstance;
            module.NativeWrite(memorystream, writer);
        }
        byte[] ByteArray = new byte[memorystream.Length];
        memorystream.Position = 0;
        memorystream.Read(ByteArray, 0, (int)memorystream.Length);
        return ByteArray;
    }
    public static void Command()
    {
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine(@"Please enter the name of the resource that contains the bodies : ");
        Stores.resource = Console.ReadLine();

        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine(@"Please enter the name of the Run Method : ");
        Stores.method_name = Console.ReadLine();
    }
    public static void Welcome()
    {
        Console.Title = "CawkVM Devirt Console 1.0";
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(@"Made By Sir-_-MaGeLanD#7358");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(@"                         __          __  _                                  _______    ");
        Console.WriteLine(@"                         \ \        / / | |                                |__   __|   ");
        Console.WriteLine(@"                          \ \  /\  / /__| | ___ ___  _ __ ___   ___           | | ___  ");
        Console.WriteLine(@"                           \ \/  \/ / _ \ |/ __/ _ \| '_ ` _ \ / _ \          | |/ _ \ ");
        Console.WriteLine(@"                            \  /\  /  __/ | (_| (_) | | | | | |  __/          | | (_) |");
        Console.WriteLine(@"                             \/  \/ \___|_|\___\___/|_| |_| |_|\___|          |_|\___/ ");
        Console.WriteLine(@"   ____               _   __     ____  __            _____           _      _   ");
        Console.WriteLine(@"  / ___|__ ___      _| | _\ \   / /  \/  |          |  _  \         (_)    | |  ");
        Console.WriteLine(@" | |   / _` \ \ /\ / / |/ /\ \ / /| |\/| |          | | | |_____   ___ _ __| |_ ");
        Console.WriteLine(@" | |__| (_| |\ V  V /|   <  \ V / | |  | |          | | | / _ \ \ / / | '__| __|");
        Console.WriteLine(@"  \____\__,_| \_/\_/ |_|\_\  \_/  |_|  |_|          | |/ /  __/\ V /| | |  | |_ ");
        Console.WriteLine(@"                                                    |___/ \___| \_/ |_|_|   \__|");
        Console.WriteLine(Environment.NewLine);
        Console.WriteLine(Environment.NewLine);
        Console.WriteLine(Environment.NewLine);
        Command();
    }
}
