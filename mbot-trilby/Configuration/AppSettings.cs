using System;
using System.Collections.Generic;
using System.Linq;

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
        public TrilbyEnvironmentSettings? Dev { get; set; }

        public TrilbyEnvironmentSettings? Test { get; set; }

        public TrilbyEnvironmentSettings? Prod { get; set; }

        public static TrilbyEnvironmentCatalogSettings CreateDefaults()
        {
            return new TrilbyEnvironmentCatalogSettings
            {
                Dev = new TrilbyEnvironmentSettings
                {
                    DisplayName = "Development",
                    BaseUrl = "http://127.0.0.1:28765"
                },
                Test = new TrilbyEnvironmentSettings
                {
                    DisplayName = "Test",
                    BaseUrl = "https://trilby-test.pwnb0t.com"
                },
                Prod = new TrilbyEnvironmentSettings
                {
                    DisplayName = "Production",
                    BaseUrl = "https://trilby.pwnb0t.com"
                }
            };
        }

        public IReadOnlyList<TrilbyEnvironmentCatalogEntry> GetAvailableEnvironments()
        {
            var environments = new List<TrilbyEnvironmentCatalogEntry>();
            AddIfConfigured(environments, "dev", Dev);
            AddIfConfigured(environments, "test", Test);
            AddIfConfigured(environments, "prod", Prod);
            return environments;
        }

        public TrilbyEnvironmentSettings GetByName(string environmentName)
        {
            var configuredEnvironment = environmentName.ToLowerInvariant() switch
            {
                "dev" => Dev,
                "test" => Test,
                "prod" => Prod,
                _ => null
            };

            if (IsConfigured(configuredEnvironment))
            {
                return configuredEnvironment!;
            }

            var fallback = GetAvailableEnvironments().FirstOrDefault();
            if (fallback is not null)
            {
                return fallback.Settings;
            }

            return CreateDefaults().Prod!;
        }

        public string GetDefaultEnvironmentName()
        {
            return GetAvailableEnvironments().FirstOrDefault()?.Name ?? "prod";
        }

        private static void AddIfConfigured(
            ICollection<TrilbyEnvironmentCatalogEntry> environments,
            string name,
            TrilbyEnvironmentSettings? settings)
        {
            if (!IsConfigured(settings))
            {
                return;
            }

            environments.Add(new TrilbyEnvironmentCatalogEntry(name, settings!));
        }

        private static bool IsConfigured(TrilbyEnvironmentSettings? settings)
        {
            return settings is not null &&
                !string.IsNullOrWhiteSpace(settings.DisplayName) &&
                !string.IsNullOrWhiteSpace(settings.BaseUrl);
        }
    }

    public sealed class TrilbyEnvironmentCatalogEntry
    {
        public TrilbyEnvironmentCatalogEntry(string name, TrilbyEnvironmentSettings settings)
        {
            Name = name;
            Settings = settings;
        }

        public string Name { get; }

        public TrilbyEnvironmentSettings Settings { get; }
    }

    internal sealed class InputBindingsSettings
    {
        public string HideOverlayKey { get; set; } = "Escape";
        public string ClearSearchKey { get; set; } = "Tab";
        public string PlayFirstPrimaryKey { get; set; } = "Enter";
        public string PlayFirstSecondaryKey { get; set; } = "Space";
    }
}
