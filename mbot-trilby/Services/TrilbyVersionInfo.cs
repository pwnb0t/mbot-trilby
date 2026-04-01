using System.Reflection;

namespace mbottrilby.Services
{
    internal static class TrilbyVersionInfo
    {
        public static string CurrentVersion { get; } =
            Assembly.GetEntryAssembly()?.GetName().Version?.ToString()
            ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString()
            ?? "unknown";
    }
}
