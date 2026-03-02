using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ownbotsidekick.Services
{
    internal sealed class ClipPlaybackCoordinator
    {
        private readonly SidekickApiClientService? _sidekickApiClient;

        public ClipPlaybackCoordinator(SidekickApiClientService? sidekickApiClient)
        {
            _sidekickApiClient = sidekickApiClient;
        }

        public async Task<LoadClipsResult> LoadClipsAsync(string reason)
        {
            var logLines = new List<string>
            {
                $"Loading clips ({reason})..."
            };

            if (_sidekickApiClient is null)
            {
                logLines.Add("Load clips skipped: Sidekick API is disabled.");
                return new LoadClipsResult(
                    success: false,
                    triggers: Array.Empty<string>(),
                    total: 0,
                    logLines: logLines
                );
            }

            try
            {
                var catalog = await _sidekickApiClient.ListClipsAsync();
                logLines.Add($"Loaded {catalog.Triggers.Count} clips (API total={catalog.Total}).");
                return new LoadClipsResult(
                    success: true,
                    triggers: catalog.Triggers,
                    total: catalog.Total,
                    logLines: logLines
                );
            }
            catch (Exception ex)
            {
                logLines.Add($"Load clips failed: {ex.Message}");
                return new LoadClipsResult(
                    success: false,
                    triggers: Array.Empty<string>(),
                    total: 0,
                    logLines: logLines
                );
            }
        }

        public async Task<PlayClipResult> PlayClipAsync(string clipName, string trigger)
        {
            var logLines = new List<string>();

            if (_sidekickApiClient is null)
            {
                logLines.Add($"{clipName} clicked, but Sidekick API is disabled.");
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
                var message = await _sidekickApiClient.PlayClipAsync(trigger);
                logLines.Add(message);
                return new PlayClipResult(success: true, shouldHideOverlay: true, logLines: logLines);
            }
            catch (Exception ex)
            {
                logLines.Add($"Play trigger failed: {ex.Message}");
                return new PlayClipResult(success: false, shouldHideOverlay: false, logLines: logLines);
            }
        }

        internal sealed class LoadClipsResult
        {
            public LoadClipsResult(bool success, IReadOnlyList<string> triggers, int total, IReadOnlyList<string> logLines)
            {
                Success = success;
                Triggers = triggers;
                Total = total;
                LogLines = logLines;
            }

            public bool Success { get; }
            public IReadOnlyList<string> Triggers { get; }
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
    }
}
