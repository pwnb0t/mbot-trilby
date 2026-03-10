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
        public string? Slot4Trigger { get; set; }
        public string? Slot5Trigger { get; set; }
        public string? Slot6Trigger { get; set; }
        public string? Slot7Trigger { get; set; }
        public string? Slot8Trigger { get; set; }

        public static QuickPlayAssignments CreateEmpty()
        {
            return new QuickPlayAssignments
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
