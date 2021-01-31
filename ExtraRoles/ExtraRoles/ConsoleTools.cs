using System;
using System.Collections.Generic;
using System.Text;

namespace ExtraRolesMod
{
    class ConsoleTools
    {
        public static void Info(string message)
        {
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.WriteLine("[ExtraR INFO] " + message);
            System.Console.ForegroundColor = ConsoleColor.White;
        }

        public static void Error(string message)
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine("[ExtraR ERROR] " + message);
            System.Console.ForegroundColor = ConsoleColor.White;
        }
    }
}