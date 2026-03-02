using System.IO;
using System.Text.Json;

namespace ownbotsidekick.Configuration
{
    internal static class AppSettingsLoader
    {
        public static AppSettings LoadFromBaseDirectory(string appBaseDirectory)
        {
            var settingsPath = Path.Combine(appBaseDirectory, "appsettings.json");
            if (!File.Exists(settingsPath))
            {
                return new AppSettings();
            }

            try
            {
                var json = File.ReadAllText(settingsPath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return settings ?? new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }
    }
}
