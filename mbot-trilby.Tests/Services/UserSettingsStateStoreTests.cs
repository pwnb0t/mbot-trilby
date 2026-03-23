using System;
using System.IO;
using System.Text.Json;
using mbottrilby.Services;
using Xunit;

namespace mbottrilby.Tests.Services
{
    public sealed class UserSettingsStateStoreTests : IDisposable
    {
        private readonly string _tempDirectory;

        public UserSettingsStateStoreTests()
        {
            _tempDirectory = Path.Combine(
                Path.GetTempPath(),
                "mbot-trilby-tests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDirectory);
        }

        [Fact]
        public void Load_Creates_UserSettingsFile_When_None_Exists()
        {
            var store = new UserSettingsStateStore(_tempDirectory);

            var state = store.Load();

            Assert.Null(state.GetSelectedTagName("dev", 123));
            Assert.True(File.Exists(Path.Combine(_tempDirectory, "user-settings.json")));
            var json = File.ReadAllText(Path.Combine(_tempDirectory, "user-settings.json"));
            Assert.Contains("\"quickPlay\"", json);
            Assert.Contains("\"tags\"", json);
            Assert.Contains("\"serverSelections\"", json);
        }

        [Fact]
        public void Save_Persists_SelectedTag_And_QuickPlayState_Per_Server()
        {
            var store = new UserSettingsStateStore(_tempDirectory);
            var state = UserSettingsState.CreateEmpty();
            state.SelectedEnvironmentName = "test";
            state.SetSelectedGuildId("test", 123);
            state.SetTrigger("test", 123, 1, "alpha");
            state.SetSelectedTagName("test", 123, "test");
            state.SetSession("test", new TrilbySessionSettings
            {
                AccessToken = "access",
                RefreshToken = "refresh",
                ExpiresAtUtc = "2030-01-01T00:00:00Z",
                UserId = 42,
                Username = "tester",
                Servers =
                {
                    new TrilbyGuildSettings
                    {
                        GuildId = 123,
                        GuildName = "Guild"
                    }
                }
            });

            store.Save(state);

            var reloaded = store.Load();
            Assert.Equal(123, reloaded.GetSelectedGuildId("test"));
            Assert.Equal("alpha", reloaded.GetTrigger("test", 123, 1));
            Assert.Equal("test", reloaded.GetSelectedTagName("test", 123));
            Assert.Equal("test", reloaded.SelectedEnvironmentName);
            Assert.Equal("access", reloaded.GetSession("test")?.AccessToken);
            var json = File.ReadAllText(Path.Combine(_tempDirectory, "user-settings.json"));
            Assert.Contains("\"quickPlay\"", json);
            Assert.Contains("\"tags\"", json);
            Assert.Contains("\"environment\"", json);
            Assert.Contains("\"auth\"", json);
            Assert.Contains("\"serverSelections\"", json);
            Assert.Contains("\"selectedTagName\"", json);
            Assert.Contains("\"servers\"", json);
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_tempDirectory))
                {
                    Directory.Delete(_tempDirectory, recursive: true);
                }
            }
            catch
            {
                // Ignore cleanup failures in temp test directories.
            }
        }
    }
}
