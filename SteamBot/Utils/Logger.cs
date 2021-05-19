using System;
using System.IO;

namespace SellCards {

    class Logger
    {
        public static void error(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            msg = DateTime.Now + " - " + msg;
            Console.WriteLine(msg);
            Console.ResetColor();

            File.AppendAllText("log.txt", msg + "\n");
        }

        public static void error(string format, params object[] args)
        {
            var msg = string.Format(format, args);
            error(msg);
        }

        public static void error(string msg, Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            msg = DateTime.Now + " - " + msg + ". " + e.Message;
            Console.WriteLine(msg);
            Console.ResetColor();

            File.AppendAllText("log.txt", msg + "\n");
            File.AppendAllText("error.txt", msg + "\n" + e.StackTrace + "\n");
        }

        public static void info(string msg)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            msg = DateTime.Now + " - " + msg;
            Console.WriteLine(msg);
            Console.ResetColor();

            File.AppendAllText("log.txt", msg + "\n");
        }

        public static void info(string format, params object[] args)
        {
            var msg = string.Format(format, args);
            info(msg);
        }
    }
}