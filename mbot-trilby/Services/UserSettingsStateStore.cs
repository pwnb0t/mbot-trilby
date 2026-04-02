using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace mbottrilby.Services
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
            return new QuickPlaySettings();
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

    internal sealed class EnvironmentSettings
    {
        public string SelectedName { get; set; } = "dev";
    }

    internal sealed class OptionSettings
    {
        public bool OpaqueBackground { get; set; }
        public bool DoNotHideWhenPlayingClip { get; set; }
    }

    internal sealed class ServerSelectionSettings
    {
        public long? Dev { get; set; }
        public long? Test { get; set; }
        public long? Prod { get; set; }

        public long? GetSelectedGuildId(string environmentName)
        {
            return environmentName.ToLowerInvariant() switch
            {
                "dev" => Dev,
                "test" => Test,
                "prod" => Prod,
                _ => null
            };
        }

        public void SetSelectedGuildId(string environmentName, long? guildId)
        {
            switch (environmentName.ToLowerInvariant())
            {
                case "dev":
                    Dev = guildId;
                    break;
                case "test":
                    Test = guildId;
                    break;
                case "prod":
                    Prod = guildId;
                    break;
            }
        }
    }

    public sealed class TrilbyGuildSettings
    {
        public long GuildId { get; set; }
        public string? GuildName { get; set; }
    }

    public sealed class TrilbySessionSettings
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public string? ExpiresAtUtc { get; set; }
        public long UserId { get; set; }
        public string? Username { get; set; }
        public List<TrilbyGuildSettings> Servers { get; set; } = new();
        public long? DefaultGuildId { get; set; }

        public bool IsAuthenticated =>
            !string.IsNullOrWhiteSpace(AccessToken) &&
            !string.IsNullOrWhiteSpace(RefreshToken) &&
            UserId > 0;
    }

    internal sealed class AuthSettings
    {
        public TrilbySessionSettings? Dev { get; set; }
        public TrilbySessionSettings? Test { get; set; }
        public TrilbySessionSettings? Prod { get; set; }

        public TrilbySessionSettings? GetSession(string environmentName)
        {
            return environmentName.ToLowerInvariant() switch
            {
                "dev" => Dev,
                "test" => Test,
                "prod" => Prod,
                _ => null
            };
        }

        public void SetSession(string environmentName, TrilbySessionSettings? session)
        {
            switch (environmentName.ToLowerInvariant())
            {
                case "dev":
                    Dev = session;
                    break;
                case "test":
                    Test = session;
                    break;
                case "prod":
                    Prod = session;
                    break;
            }
        }
    }

    internal sealed class ServerQuickPlaySettings
    {
        public string EnvironmentName { get; set; } = "dev";
        public long GuildId { get; set; }
        public QuickPlaySettings QuickPlay { get; set; } = QuickPlaySettings.CreateEmpty();
    }

    internal sealed class QuickPlayStateCatalog
    {
        public List<ServerQuickPlaySettings> Servers { get; set; } = new();
    }

    internal sealed class ServerTagState
    {
        public string EnvironmentName { get; set; } = "dev";
        public long GuildId { get; set; }
        public TagSettings Tags { get; set; } = new();
    }

    internal sealed class TagStateCatalog
    {
        public List<ServerTagState> Servers { get; set; } = new();
    }

    internal sealed class UserSettingsState
    {
        public QuickPlayStateCatalog QuickPlay { get; set; } = new();
        public TagStateCatalog Tags { get; set; } = new();
        public EnvironmentSettings Environment { get; set; } = new();
        public OptionSettings Options { get; set; } = new();
        public AuthSettings Auth { get; set; } = new();
        public ServerSelectionSettings ServerSelections { get; set; } = new();

        public static UserSettingsState CreateEmpty()
        {
            return new UserSettingsState();
        }

        public string SelectedEnvironmentName
        {
            get => Environment.SelectedName;
            set => Environment.SelectedName = string.IsNullOrWhiteSpace(value) ? "dev" : value.Trim().ToLowerInvariant();
        }

        public TrilbySessionSettings? GetSession(string environmentName)
        {
            return Auth.GetSession(environmentName);
        }

        public void SetSession(string environmentName, TrilbySessionSettings? session)
        {
            Auth.SetSession(environmentName, session);
        }

        public long? GetSelectedGuildId(string environmentName)
        {
            return ServerSelections.GetSelectedGuildId(environmentName);
        }

        public void SetSelectedGuildId(string environmentName, long? guildId)
        {
            ServerSelections.SetSelectedGuildId(environmentName, guildId);
        }

        public string? GetTrigger(string environmentName, long guildId, int slotIndex)
        {
            return GetOrCreateQuickPlay(environmentName, guildId).GetTrigger(slotIndex);
        }

        public void SetTrigger(string environmentName, long guildId, int slotIndex, string? trigger)
        {
            GetOrCreateQuickPlay(environmentName, guildId).SetTrigger(slotIndex, trigger);
        }

        public string? GetSelectedTagName(string environmentName, long guildId)
        {
            return GetOrCreateTagSettings(environmentName, guildId).SelectedTagName;
        }

        public void SetSelectedTagName(string environmentName, long guildId, string? tagName)
        {
            GetOrCreateTagSettings(environmentName, guildId).SelectedTagName = tagName;
        }

        private QuickPlaySettings GetOrCreateQuickPlay(string environmentName, long guildId)
        {
            string normalizedEnvironmentName = NormalizeEnvironmentName(environmentName);
            mbottrilby.Services.ServerQuickPlaySettings entry = QuickPlay.Servers.FirstOrDefault(candidate =>
                string.Equals(candidate.EnvironmentName, normalizedEnvironmentName, StringComparison.OrdinalIgnoreCase) &&
                candidate.GuildId == guildId);
            if (entry is not null)
            {
                return entry.QuickPlay;
            }

            entry = new ServerQuickPlaySettings
            {
                EnvironmentName = normalizedEnvironmentName,
                GuildId = guildId,
                QuickPlay = QuickPlaySettings.CreateEmpty()
            };
            QuickPlay.Servers.Add(entry);
            return entry.QuickPlay;
        }

        private TagSettings GetOrCreateTagSettings(string environmentName, long guildId)
        {
            string normalizedEnvironmentName = NormalizeEnvironmentName(environmentName);
            mbottrilby.Services.ServerTagState entry = Tags.Servers.FirstOrDefault(candidate =>
                string.Equals(candidate.EnvironmentName, normalizedEnvironmentName, StringComparison.OrdinalIgnoreCase) &&
                candidate.GuildId == guildId);
            if (entry is not null)
            {
                return entry.Tags;
            }

            entry = new ServerTagState
            {
                EnvironmentName = normalizedEnvironmentName,
                GuildId = guildId,
                Tags = new TagSettings()
            };
            Tags.Servers.Add(entry);
            return entry.Tags;
        }

        private static string NormalizeEnvironmentName(string environmentName)
        {
            return string.IsNullOrWhiteSpace(environmentName) ? "dev" : environmentName.Trim().ToLowerInvariant();
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
            mbottrilby.Services.UserSettingsState fallback = UserSettingsState.CreateEmpty();

            if (File.Exists(_userSettingsPath))
            {
                return LoadUserSettings(fallback);
            }

            Save(fallback);
            return fallback;
        }

        public void Save(UserSettingsState settings)
        {
            string json = JsonSerializer.Serialize(settings, _serializerOptions);
            File.WriteAllText(_userSettingsPath, json);
        }

        private UserSettingsState LoadUserSettings(UserSettingsState fallback)
        {
            try
            {
                string json = File.ReadAllText(_userSettingsPath);
                using System.Text.Json.JsonDocument document = JsonDocument.Parse(json);
                return Normalize(LoadFromDocument(document.RootElement)) ?? fallback;
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

            settings.QuickPlay ??= new QuickPlayStateCatalog();
            settings.Tags ??= new TagStateCatalog();
            settings.Environment ??= new EnvironmentSettings();
            settings.Options ??= new OptionSettings();
            settings.Auth ??= new AuthSettings();
            settings.ServerSelections ??= new ServerSelectionSettings();
            settings.QuickPlay.Servers ??= new List<ServerQuickPlaySettings>();
            settings.Tags.Servers ??= new List<ServerTagState>();

            foreach (mbottrilby.Services.TrilbySessionSettings session in new[] { settings.Auth.Dev, settings.Auth.Test, settings.Auth.Prod })
            {
                if (session?.Servers is null)
                {
                    continue;
                }

                session.Servers = session.Servers
                    .Where(server => server.GuildId > 0)
                    .OrderBy(server => server.GuildName, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }

            return settings;
        }

        private static UserSettingsState LoadFromDocument(JsonElement root)
        {
            mbottrilby.Services.UserSettingsState state = UserSettingsState.CreateEmpty();
            state.SelectedEnvironmentName = ReadEnvironmentName(root);
            state.Options = ReadOptions(root);
            state.Auth = ReadAuthSettings(root);
            state.ServerSelections = ReadServerSelections(root, state.Auth);
            state.QuickPlay = ReadQuickPlaySettings(root, state);
            state.Tags = ReadTagSettings(root, state);
            return state;
        }

        private static string ReadEnvironmentName(JsonElement root)
        {
            if (TryGetProperty(root, "environment", out System.Text.Json.JsonElement environmentElement) &&
                TryGetProperty(environmentElement, "selectedName", out System.Text.Json.JsonElement selectedNameElement) &&
                selectedNameElement.ValueKind == JsonValueKind.String)
            {
                return selectedNameElement.GetString() ?? "dev";
            }

            return "dev";
        }

        private static OptionSettings ReadOptions(JsonElement root)
        {
            mbottrilby.Services.OptionSettings options = new OptionSettings();
            if (!TryGetProperty(root, "options", out System.Text.Json.JsonElement optionsElement) || optionsElement.ValueKind != JsonValueKind.Object)
            {
                return options;
            }

            if (TryGetProperty(optionsElement, "opaqueBackground", out System.Text.Json.JsonElement opaqueBackgroundElement))
            {
                options.OpaqueBackground = opaqueBackgroundElement.ValueKind switch
                {
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.String when bool.TryParse(opaqueBackgroundElement.GetString(), out bool parsed) => parsed,
                    _ => false
                };
            }

            if (TryGetProperty(optionsElement, "doNotHideWhenPlayingClip", out System.Text.Json.JsonElement doNotHideElement))
            {
                options.DoNotHideWhenPlayingClip = doNotHideElement.ValueKind switch
                {
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.String when bool.TryParse(doNotHideElement.GetString(), out bool parsed) => parsed,
                    _ => false
                };
            }

            return options;
        }

        private static AuthSettings ReadAuthSettings(JsonElement root)
        {
            mbottrilby.Services.AuthSettings auth = new AuthSettings();
            if (!TryGetProperty(root, "auth", out System.Text.Json.JsonElement authElement) || authElement.ValueKind != JsonValueKind.Object)
            {
                return auth;
            }

            auth.Dev = ReadSession(authElement, "dev");
            auth.Test = ReadSession(authElement, "test");
            auth.Prod = ReadSession(authElement, "prod");
            return auth;
        }

        private static TrilbySessionSettings? ReadSession(JsonElement authElement, string environmentName)
        {
            if (!TryGetProperty(authElement, environmentName, out System.Text.Json.JsonElement sessionElement) || sessionElement.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            mbottrilby.Services.TrilbySessionSettings session = new TrilbySessionSettings
            {
                AccessToken = ReadString(sessionElement, "accessToken"),
                RefreshToken = ReadString(sessionElement, "refreshToken"),
                ExpiresAtUtc = ReadString(sessionElement, "expiresAtUtc"),
                UserId = ReadInt64(sessionElement, "userId"),
                Username = ReadString(sessionElement, "username"),
                DefaultGuildId = ReadNullableInt64(sessionElement, "defaultGuildId"),
                Servers = ReadServers(sessionElement)
            };

            if (session.Servers.Count == 0)
            {
                long legacyGuildId = ReadInt64(sessionElement, "guildId");
                if (legacyGuildId > 0)
                {
                    session.Servers.Add(new TrilbyGuildSettings
                    {
                        GuildId = legacyGuildId,
                        GuildName = ReadString(sessionElement, "guildName")
                    });
                }
            }

            return session;
        }

        private static List<TrilbyGuildSettings> ReadServers(JsonElement sessionElement)
        {
            System.Collections.Generic.List<mbottrilby.Services.TrilbyGuildSettings> servers = new List<TrilbyGuildSettings>();
            if (!TryGetProperty(sessionElement, "servers", out System.Text.Json.JsonElement serversElement) &&
                !TryGetProperty(sessionElement, "guilds", out serversElement))
            {
                return servers;
            }

            if (serversElement.ValueKind != JsonValueKind.Array)
            {
                return servers;
            }

            foreach (System.Text.Json.JsonElement serverElement in serversElement.EnumerateArray())
            {
                long guildId = ReadInt64(serverElement, "guildId");
                if (guildId <= 0)
                {
                    guildId = ReadInt64(serverElement, "guild_id");
                }

                if (guildId <= 0)
                {
                    continue;
                }

                servers.Add(new TrilbyGuildSettings
                {
                    GuildId = guildId,
                    GuildName = ReadString(serverElement, "guildName") ?? ReadString(serverElement, "guild_name")
                });
            }

            return servers
                .OrderBy(server => server.GuildName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static ServerSelectionSettings ReadServerSelections(JsonElement root, AuthSettings auth)
        {
            mbottrilby.Services.ServerSelectionSettings selections = new ServerSelectionSettings
            {
                Dev = ReadSelectedGuildId(root, "dev"),
                Test = ReadSelectedGuildId(root, "test"),
                Prod = ReadSelectedGuildId(root, "prod")
            };

            selections.Dev ??= auth.Dev?.Servers.Count == 1 ? auth.Dev.Servers[0].GuildId : null;
            selections.Test ??= auth.Test?.Servers.Count == 1 ? auth.Test.Servers[0].GuildId : null;
            selections.Prod ??= auth.Prod?.Servers.Count == 1 ? auth.Prod.Servers[0].GuildId : null;
            return selections;
        }

        private static long? ReadSelectedGuildId(JsonElement root, string environmentName)
        {
            if (!TryGetProperty(root, "serverSelections", out System.Text.Json.JsonElement selectionsElement) ||
                selectionsElement.ValueKind != JsonValueKind.Object ||
                !TryGetProperty(selectionsElement, environmentName, out System.Text.Json.JsonElement guildElement))
            {
                return null;
            }

            return guildElement.ValueKind switch
            {
                JsonValueKind.Number => guildElement.GetInt64(),
                JsonValueKind.String when long.TryParse(guildElement.GetString(), out long parsed) => parsed,
                _ => null
            };
        }

        private static QuickPlayStateCatalog ReadQuickPlaySettings(JsonElement root, UserSettingsState state)
        {
            mbottrilby.Services.QuickPlayStateCatalog catalog = new QuickPlayStateCatalog();
            if (!TryGetProperty(root, "quickPlay", out System.Text.Json.JsonElement quickPlayElement) || quickPlayElement.ValueKind != JsonValueKind.Object)
            {
                return catalog;
            }

            if (TryGetProperty(quickPlayElement, "servers", out System.Text.Json.JsonElement serversElement) && serversElement.ValueKind == JsonValueKind.Array)
            {
                foreach (System.Text.Json.JsonElement serverElement in serversElement.EnumerateArray())
                {
                    string environmentName = ReadString(serverElement, "environmentName") ?? "dev";
                    long guildId = ReadInt64(serverElement, "guildId");
                    if (guildId <= 0)
                    {
                        continue;
                    }

                    catalog.Servers.Add(new ServerQuickPlaySettings
                    {
                        EnvironmentName = environmentName,
                        GuildId = guildId,
                        QuickPlay = TryGetProperty(serverElement, "quickPlay", out System.Text.Json.JsonElement quickPlayStateElement) &&
                            quickPlayStateElement.ValueKind == JsonValueKind.Object
                            ? ReadLegacyQuickPlay(quickPlayStateElement)
                            : ReadLegacyQuickPlay(serverElement)
                    });
                }

                return catalog;
            }

            string selectedEnvironmentName = state.SelectedEnvironmentName;
            long? selectedGuildId = state.GetSelectedGuildId(selectedEnvironmentName);
            if (selectedGuildId is null or <= 0)
            {
                return catalog;
            }

            catalog.Servers.Add(new ServerQuickPlaySettings
            {
                EnvironmentName = selectedEnvironmentName,
                GuildId = selectedGuildId.Value,
                QuickPlay = ReadLegacyQuickPlay(quickPlayElement)
            });
            return catalog;
        }

        private static QuickPlaySettings ReadLegacyQuickPlay(JsonElement element)
        {
            return new QuickPlaySettings
            {
                Slot1Trigger = ReadString(element, "slot1Trigger"),
                Slot2Trigger = ReadString(element, "slot2Trigger"),
                Slot3Trigger = ReadString(element, "slot3Trigger"),
                Slot4Trigger = ReadString(element, "slot4Trigger"),
                Slot5Trigger = ReadString(element, "slot5Trigger"),
                Slot6Trigger = ReadString(element, "slot6Trigger"),
                Slot7Trigger = ReadString(element, "slot7Trigger"),
                Slot8Trigger = ReadString(element, "slot8Trigger")
            };
        }

        private static TagStateCatalog ReadTagSettings(JsonElement root, UserSettingsState state)
        {
            mbottrilby.Services.TagStateCatalog catalog = new TagStateCatalog();
            if (!TryGetProperty(root, "tags", out System.Text.Json.JsonElement tagsElement) || tagsElement.ValueKind != JsonValueKind.Object)
            {
                return catalog;
            }

            if (TryGetProperty(tagsElement, "servers", out System.Text.Json.JsonElement serversElement) && serversElement.ValueKind == JsonValueKind.Array)
            {
                foreach (System.Text.Json.JsonElement serverElement in serversElement.EnumerateArray())
                {
                    string environmentName = ReadString(serverElement, "environmentName") ?? "dev";
                    long guildId = ReadInt64(serverElement, "guildId");
                    if (guildId <= 0)
                    {
                        continue;
                    }

                    catalog.Servers.Add(new ServerTagState
                    {
                        EnvironmentName = environmentName,
                        GuildId = guildId,
                        Tags = new TagSettings
                        {
                            SelectedTagName = TryGetProperty(serverElement, "tags", out System.Text.Json.JsonElement tagStateElement) &&
                                tagStateElement.ValueKind == JsonValueKind.Object
                                ? ReadString(tagStateElement, "selectedTagName")
                                : ReadString(serverElement, "selectedTagName")
                        }
                    });
                }

                return catalog;
            }

            string selectedEnvironmentName = state.SelectedEnvironmentName;
            long? selectedGuildId = state.GetSelectedGuildId(selectedEnvironmentName);
            if (selectedGuildId is null or <= 0)
            {
                return catalog;
            }

            catalog.Servers.Add(new ServerTagState
            {
                EnvironmentName = selectedEnvironmentName,
                GuildId = selectedGuildId.Value,
                Tags = new TagSettings
                {
                    SelectedTagName = ReadString(tagsElement, "selectedTagName")
                }
            });
            return catalog;
        }

        private static bool TryGetProperty(JsonElement element, string propertyName, out JsonElement propertyValue)
        {
            foreach (System.Text.Json.JsonProperty property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    propertyValue = property.Value;
                    return true;
                }
            }

            propertyValue = default;
            return false;
        }

        private static string? ReadString(JsonElement element, string propertyName)
        {
            return TryGetProperty(element, propertyName, out System.Text.Json.JsonElement value) && value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;
        }

        private static long ReadInt64(JsonElement element, string propertyName)
        {
            if (!TryGetProperty(element, propertyName, out System.Text.Json.JsonElement value))
            {
                return 0;
            }

            return value.ValueKind switch
            {
                JsonValueKind.Number => value.GetInt64(),
                JsonValueKind.String when long.TryParse(value.GetString(), out long parsed) => parsed,
                _ => 0
            };
        }

        private static long? ReadNullableInt64(JsonElement element, string propertyName)
        {
            if (!TryGetProperty(element, propertyName, out System.Text.Json.JsonElement value))
            {
                return null;
            }

            return value.ValueKind switch
            {
                JsonValueKind.Null => null,
                JsonValueKind.Number => value.GetInt64(),
                JsonValueKind.String when long.TryParse(value.GetString(), out long parsed) => parsed,
                _ => null
            };
        }
    }
}
