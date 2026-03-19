namespace ownbotsidekick.Configuration
{
    internal sealed class AppSettings
    {
        public HotkeySettings Hotkey { get; set; } = new();
        public OverlaySettings Overlay { get; set; } = new();
        public SidekickApiSettings SidekickApi { get; set; } = new();
        public SidekickEnvironmentCatalogSettings SidekickEnvironments { get; set; } = SidekickEnvironmentCatalogSettings.CreateDefaults();
        public InputBindingsSettings InputBindings { get; set; } = new();
    }

    internal sealed class HotkeySettings
    {
        public string Modifiers { get; set; } = "Alt";
        public string Key { get; set; } = "Oem3";
    }

    internal sealed class OverlaySettings
    {
        public bool StartHidden { get; set; }
        public bool Topmost { get; set; } = true;
    }

    internal sealed class SidekickApiSettings
    {
        public bool Enabled { get; set; }
        public string BaseUrl { get; set; } = "http://127.0.0.1:28765";
        public string ApiToken { get; set; } = string.Empty;
        public long GuildId { get; set; }
        public long RequestingUserId { get; set; }
    }

    public sealed class SidekickEnvironmentSettings
    {
        public string DisplayName { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
    }

    public sealed class SidekickEnvironmentCatalogSettings
    {
        public SidekickEnvironmentSettings Dev { get; set; } = new()
        {
            DisplayName = "Development",
            BaseUrl = "http://127.0.0.1:28765"
        };

        public SidekickEnvironmentSettings Test { get; set; } = new()
        {
            DisplayName = "Test",
            BaseUrl = "https://sidekick-test.pwnb0t.com"
        };

        public SidekickEnvironmentSettings Prod { get; set; } = new()
        {
            DisplayName = "Production",
            BaseUrl = "https://sidekick.pwnb0t.com"
        };

        public static SidekickEnvironmentCatalogSettings CreateDefaults()
        {
            return new SidekickEnvironmentCatalogSettings();
        }

        public SidekickEnvironmentSettings GetByName(string environmentName)
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
