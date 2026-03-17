using System;
using System.IO;
using System.Text.Json;

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

    internal sealed class LegacyFlatUserSettingsState
    {
        public string? Slot1Trigger { get; set; }
        public string? Slot2Trigger { get; set; }
        public string? Slot3Trigger { get; set; }
        public string? Slot4Trigger { get; set; }
        public string? Slot5Trigger { get; set; }
        public string? Slot6Trigger { get; set; }
        public string? Slot7Trigger { get; set; }
        public string? Slot8Trigger { get; set; }
        public string? SelectedTagName { get; set; }

        public UserSettingsState ToUserSettingsState()
        {
            return new UserSettingsState
            {
                QuickPlay = new QuickPlaySettings
                {
                    Slot1Trigger = Slot1Trigger,
                    Slot2Trigger = Slot2Trigger,
                    Slot3Trigger = Slot3Trigger,
                    Slot4Trigger = Slot4Trigger,
                    Slot5Trigger = Slot5Trigger,
                    Slot6Trigger = Slot6Trigger,
                    Slot7Trigger = Slot7Trigger,
                    Slot8Trigger = Slot8Trigger
                },
                Tags = new TagSettings
                {
                    SelectedTagName = SelectedTagName
                }
            };
        }
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

        public string? SelectedTagName
        {
            get => Tags.SelectedTagName;
            set => Tags.SelectedTagName = value;
        }
    }

    internal sealed class UserSettingsStateStore
    {
        private readonly string _userSettingsPath;
        private readonly string _legacyQuickPlayPath;
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
            _legacyQuickPlayPath = Path.Combine(baseDirectoryPath, "quickplay.json");
        }

        public UserSettingsState Load()
        {
            var fallback = UserSettingsState.CreateEmpty();

            if (File.Exists(_userSettingsPath))
            {
                return LoadUserSettings(fallback);
            }

            if (File.Exists(_legacyQuickPlayPath))
            {
                var migrated = LoadLegacyQuickPlaySettings(fallback);
                Save(migrated);
                TryDeleteLegacyQuickPlayFile();
                return migrated;
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
                using var document = JsonDocument.Parse(json);
                if (document.RootElement.ValueKind == JsonValueKind.Object)
                {
                    if (document.RootElement.TryGetProperty("quickPlay", out _) ||
                        document.RootElement.TryGetProperty("tags", out _))
                    {
                        var nestedSettings = JsonSerializer.Deserialize<UserSettingsState>(json, _serializerOptions);
                        return Normalize(nestedSettings) ?? fallback;
                    }
                }

                var flatSettings = JsonSerializer.Deserialize<LegacyFlatUserSettingsState>(json, _serializerOptions);
                if (flatSettings is null)
                {
                    return fallback;
                }

                var migratedSettings = flatSettings.ToUserSettingsState();
                Save(migratedSettings);
                return migratedSettings;
            }
            catch
            {
                return fallback;
            }
        }

        private UserSettingsState LoadLegacyQuickPlaySettings(UserSettingsState fallback)
        {
            try
            {
                var json = File.ReadAllText(_legacyQuickPlayPath);
                var legacySettings = JsonSerializer.Deserialize<QuickPlaySettings>(json, _serializerOptions);
                if (legacySettings is null)
                {
                    return fallback;
                }

                return new UserSettingsState
                {
                    QuickPlay = legacySettings,
                    Tags = new TagSettings()
                };
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

        private void TryDeleteLegacyQuickPlayFile()
        {
            try
            {
                File.Delete(_legacyQuickPlayPath);
            }
            catch
            {
                // Ignore deletion failure; the new canonical file is already written.
            }
        }
    }
}
