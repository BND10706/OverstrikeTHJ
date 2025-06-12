using System;
using System.IO;
using System.Reflection;
using System.Windows;

namespace Overstrike.Utilities
{
    public static class DebugHelper
    {
        private static readonly string LogFilePath = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,
            "overstrike_debug.log");

        static DebugHelper()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                LogError("Unhandled Exception", args.ExceptionObject as Exception);
                MessageBox.Show(
                    $"An unhandled error occurred. See the log file for details:\n{LogFilePath}",
                    "Overstrike Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            };
        }

        public static void LogInfo(string message)
        {
            Log("INFO", message);
        }

        public static void LogError(string context, Exception ex)
        {
            Log("ERROR", $"{context}: {ex.Message}");
            Log("STACK", ex.StackTrace ?? "No stack trace available");

            if (ex.InnerException != null)
            {
                Log("INNER", ex.InnerException.Message);
                Log("INNER_STACK", ex.InnerException.StackTrace ?? "No stack trace available");
            }
        }

        private static void Log(string level, string message)
        {
            try
            {
                string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";
                File.AppendAllText(LogFilePath, logMessage + Environment.NewLine);

                // Also output to console if available
                Console.WriteLine(logMessage);
            }
            catch
            {
                // Suppress errors in the error logger
            }
        }
    }
}
