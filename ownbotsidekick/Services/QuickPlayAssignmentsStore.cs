using System;
using System.IO;
using System.Text.Json;

namespace ownbotsidekick.Services
{
    internal sealed class QuickPlayAssignments
    {
        public string? Slot1Trigger { get; set; }
        public string? Slot2Trigger { get; set; }
        public string? Slot3Trigger { get; set; }

        public static QuickPlayAssignments CreateEmpty()
        {
            return new QuickPlayAssignments
            {
                Slot1Trigger = null,
                Slot2Trigger = null,
                Slot3Trigger = null
            };
        }
    }

    internal sealed class QuickPlayAssignmentsStore
    {
        private readonly string _filePath;

        public QuickPlayAssignmentsStore(string baseDirectoryPath)
        {
            Directory.CreateDirectory(baseDirectoryPath);
            _filePath = Path.Combine(baseDirectoryPath, "quickplay.json");
        }

        public QuickPlayAssignments Load()
        {
            var fallback = QuickPlayAssignments.CreateEmpty();
            if (!File.Exists(_filePath))
            {
                Save(fallback);
                return fallback;
            }

            try
            {
                var json = File.ReadAllText(_filePath);
                var assignments = JsonSerializer.Deserialize<QuickPlayAssignments>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }
                );
                return assignments ?? fallback;
            }
            catch
            {
                return fallback;
            }
        }

        public void Save(QuickPlayAssignments assignments)
        {
            var json = JsonSerializer.Serialize(
                assignments,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                }
            );
            File.WriteAllText(_filePath, json);
        }
    }
}
