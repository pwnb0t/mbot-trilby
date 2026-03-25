using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace mbottrilby.Services
{
    internal sealed class ClipPlaybackCoordinator
    {
        private readonly TrilbyApiClientService? _trilbyApiClient;

        public ClipPlaybackCoordinator(TrilbyApiClientService? trilbyApiClient)
        {
            _trilbyApiClient = trilbyApiClient;
        }

        public async Task<LoadClipsResult> LoadClipsAsync(string reason)
        {
            var logLines = new List<string>
            {
                $"Loading clips ({reason})..."
            };

            if (_trilbyApiClient is null)
            {
                logLines.Add("Load clips skipped: Trilby API is disabled.");
                return new LoadClipsResult(
                    success: false,
                    clips: Array.Empty<TrilbyApiClientService.ClipCatalogEntry>(),
                    total: 0,
                    logLines: logLines
                );
            }

            try
            {
                var catalog = await _trilbyApiClient.ListClipsAsync();
                logLines.Add($"Loaded {catalog.Clips.Count} clips (API total={catalog.Total}).");
                return new LoadClipsResult(
                    success: true,
                    clips: catalog.Clips,
                    total: catalog.Total,
                    logLines: logLines
                );
            }
            catch (Exception ex)
            {
                logLines.Add($"Load clips failed: {ex.Message}");
                return new LoadClipsResult(
                    success: false,
                    clips: Array.Empty<TrilbyApiClientService.ClipCatalogEntry>(),
                    total: 0,
                    logLines: logLines
                );
            }
        }

        public async Task<LoadTagsResult> LoadTagsAsync(string reason)
        {
            var logLines = new List<string>
            {
                $"Loading tags ({reason})..."
            };

            if (_trilbyApiClient is null)
            {
                logLines.Add("Load tags skipped: Trilby API is disabled.");
                return new LoadTagsResult(
                    success: false,
                    tagNames: Array.Empty<string>(),
                    total: 0,
                    logLines: logLines
                );
            }

            try
            {
                var catalog = await _trilbyApiClient.ListTagsAsync();
                logLines.Add($"Loaded {catalog.TagNames.Count} tags (API total={catalog.Total}).");
                return new LoadTagsResult(
                    success: true,
                    tagNames: catalog.TagNames,
                    total: catalog.Total,
                    logLines: logLines
                );
            }
            catch (Exception ex)
            {
                logLines.Add($"Load tags failed: {ex.Message}");
                return new LoadTagsResult(
                    success: false,
                    tagNames: Array.Empty<string>(),
                    total: 0,
                    logLines: logLines
                );
            }
        }

        public async Task<PlayClipResult> PlayClipAsync(string clipName, string trigger)
        {
            var logLines = new List<string>();

            if (_trilbyApiClient is null)
            {
                logLines.Add($"{clipName} clicked, but Trilby API is disabled.");
                return new PlayClipResult(success: false, shouldHideOverlay: false, logLines: logLines);
            }

            if (string.IsNullOrWhiteSpace(trigger))
            {
                logLines.Add($"{clipName} clicked, but trigger is empty.");
                return new PlayClipResult(success: false, shouldHideOverlay: false, logLines: logLines);
            }

            logLines.Add($"{clipName} clicked -> trigger '{trigger}'");

            try
            {
                var message = await _trilbyApiClient.PlayClipAsync(trigger);
                logLines.Add(message);
                return new PlayClipResult(success: true, shouldHideOverlay: true, logLines: logLines);
            }
            catch (Exception ex)
            {
                logLines.Add($"Play trigger failed: {ex.Message}");
                return new PlayClipResult(success: false, shouldHideOverlay: false, logLines: logLines);
            }
        }

        public async Task<PlayClipResult> PlayRandomAsync()
        {
            var logLines = new List<string>();

            if (_trilbyApiClient is null)
            {
                logLines.Add("Play random clicked, but Trilby API is disabled.");
                return new PlayClipResult(success: false, shouldHideOverlay: false, logLines: logLines);
            }

            logLines.Add("Play Random clicked.");
            try
            {
                var message = await _trilbyApiClient.PlayRandomClipAsync();
                logLines.Add(message);
                return new PlayClipResult(success: true, shouldHideOverlay: true, logLines: logLines);
            }
            catch (Exception ex)
            {
                logLines.Add($"Play trigger failed: {ex.Message}");
                return new PlayClipResult(success: false, shouldHideOverlay: false, logLines: logLines);
            }
        }

        public async Task<StopClipResult> StopClipAsync()
        {
            var logLines = new List<string>();

            if (_trilbyApiClient is null)
            {
                logLines.Add("Stop clicked, but Trilby API is disabled.");
                return new StopClipResult(success: false, logLines: logLines);
            }

            logLines.Add("Stop clicked.");

            try
            {
                var message = await _trilbyApiClient.StopClipAsync();
                logLines.Add(message);
                return new StopClipResult(success: true, logLines: logLines);
            }
            catch (Exception ex)
            {
                logLines.Add($"Stop failed: {ex.Message}");
                return new StopClipResult(success: false, logLines: logLines);
            }
        }

        internal sealed class LoadClipsResult
        {
            public LoadClipsResult(
                bool success,
                IReadOnlyList<TrilbyApiClientService.ClipCatalogEntry> clips,
                int total,
                IReadOnlyList<string> logLines)
            {
                Success = success;
                Clips = clips;
                Total = total;
                LogLines = logLines;
            }

            public bool Success { get; }
            public IReadOnlyList<TrilbyApiClientService.ClipCatalogEntry> Clips { get; }
            public int Total { get; }
            public IReadOnlyList<string> LogLines { get; }
        }

        internal sealed class PlayClipResult
        {
            public PlayClipResult(bool success, bool shouldHideOverlay, IReadOnlyList<string> logLines)
            {
                Success = success;
                ShouldHideOverlay = shouldHideOverlay;
                LogLines = logLines;
            }

            public bool Success { get; }
            public bool ShouldHideOverlay { get; }
            public IReadOnlyList<string> LogLines { get; }
        }

        internal sealed class LoadTagsResult
        {
            public LoadTagsResult(bool success, IReadOnlyList<string> tagNames, int total, IReadOnlyList<string> logLines)
            {
                Success = success;
                TagNames = tagNames;
                Total = total;
                LogLines = logLines;
            }

            public bool Success { get; }
            public IReadOnlyList<string> TagNames { get; }
            public int Total { get; }
            public IReadOnlyList<string> LogLines { get; }
        }

        internal sealed class StopClipResult
        {
            public StopClipResult(bool success, IReadOnlyList<string> logLines)
            {
                Success = success;
                LogLines = logLines;
            }

            public bool Success { get; }
            public IReadOnlyList<string> LogLines { get; }
        }
    }
}
