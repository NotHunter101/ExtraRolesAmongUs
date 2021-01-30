using System;
using System.Collections.Generic;
using System.Text;

namespace ExtraRolesMod
{
    class ConsoleTools
    {
        public static void Info(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[ExtraR INFO] " + message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void Error(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[ExtraR ERROR] " + message);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}