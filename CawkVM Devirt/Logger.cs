using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class Logger
{
    public static void Write(string message, TypeMessage type)
    {
        switch (type)
        {
            case TypeMessage.Debug:
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"{DateTime.Now} [Debug] {message}");
                Console.ForegroundColor = ConsoleColor.White;
                break;
            case TypeMessage.Done:
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{DateTime.Now} [+] : {message}");
                Console.ForegroundColor = ConsoleColor.White;
                break;
            case TypeMessage.Error:
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{DateTime.Now} [!] : {message}");
                Console.ForegroundColor = ConsoleColor.White;
                break;
            case TypeMessage.Info:
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{DateTime.Now} [-] : {message}");
                Console.ForegroundColor = ConsoleColor.White;
                break;
        }
    }
}
public enum TypeMessage
{
    Error,
    Done,
    Debug,
    Info,
}
