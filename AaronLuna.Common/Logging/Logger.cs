using AaronLuna.Common.Extensions;

namespace AaronLuna.Common.Logging
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading;

    public class Logger
    {
        bool _errorShown;
        static readonly Timer Timer = new Timer(Tick);
        static readonly StringBuilder LogQueue = new StringBuilder();

        public static FileInfo LogFile { get; private set; }
        public static DirectoryInfo LogFolder => LogFile?.Directory;
        public static bool Listening { get; private set; }
        public static bool LogToConsole;
        public static bool IgnoreDebug;
        public static int BatchInterval = 1000;

        public string Name { get; }

        public Logger(Type t) : this(t.Name)
        {
        }

        public Logger(string name)
        {
            Name = name;
        }

        public EventHandler<LogMessageInfo> LogMessageAdded;

        public static void Start(string logFilePath)
        {
            if (Listening)
            {
                return;
            }

            Listening = true;
            LogFile = new FileInfo(logFilePath);
            VerifyLogFolder();

            Timer.Change(BatchInterval, Timeout.Infinite); // A one-off tick event that is reset every time.
        }

        public static void ShutDown()
        {
            if (!Listening)
                return;

            Listening = false;
            Timer.Dispose();
            Tick(null); // Flush.
        }

        public void Debug(string message)
        {
            if (IgnoreDebug)
                return;

            Log(Logging.DEBUG, message);
        }

        public void Info(string message)
        {
            Log(Logging.INFO, message);
        }

        public void Warn(string message, Exception ex = null)
        {
            Log(Logging.WARN, message, ex);
        }

        public void Error(string message, Exception ex = null)
        {
            Log(Logging.ERROR, message, ex);
        }

        public void Log(string level, string message, Exception ex = null)
        {
            if (!CheckListening()) return;
            if (ex != null)
            {
                message += ex.GetReport();
            }

            var info = new LogMessageInfo(level, Name, message);
            var msg = info.ToString();

            lock (LogQueue)
            {
                LogQueue.AppendLine(msg);
            }

            LogMessageAdded?.Invoke(this, info); // Block caller.
        }

        static void VerifyLogFolder()
        {
            if (LogFolder == null)
            {
                throw new DirectoryNotFoundException("Target logging directory not found.");
            }

            LogFolder.Refresh();
            if (!LogFolder.Exists)
            {
                LogFolder.Create();
            }
        }

        static void Tick(object state)
        {
            try
            {
                string logMessage;
                lock (LogQueue)
                {
                    logMessage = LogQueue.ToString();
                    LogQueue.Length = 0;
                }

                if (string.IsNullOrEmpty(logMessage))
                {
                    return;
                }

                if (LogToConsole)
                {
                    Console.Write(logMessage);
                }

                VerifyLogFolder(); // File may be deleted after initialization.
                File.AppendAllText(LogFile.FullName, logMessage);
            }
            finally
            {
                if (Listening)
                {
                    Timer.Change(BatchInterval, Timeout.Infinite); // Reset timer for next tick.
                }
            }
        }

        bool CheckListening()
        {
            if (Listening) return true;
            if (_errorShown) return false;

            Console.WriteLine("Logging has not been started.");
            _errorShown = true; // No need to excessively repeat this message.

            return false;
        }
    }
}
