using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace OctopusDeployNuGetFeed.Logging
{
    public class FileLogger : ILogger
    {
        private static readonly string RootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static readonly string LogFileNameTemplate = $"{Assembly.GetExecutingAssembly().GetName().Name}.{{0:yyyy-MM-dd}}.log";
        private readonly ILogger _consoleLogger;

        public FileLogger(ILogger consoleLogger)
        {
            _consoleLogger = consoleLogger;
        }

        private static string LogFile => Path.Combine(RootPath, string.Format(LogFileNameTemplate, DateTime.Now));

        public void Critical(string message)
        {
            WriteLogFile(message, "CRITICAL");
        }

        public void Error(string message)
        {
            WriteLogFile(message, "ERROR");
        }

        public void Warning(string message)
        {
            WriteLogFile(message, "WARNING");
        }

        public void Info(string message)
        {
            WriteLogFile(message, "INFO");
        }

        public void Exception(Exception exception, string callerFilePath = null, string callerMemberName = null)
        {
        }

        public void UnhandledException(Exception exception)
        {
        }

        public void Init()
        {
            Info($"Initializing log file: {LogFile}");
            foreach (var logFileToRemove in Directory.GetFiles(RootPath, "*.log", SearchOption.TopDirectoryOnly).OrderByDescending(logFile => logFile).Skip(7))
                try
                {
                    Info($"Removing old log file: {logFileToRemove}");
                    File.Delete(logFileToRemove);
                }
                catch (Exception e)
                {
                    Warning($"Unable to remove log file {logFileToRemove}: {e.Message}");
                }
        }

        private void WriteLogFile(string message, string prefix)
        {
            var buffer = Encoding.UTF8.GetBytes($"{Environment.NewLine}[{DateTime.Now:HH:mm:ss}] {Thread.CurrentThread.ManagedThreadId} {prefix}: {message}");

            try
            {
                using (var fileStream = File.Open(LogFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                {
                    fileStream.Write(buffer, 0, buffer.Length);
                }
            }
            catch (Exception e)
            {
                _consoleLogger.Error($"FileLogger.WriteLogFile: {e.Message}");
            }
        }
    }
}