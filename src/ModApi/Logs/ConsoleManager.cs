using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModLoader.Events;

namespace ModLoader.Logs
{
    internal class ConsoleManager : IConsoleHelper
    {
        private ModHelper helper;
        private Dictionary<string, Action<string, string[]>> consoleCommands;
        private Action<object, ConsoleInputReceivedEventArgs> commandHandler;

        public ConsoleManager(ModHelper modhelper)
        {
            helper = modhelper;
            consoleCommands = new Dictionary<string, Action<string, string[]>>();
        }


        public void Log(string message) => WriteMessage(message, "LOG", ConsoleColor.DarkGray);
        public void Trace(string message) => WriteMessage(message, "TRACE", ConsoleColor.DarkGray);
        public void Debug(string message) => WriteMessage(message, "DEBUG", ConsoleColor.DarkMagenta);
        public void Info(string message) => WriteMessage(message, "INFO", ConsoleColor.White);
        public void Error(string message) => WriteMessage(message, "ERROR", ConsoleColor.Red);
        public void Announcement(string message) => WriteMessage(message, "INFO", ConsoleColor.DarkGreen);
        public void Warn(string message) => WriteMessage(message, "WARN", ConsoleColor.DarkYellow);
        public void Success(string message, string info) => WriteTwoPartMessage(message, info, "INFO", ConsoleColor.White, ConsoleColor.Green);
        public void Failure(string message, string info) => WriteTwoPartMessage(message, info, "WARN", ConsoleColor.White, ConsoleColor.Red);
        public void Log(string message, string info) => WriteTwoPartMessage(message, info, "INFO", ConsoleColor.White, ConsoleColor.White);

        internal void WriteMessage(string message, string type, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"[{helper.Manifest.Name}][{type}] {message}");
            Console.ForegroundColor = ConsoleColor.White;
        }

        internal void WriteTwoPartMessage(string message1, string message2, string type, ConsoleColor color1, ConsoleColor color2)
        {
            Console.ForegroundColor = color1;
            Console.Write($"[{helper.Manifest.Name}][{type}] {message1}");
            Console.ForegroundColor = color2;
            Console.Write($"\t{message2}");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
        }

        public void AddConsoleCommand(string trigger, Action<string, string[]> handler)
        {
            if (consoleCommands.ContainsKey(trigger.ToLower()))
                return;

            consoleCommands.Add(trigger.ToLower(), handler);
            if(commandHandler == null)
            {
                commandHandler = (sender, args) =>
                {
                    string[] inputs = args.Input.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (inputs.Length > 0 && inputs[0].ToLower() is string key && consoleCommands.ContainsKey(key) && consoleCommands[key] is Action<string, string[]> action)
                        action.Invoke(inputs[0], inputs.Length > 1 ? inputs.Skip(1).ToArray() : new string[0]);
                };

                EventManager.Singleton.ConsoleInputReceived += (sender, args) => commandHandler(sender, args);
            }
        }

    }
}