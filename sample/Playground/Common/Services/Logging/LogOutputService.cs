using System;
using System.Globalization;
using System.IO;
using System.Reactive.Linq;
using Genesis.Logging;
using ReactiveUI;
using Xamarin.Essentials;

namespace Playground.Common.Services.Logging
{
    public static class LogOutputService
    {
        private static readonly string _logOutputFolderPath = $"{FileSystem.CacheDirectory}/logs";
        private static readonly long _currentSessionId = DateTime.Now.Ticks;

        public static void Initialize()
        {
            ConfigureAmbientLoggerService();
            DirectLoggingOutputToConsole();
        }

        private static void ConfigureAmbientLoggerService()
        {
            LoggerService.Current = new DefaultLoggerService();
        }

        private static void DirectLoggingOutputToConsole()
        {
            LoggerService
                .Current
                .Entries
                .ObserveOn(RxApp.TaskpoolScheduler)
                .Where(ShouldLogEntry)
                .Select(CreateLogMessage)
                .Do(LogToConsole)
                .Do(TryLogToFile)
                .Subscribe();
        }

        private static bool ShouldLogEntry(LogEntry arg)
        {
#if DEBUG
            return true;
#else
            return arg.Level == LogLevel.Info || arg.Level == LogLevel.Warn || arg.Level == LogLevel.Error;
#endif
        }

        private static string CreateLogMessage(LogEntry entry)
        {
            return $"{entry.Timestamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)} " +
                   $"[{entry.Level.ToString()}] #{entry.ThreadId} {entry.Name} : {entry.Message}";
        }

        private static void LogToConsole(string message)
        {
            Console.Out.WriteLine(message);
        }

        private static void TryLogToFile(string message)
        {
            try {
                LogToFile(message);
            } catch(Exception e) {
                // ReSharper disable once LocalizableElement
                Console.WriteLine($"Couldn't log to file: {e.Message}\n{e.StackTrace}");
            }
        }

        private static void LogToFile(string message)
        {
            var filePath = $"{_logOutputFolderPath}/log_{_currentSessionId}.txt";
            Directory.CreateDirectory(_logOutputFolderPath);

            var streamWriter = File.AppendText(filePath);
            streamWriter.WriteLine(message);
            streamWriter.Close();
        }

        public static string[] GetLogOutputFilePaths()
        {
            return Directory.GetFiles(_logOutputFolderPath);
        }
    }
}