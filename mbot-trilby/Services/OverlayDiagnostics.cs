using System;
using System.Diagnostics;
using System.IO;

namespace mbottrilby.Services
{
    internal sealed class OverlayDiagnostics
    {
        private readonly string _logFilePath;

        public OverlayDiagnostics(string logFilePath)
        {
            _logFilePath = logFilePath;
        }

        public void Info(string category, string message)
        {
            Write("INFO", category, message);
        }

        public void Error(string category, string message, Exception? exception = null)
        {
            string fullMessage = exception is null
                ? message
                : $"{message} Exception={exception.GetType().Name}: {exception.Message}";
            Write("ERROR", category, fullMessage);
        }

        public void OverlayShown()
        {
            Info("overlay", "Overlay shown.");
        }

        public void OverlayShownFromTray()
        {
            Info("overlay", "Overlay shown from tray.");
        }

        public void OverlayHidden(string reason)
        {
            Info("overlay", $"Overlay hidden. reason={reason}");
        }

        public void HookInitialized(string name)
        {
            Info("input", $"{name} hook initialized.");
        }

        public void HookInitFailed(string name)
        {
            Error("input", $"{name} hook initialization failed.");
        }

        private void Write(string level, string category, string message)
        {
            string timestamped = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] [{category}] {message}";
            Debug.WriteLine(timestamped);

            try
            {
                File.AppendAllText(_logFilePath, timestamped + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to write log file: {ex.Message}");
            }
        }
    }
}
