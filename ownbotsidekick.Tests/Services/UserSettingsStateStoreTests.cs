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
            var json = File.ReadAllText(Path.Combine(_tempDirectory, "user-settings.json"));
            Assert.Contains("\"quickPlay\"", json);
            Assert.Contains("\"tags\"", json);
            Assert.Contains("\"selectedTagName\"", json);
            Assert.DoesNotContain("\n  \"selectedTagName\":", json.Replace("\r", string.Empty), StringComparison.Ordinal);
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
