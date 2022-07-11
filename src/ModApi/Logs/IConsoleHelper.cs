using System;

namespace ModLoader.Logs
{
    public interface IConsoleHelper
    {
        void Log(string message);
        void Trace(string message);
        void Debug(string message);
        void Info(string message);
        void Error(string message);
        void Announcement(string message);
        void Warn(string message);
        void Success(string message, string info);
        void Failure(string message, string info);
        void Log(string message, string info);
        void AddConsoleCommand(string trigger, Action<string, string[]> handler);
    }
}