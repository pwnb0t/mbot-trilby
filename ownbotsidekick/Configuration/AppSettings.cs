using System.Text.Json.Serialization;

namespace ownbotsidekick.Configuration
{
    internal sealed class AppSettings
    {
        public HotkeySettings Hotkey { get; set; } = new();
        public OverlaySettings Overlay { get; set; } = new();
        public SidekickApiSettings SidekickApi { get; set; } = new();
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
        public string QuickPlay1Trigger { get; set; } = "clip-a";
        public string QuickPlay2Trigger { get; set; } = "clip-b";
        public string QuickPlay3Trigger { get; set; } = "clip-c";

        [JsonPropertyName("ClipATrigger")]
        public string LegacyClipATrigger { set => QuickPlay1Trigger = value; }

        [JsonPropertyName("ClipBTrigger")]
        public string LegacyClipBTrigger { set => QuickPlay2Trigger = value; }

        [JsonPropertyName("ClipCTrigger")]
        public string LegacyClipCTrigger { set => QuickPlay3Trigger = value; }
    }

    internal sealed class InputBindingsSettings
    {
        public string HideOverlayKey { get; set; } = "Escape";
        public string ClearSearchKey { get; set; } = "Tab";
        public string PlayFirstPrimaryKey { get; set; } = "Enter";
        public string PlayFirstSecondaryKey { get; set; } = "Space";
    }
}
