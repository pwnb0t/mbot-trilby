using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ownbotsidekick.Services
{
    internal sealed class QuickPlaySettings
    {
        public string? Slot1Trigger { get; set; }
        public string? Slot2Trigger { get; set; }
        public string? Slot3Trigger { get; set; }
        public string? Slot4Trigger { get; set; }
        public string? Slot5Trigger { get; set; }
        public string? Slot6Trigger { get; set; }
        public string? Slot7Trigger { get; set; }
        public string? Slot8Trigger { get; set; }

        public static QuickPlaySettings CreateEmpty()
        {
            return new QuickPlaySettings
            {
                Slot1Trigger = null,
                Slot2Trigger = null,
                Slot3Trigger = null,
                Slot4Trigger = null,
                Slot5Trigger = null,
                Slot6Trigger = null,
                Slot7Trigger = null,
                Slot8Trigger = null
            };
        }

        public string? GetTrigger(int slotIndex)
        {
            return slotIndex switch
            {
                1 => Slot1Trigger,
                2 => Slot2Trigger,
                3 => Slot3Trigger,
                4 => Slot4Trigger,
                5 => Slot5Trigger,
                6 => Slot6Trigger,
                7 => Slot7Trigger,
                8 => Slot8Trigger,
                _ => null
            };
        }

        public void SetTrigger(int slotIndex, string? trigger)
        {
            switch (slotIndex)
            {
                case 1:
                    Slot1Trigger = trigger;
                    break;
                case 2:
                    Slot2Trigger = trigger;
                    break;
                case 3:
                    Slot3Trigger = trigger;
                    break;
                case 4:
                    Slot4Trigger = trigger;
                    break;
                case 5:
                    Slot5Trigger = trigger;
                    break;
                case 6:
                    Slot6Trigger = trigger;
                    break;
                case 7:
                    Slot7Trigger = trigger;
                    break;
                case 8:
                    Slot8Trigger = trigger;
                    break;
            }
        }
    }

    internal sealed class TagSettings
    {
        public string? SelectedTagName { get; set; }
    }

    internal sealed class UserSettingsState
    {
        public QuickPlaySettings QuickPlay { get; set; } = QuickPlaySettings.CreateEmpty();
        public TagSettings Tags { get; set; } = new();

        public static UserSettingsState CreateEmpty()
        {
            return new UserSettingsState
            {
                QuickPlay = QuickPlaySettings.CreateEmpty(),
                Tags = new TagSettings()
            };
        }

        public string? GetTrigger(int slotIndex)
        {
            return QuickPlay.GetTrigger(slotIndex);
        }

        public void SetTrigger(int slotIndex, string? trigger)
        {
            QuickPlay.SetTrigger(slotIndex, trigger);
        }

        [JsonIgnore]
        public string? SelectedTagName
        {
            get => Tags.SelectedTagName;
            set => Tags.SelectedTagName = value;
        }
    }

    internal sealed class UserSettingsStateStore
    {
        private readonly string _userSettingsPath;
        private readonly JsonSerializerOptions _serializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        public UserSettingsStateStore(string baseDirectoryPath)
        {
            Directory.CreateDirectory(baseDirectoryPath);
            _userSettingsPath = Path.Combine(baseDirectoryPath, "user-settings.json");
        }

        public UserSettingsState Load()
        {
            var fallback = UserSettingsState.CreateEmpty();

            if (File.Exists(_userSettingsPath))
            {
                return LoadUserSettings(fallback);
            }

            Save(fallback);
            return fallback;
        }

        public void Save(UserSettingsState settings)
        {
            var json = JsonSerializer.Serialize(settings, _serializerOptions);
            File.WriteAllText(_userSettingsPath, json);
        }

        private UserSettingsState LoadUserSettings(UserSettingsState fallback)
        {
            try
            {
                var json = File.ReadAllText(_userSettingsPath);
                var settings = JsonSerializer.Deserialize<UserSettingsState>(json, _serializerOptions);
                return Normalize(settings) ?? fallback;
            }
            catch
            {
                return fallback;
            }
        }

        private static UserSettingsState? Normalize(UserSettingsState? settings)
        {
            if (settings is null)
            {
                return null;
            }

            settings.QuickPlay ??= QuickPlaySettings.CreateEmpty();
            settings.Tags ??= new TagSettings();
            return settings;
        }
    }
}
