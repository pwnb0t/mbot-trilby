using System;
using System.IO;
using System.Text.Json;
using ownbotsidekick.Services;
using Xunit;

namespace ownbotsidekick.Tests.Services
{
    public sealed class UserSettingsStateStoreTests : IDisposable
    {
        private readonly string _tempDirectory;

        public UserSettingsStateStoreTests()
        {
            _tempDirectory = Path.Combine(
                Path.GetTempPath(),
                "ownbotsidekick-tests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDirectory);
        }

        [Fact]
        public void Load_Creates_UserSettingsFile_When_None_Exists()
        {
            var store = new UserSettingsStateStore(_tempDirectory);

            var state = store.Load();

            Assert.Null(state.SelectedTagName);
            Assert.True(File.Exists(Path.Combine(_tempDirectory, "user-settings.json")));
            var json = File.ReadAllText(Path.Combine(_tempDirectory, "user-settings.json"));
            Assert.Contains("\"quickPlay\"", json);
            Assert.Contains("\"tags\"", json);
        }

        [Fact]
        public void Load_Migrates_Legacy_QuickPlayFile_Once()
        {
            var legacyPath = Path.Combine(_tempDirectory, "quickplay.json");
            File.WriteAllText(
                legacyPath,
                """
                {
                  "slot1Trigger": "alpha",
                  "slot2Trigger": "beta"
                }
                """);
            var store = new UserSettingsStateStore(_tempDirectory);

            var state = store.Load();

            Assert.Equal("alpha", state.GetTrigger(1));
            Assert.Equal("beta", state.GetTrigger(2));
            Assert.True(File.Exists(Path.Combine(_tempDirectory, "user-settings.json")));
            Assert.False(File.Exists(legacyPath));
        }

        [Fact]
        public void Save_Persists_SelectedTag_And_QuickPlayAssignments()
        {
            var store = new UserSettingsStateStore(_tempDirectory);
            var state = UserSettingsState.CreateEmpty();
            state.SetTrigger(1, "alpha");
            state.SelectedTagName = "test";

            store.Save(state);

            var reloaded = store.Load();
            Assert.Equal("alpha", reloaded.GetTrigger(1));
            Assert.Equal("test", reloaded.SelectedTagName);
        }

        [Fact]
        public void Load_Upgrades_Flat_UserSettingsFile_To_Sectioned_Shape()
        {
            var userSettingsPath = Path.Combine(_tempDirectory, "user-settings.json");
            File.WriteAllText(
                userSettingsPath,
                """
                {
                  "slot1Trigger": "alpha",
                  "selectedTagName": "test"
                }
                """);
            var store = new UserSettingsStateStore(_tempDirectory);

            var state = store.Load();

            Assert.Equal("alpha", state.GetTrigger(1));
            Assert.Equal("test", state.SelectedTagName);

            var rewrittenJson = File.ReadAllText(userSettingsPath);
            Assert.Contains("\"quickPlay\"", rewrittenJson);
            Assert.Contains("\"tags\"", rewrittenJson);
            Assert.Contains("\"selectedTagName\"", rewrittenJson);
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
