using System.IO;
using System.Text.Json;

namespace mbottrilby.Configuration
{
    internal static class AppSettingsLoader
    {
        public static AppSettings LoadFromBaseDirectory(string appBaseDirectory)
        {
            string settingsPath = Path.Combine(appBaseDirectory, "appsettings.json");
            if (!File.Exists(settingsPath))
            {
                return new AppSettings();
            }

            try
            {
                string json = File.ReadAllText(settingsPath);
                mbottrilby.Configuration.AppSettings settings = JsonSerializer.Deserialize<AppSettings>(json, new JsonSerializerOptions
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
