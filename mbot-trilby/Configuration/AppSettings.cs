namespace mbottrilby.Configuration
{
    internal sealed class AppSettings
    {
        public HotkeySettings Hotkey { get; set; } = new();
        public OverlaySettings Overlay { get; set; } = new();
        public TrilbyEnvironmentCatalogSettings TrilbyEnvironments { get; set; } = TrilbyEnvironmentCatalogSettings.CreateDefaults();
        public InputBindingsSettings InputBindings { get; set; } = new();
    }

    internal sealed class HotkeySettings
    {
        public string Modifiers { get; set; } = "Alt";
        public string Key { get; set; } = "Oem3";
    }

    internal sealed class OverlaySettings
    {
        public bool Topmost { get; set; } = true;
    }

    public sealed class TrilbyEnvironmentSettings
    {
        public string DisplayName { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
    }

    public sealed class TrilbyEnvironmentCatalogSettings
    {
        public TrilbyEnvironmentSettings Dev { get; set; } = new()
        {
            DisplayName = "Development",
            BaseUrl = "http://127.0.0.1:28765"
        };

        public TrilbyEnvironmentSettings Test { get; set; } = new()
        {
            DisplayName = "Test",
            BaseUrl = "https://trilby-test.pwnb0t.com"
        };

        public TrilbyEnvironmentSettings Prod { get; set; } = new()
        {
            DisplayName = "Production",
            BaseUrl = "https://trilby.pwnb0t.com"
        };

        public static TrilbyEnvironmentCatalogSettings CreateDefaults()
        {
            return new TrilbyEnvironmentCatalogSettings();
        }

        public TrilbyEnvironmentSettings GetByName(string environmentName)
        {
            return environmentName.ToLowerInvariant() switch
            {
                "dev" => Dev,
                "test" => Test,
                "prod" => Prod,
                _ => Dev
            };
        }
    }

    internal sealed class InputBindingsSettings
    {
        public string HideOverlayKey { get; set; } = "Escape";
        public string ClearSearchKey { get; set; } = "Tab";
        public string PlayFirstPrimaryKey { get; set; } = "Enter";
        public string PlayFirstSecondaryKey { get; set; } = "Space";
    }
}
