using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace mbottrilby.Services
{
    internal sealed class TrilbySupportLogService
    {
        private const int MaxLogFiles = 5;
        private static readonly Regex UnsafeFileNameChars = new("[^A-Za-z0-9._-]+", RegexOptions.Compiled);

        private readonly string _appDataDirectory;

        public TrilbySupportLogService(string appDataDirectory)
        {
            _appDataDirectory = appDataDirectory ?? throw new ArgumentNullException(nameof(appDataDirectory));
        }

        public PreparedLogBundle CreateBundle(
            string environmentName,
            long userId,
            string? username,
            long? selectedServerId)
        {
            string sanitizedUsername = SanitizeFileNameSegment(username);
            System.DateTime timestamp = DateTime.UtcNow;
            string fileName =
                $"{timestamp:yyyyMMdd-HHmmss}_{sanitizedUsername}_{userId}_{SanitizeFileNameSegment(environmentName)}.zip";

            using System.IO.MemoryStream archiveBuffer = new MemoryStream();
            using (System.IO.Compression.ZipArchive archive = new ZipArchive(archiveBuffer, ZipArchiveMode.Create, leaveOpen: true))
            {
                WriteMetadataEntry(archive, environmentName, userId, username, selectedServerId, timestamp);
                foreach (string logFilePath in GetRecentLogFilePaths())
                {
                    System.IO.Compression.ZipArchiveEntry entry = archive.CreateEntry(Path.GetFileName(logFilePath), CompressionLevel.Optimal);
                    using (System.IO.Stream entryStream = entry.Open())
                    using (System.IO.FileStream fileStream = File.OpenRead(logFilePath))
                    {
                        fileStream.CopyTo(entryStream);
                    }
                }
            }

            return new PreparedLogBundle(
                fileName,
                Convert.ToBase64String(archiveBuffer.ToArray()));
        }

        private IEnumerable<string> GetRecentLogFilePaths()
        {
            string logsDirectory = Path.Combine(_appDataDirectory, "logs");
            if (!Directory.Exists(logsDirectory))
            {
                return Array.Empty<string>();
            }

            return Directory.EnumerateFiles(logsDirectory)
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .Take(MaxLogFiles)
                .ToArray();
        }

        private static void WriteMetadataEntry(
            ZipArchive archive,
            string environmentName,
            long userId,
            string? username,
            long? selectedServerId,
            DateTime timestampUtc)
        {
            mbottrilby.Services.TrilbySupportLogService.LogBundleMetadata metadata = new LogBundleMetadata(
                AppVersion: Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "unknown",
                EnvironmentName: environmentName,
                UserId: userId,
                Username: username ?? string.Empty,
                SelectedServerId: selectedServerId,
                OperatingSystem: RuntimeInformation.OSDescription,
                GeneratedAtUtc: timestampUtc.ToString("O"));
            System.IO.Compression.ZipArchiveEntry metadataEntry = archive.CreateEntry("metadata.json", CompressionLevel.Optimal);
            using (System.IO.Stream metadataStream = metadataEntry.Open())
            using (System.Text.Json.Utf8JsonWriter writer = new Utf8JsonWriter(metadataStream, new JsonWriterOptions { Indented = true }))
            {
                JsonSerializer.Serialize(writer, metadata);
            }
        }

        private static string SanitizeFileNameSegment(string? value)
        {
            string sanitized = UnsafeFileNameChars.Replace((value ?? string.Empty).Trim().ToLowerInvariant(), "-");
            sanitized = sanitized.Trim('-', '.', '_');
            return string.IsNullOrWhiteSpace(sanitized) ? "unknown-user" : sanitized;
        }

        private sealed record LogBundleMetadata(
            string AppVersion,
            string EnvironmentName,
            long UserId,
            string Username,
            long? SelectedServerId,
            string OperatingSystem,
            string GeneratedAtUtc);
    }

    internal sealed class PreparedLogBundle
    {
        public PreparedLogBundle(string fileName, string contentBase64)
        {
            FileName = fileName;
            ContentBase64 = contentBase64;
        }

        public string FileName { get; }

        public string ContentBase64 { get; }
    }
}
