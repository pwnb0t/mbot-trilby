using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using mbottrilby.Services;
using Xunit;

namespace mbottrilby.Tests.Services
{
    public sealed class TrilbySupportLogServiceTests : IDisposable
    {
        private readonly string _tempDirectory;

        public TrilbySupportLogServiceTests()
        {
            _tempDirectory = Path.Combine(
                Path.GetTempPath(),
                "mbot-trilby-tests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDirectory);
            Directory.CreateDirectory(Path.Combine(_tempDirectory, "logs"));
        }

        [Fact]
        public void CreateBundle_Includes_Metadata_And_Recent_Log_Files()
        {
            WriteLogFile("overlay.log", "first", DateTime.UtcNow.AddMinutes(-3));
            WriteLogFile("overlay-2.log", "second", DateTime.UtcNow.AddMinutes(-2));
            WriteLogFile("overlay-3.log", "third", DateTime.UtcNow.AddMinutes(-1));

            var service = new TrilbySupportLogService(_tempDirectory);

            var bundle = service.CreateBundle("dev", 174346738478481408, "pwn b0t", 123);

            Assert.Contains("pwn-b0t_174346738478481408_dev.zip", bundle.FileName);

            using var archiveStream = new MemoryStream(Convert.FromBase64String(bundle.ContentBase64));
            using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Read);
            Assert.NotNull(archive.GetEntry("metadata.json"));
            Assert.NotNull(archive.GetEntry("overlay.log"));
            Assert.NotNull(archive.GetEntry("overlay-2.log"));
            Assert.NotNull(archive.GetEntry("overlay-3.log"));

            using var metadataStream = archive.GetEntry("metadata.json")!.Open();
            using var document = JsonDocument.Parse(metadataStream);
            Assert.Equal("dev", document.RootElement.GetProperty("EnvironmentName").GetString());
            Assert.Equal(174346738478481408, document.RootElement.GetProperty("UserId").GetInt64());
            Assert.Equal("pwn b0t", document.RootElement.GetProperty("Username").GetString());
            Assert.Equal(123, document.RootElement.GetProperty("SelectedServerId").GetInt64());
        }

        [Fact]
        public void CreateBundle_Limits_Log_Files_To_Five_Most_Recent()
        {
            for (var index = 0; index < 6; index += 1)
            {
                WriteLogFile(
                    $"overlay-{index}.log",
                    $"log-{index}",
                    DateTime.UtcNow.AddMinutes(-index));
            }

            var service = new TrilbySupportLogService(_tempDirectory);

            var bundle = service.CreateBundle("prod", 1, "tester", null);

            using var archiveStream = new MemoryStream(Convert.FromBase64String(bundle.ContentBase64));
            using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Read);
            var logEntryNames = archive.Entries
                .Select(entry => entry.FullName)
                .Where(name => name.EndsWith(".log", StringComparison.OrdinalIgnoreCase))
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            Assert.Equal(5, logEntryNames.Length);
            Assert.DoesNotContain("overlay-5.log", logEntryNames);
        }

        private void WriteLogFile(string fileName, string content, DateTime lastWriteTimeUtc)
        {
            var path = Path.Combine(_tempDirectory, "logs", fileName);
            File.WriteAllText(path, content);
            File.SetLastWriteTimeUtc(path, lastWriteTimeUtc);
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
