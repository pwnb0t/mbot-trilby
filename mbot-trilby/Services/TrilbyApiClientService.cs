using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TrilbyApi.Api;
using TrilbyApi.Client;
using TrilbyApi.Extensions;
using TrilbyApi.Model;

namespace mbottrilby.Services
{
    internal sealed class TrilbyApiClientService : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly IDefaultApi _api;
        private readonly long _guildId;

        public TrilbyApiClientService(string baseUrl, string accessToken, long guildId)
        {
            _ = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
            _guildId = guildId;

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddApi(options =>
            {
                options.AddTokens(new BearerToken(accessToken));
                options.AddApiHttpClients(client =>
                {
                    client.BaseAddress = new Uri(baseUrl, UriKind.Absolute);
                });
            });

            _serviceProvider = services.BuildServiceProvider();
            _api = _serviceProvider.GetRequiredService<IDefaultApi>();
        }

        public async Task<ClipCatalog> ListClipsAsync(CancellationToken cancellationToken = default)
        {
            var response = await _api.ListClipsAsync(_guildId, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (response.IsOk && response.TryOk(out var ok) && ok is not null)
            {
                var clips = ok.Clips
                    .Where(clip => !string.IsNullOrWhiteSpace(clip.Trigger))
                    .Select(clip => new ClipCatalogEntry(
                        clip.Trigger,
                        clip.SourceUrl,
                        clip.StartOffsetText,
                        clip.ClipLengthText,
                        clip.AddedByText))
                    .OrderBy(clip => clip.Trigger, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                return new ClipCatalog(clips, ok.Total);
            }

            if (response.IsUnauthorized && response.TryUnauthorized(out var unauthorized) && unauthorized is not null)
            {
                throw new InvalidOperationException($"List clips failed: {unauthorized.Message}");
            }

            if (response.IsInternalServerError &&
                response.TryInternalServerError(out var internalError) &&
                internalError is not null)
            {
                throw new InvalidOperationException($"List clips failed: {internalError.Message}");
            }

            if (response.IsUnprocessableContent)
            {
                throw new InvalidOperationException("List clips failed: validation error (422).");
            }

            throw new InvalidOperationException(
                $"List clips failed: HTTP {(int)response.StatusCode} ({response.StatusCode})");
        }

        public async Task<TagCatalog> ListTagsAsync(CancellationToken cancellationToken = default)
        {
            var response = await _api.ListTagsAsync(_guildId, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (response.IsOk && response.TryOk(out var ok) && ok is not null)
            {
                var tagNames = ok.Tags
                    .Select(tag => tag.Name)
                    .Where(tagName => !string.IsNullOrWhiteSpace(tagName))
                    .OrderBy(tagName => tagName, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                return new TagCatalog(tagNames, ok.Total);
            }

            if (response.IsUnauthorized && response.TryUnauthorized(out var unauthorized) && unauthorized is not null)
            {
                throw new InvalidOperationException($"List tags failed: {unauthorized.Message}");
            }

            if (response.IsInternalServerError &&
                response.TryInternalServerError(out var internalError) &&
                internalError is not null)
            {
                throw new InvalidOperationException($"List tags failed: {internalError.Message}");
            }

            if (response.IsUnprocessableContent)
            {
                throw new InvalidOperationException("List tags failed: validation error (422).");
            }

            throw new InvalidOperationException(
                $"List tags failed: HTTP {(int)response.StatusCode} ({response.StatusCode})");
        }

        public async Task<TagClipCatalog> ListTagClipsAsync(
            string tagName,
            CancellationToken cancellationToken = default)
        {
            var response = await _api.ListTagClipsAsync(tagName, _guildId, cancellationToken).ConfigureAwait(false);
            if (response.IsOk && response.TryOk(out var ok) && ok is not null)
            {
                var triggers = ok.Clips
                    .Select(clip => clip.Trigger)
                    .Where(trigger => !string.IsNullOrWhiteSpace(trigger))
                    .OrderBy(trigger => trigger, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                return new TagClipCatalog(ok.TagName, triggers, ok.Total);
            }

            if (response.IsUnauthorized && response.TryUnauthorized(out var unauthorized) && unauthorized is not null)
            {
                throw new InvalidOperationException($"List tag clips failed: {unauthorized.Message}");
            }

            if (response.IsNotFound && response.TryNotFound(out var notFound) && notFound is not null)
            {
                throw new InvalidOperationException($"List tag clips failed: {notFound.Message}");
            }

            if (response.IsInternalServerError &&
                response.TryInternalServerError(out var internalError) &&
                internalError is not null)
            {
                throw new InvalidOperationException($"List tag clips failed: {internalError.Message}");
            }

            if (response.IsUnprocessableContent)
            {
                throw new InvalidOperationException("List tag clips failed: validation error (422).");
            }

            throw new InvalidOperationException(
                $"List tag clips failed: HTTP {(int)response.StatusCode} ({response.StatusCode})");
        }

        public async Task AddClipToTagAsync(
            string tagName,
            string clipTrigger,
            CancellationToken cancellationToken = default)
        {
            var request = new AddTagClipBody(clipTrigger);
            var response = await _api.AddTagClipAsync(_guildId, tagName, request, cancellationToken)
                .ConfigureAwait(false);
            if (response.IsOk)
            {
                return;
            }

            if (response.IsUnauthorized && response.TryUnauthorized(out var unauthorized) && unauthorized is not null)
            {
                throw new InvalidOperationException($"Add clip to tag failed: {unauthorized.Message}");
            }

            if (response.IsNotFound && response.TryNotFound(out var notFound) && notFound is not null)
            {
                throw new InvalidOperationException($"Add clip to tag failed: {notFound.Message}");
            }

            if (response.IsInternalServerError &&
                response.TryInternalServerError(out var internalError) &&
                internalError is not null)
            {
                throw new InvalidOperationException($"Add clip to tag failed: {internalError.Message}");
            }

            if (response.IsUnprocessableContent)
            {
                throw new InvalidOperationException("Add clip to tag failed: validation error (422).");
            }

            throw new InvalidOperationException(
                $"Add clip to tag failed: HTTP {(int)response.StatusCode} ({response.StatusCode})");
        }

        public async Task RemoveClipFromTagAsync(
            string tagName,
            string clipTrigger,
            CancellationToken cancellationToken = default)
        {
            var response = await _api.RemoveTagClipAsync(_guildId, tagName, clipTrigger, cancellationToken)
                .ConfigureAwait(false);
            if (response.IsOk)
            {
                return;
            }

            if (response.IsUnauthorized && response.TryUnauthorized(out var unauthorized) && unauthorized is not null)
            {
                throw new InvalidOperationException($"Remove clip from tag failed: {unauthorized.Message}");
            }

            if (response.IsNotFound && response.TryNotFound(out var notFound) && notFound is not null)
            {
                throw new InvalidOperationException($"Remove clip from tag failed: {notFound.Message}");
            }

            if (response.IsInternalServerError &&
                response.TryInternalServerError(out var internalError) &&
                internalError is not null)
            {
                throw new InvalidOperationException($"Remove clip from tag failed: {internalError.Message}");
            }

            if (response.IsUnprocessableContent)
            {
                throw new InvalidOperationException("Remove clip from tag failed: validation error (422).");
            }

            throw new InvalidOperationException(
                $"Remove clip from tag failed: HTTP {(int)response.StatusCode} ({response.StatusCode})");
        }

        public async Task<TopClipStatsCatalog> GetTopClipStatsAsync(
            string days,
            int limit = 10,
            bool includeRandom = false,
            bool guildWide = false,
            CancellationToken cancellationToken = default)
        {
            var response = await _api.GetTopClipStatsAsync(
                _guildId,
                guildWide ? new Option<string>("guild") : new Option<string>("me"),
                days,
                limit,
                includeRandom,
                cancellationToken).ConfigureAwait(false);

            if (response.IsOk && response.TryOk(out var ok) && ok is not null)
            {
                var rows = ok.Rows
                    .Where(row => !string.IsNullOrWhiteSpace(row.Trigger))
                    .Select(row => new TopClipStatsRow(
                        row.Trigger,
                        row.PlayCount,
                        row.LastPlayedAtUtc ?? string.Empty))
                    .ToList();

                return new TopClipStatsCatalog(
                    rows,
                    TopClipStatsResponse.DaysEnumToJsonValue(ok.Days),
                    ok.IncludeRandom,
                    guildWide);
            }

            if (response.IsUnauthorized && response.TryUnauthorized(out var unauthorized) && unauthorized is not null)
            {
                throw new InvalidOperationException($"Top clip stats failed: {unauthorized.Message}");
            }

            if (response.IsBadRequest && response.TryBadRequest(out var badRequest) && badRequest is not null)
            {
                throw new InvalidOperationException($"Top clip stats failed: {badRequest.Message}");
            }

            if (response.IsInternalServerError &&
                response.TryInternalServerError(out var internalError) &&
                internalError is not null)
            {
                throw new InvalidOperationException($"Top clip stats failed: {internalError.Message}");
            }

            if (response.IsUnprocessableContent)
            {
                throw new InvalidOperationException("Top clip stats failed: validation error (422).");
            }

            throw new InvalidOperationException(
                $"Top clip stats failed: HTTP {(int)response.StatusCode} ({response.StatusCode})");
        }

        public async Task<RecentClipStatsCatalog> GetRecentClipStatsAsync(
            int limit = 10,
            bool includeRandom = true,
            bool guildWide = false,
            CancellationToken cancellationToken = default)
        {
            var response = await _api.GetRecentClipStatsAsync(
                _guildId,
                guildWide ? new Option<string>("guild") : new Option<string>("me"),
                limit,
                includeRandom,
                cancellationToken).ConfigureAwait(false);

            if (response.IsOk && response.TryOk(out var ok) && ok is not null)
            {
                var rows = ok.Rows
                    .Where(row => !string.IsNullOrWhiteSpace(row.Trigger))
                    .Select(row => new RecentClipStatsRow(
                        row.Trigger,
                        RecentClipStatsItem.ModeEnumToJsonValue(row.Mode),
                        row.RequesterDisplayName,
                        row.PlayedAtUtc))
                    .ToList();

                return new RecentClipStatsCatalog(rows, ok.IncludeRandom, guildWide);
            }

            if (response.IsUnauthorized && response.TryUnauthorized(out var unauthorized) && unauthorized is not null)
            {
                throw new InvalidOperationException($"Recent clip stats failed: {unauthorized.Message}");
            }

            if (response.IsInternalServerError &&
                response.TryInternalServerError(out var internalError) &&
                internalError is not null)
            {
                throw new InvalidOperationException($"Recent clip stats failed: {internalError.Message}");
            }

            if (response.IsUnprocessableContent)
            {
                throw new InvalidOperationException("Recent clip stats failed: validation error (422).");
            }

            throw new InvalidOperationException(
                $"Recent clip stats failed: HTTP {(int)response.StatusCode} ({response.StatusCode})");
        }

        public async Task<string> PlayClipAsync(string trigger, CancellationToken cancellationToken = default)
        {
            var request = new PlayClipBody(trigger)
            {
                RequestId = $"play:{Guid.NewGuid():N}"
            };

            var response = await _api.PlayClipAsync(_guildId, request, cancellationToken).ConfigureAwait(false);

            if (response.IsOk && response.TryOk(out var ok) && ok is not null)
            {
                return $"Played '{ok.ResolvedTrigger}' in guild {ok.GuildId}.";
            }

            if (response.IsConflict && response.TryConflict(out var conflict) && conflict is not null)
            {
                return $"Conflict ({conflict.Code}): {conflict.Message}";
            }

            if (response.IsBadRequest && response.TryBadRequest(out var badRequest) && badRequest is not null)
            {
                return $"Bad request ({badRequest.Code}): {badRequest.Message}";
            }

            if (response.IsNotFound && response.TryNotFound(out var notFound) && notFound is not null)
            {
                return $"Not found ({notFound.Code}): {notFound.Message}";
            }

            if (response.IsInternalServerError &&
                response.TryInternalServerError(out var internalError) &&
                internalError is not null)
            {
                return $"Server error ({internalError.Code}): {internalError.Message}";
            }

            if (response.IsUnprocessableContent)
            {
                return "Validation error (422). Check the guild path and trigger values.";
            }

            return $"Unexpected status: {(int)response.StatusCode} ({response.StatusCode}).";
        }

        public async Task<string> PlayRandomClipAsync(CancellationToken cancellationToken = default)
        {
            var request = new PlayRandomClipBody()
            {
                RequestId = $"random:{Guid.NewGuid():N}"
            };

            var response = await _api.PlayRandomClipAsync(_guildId, request, cancellationToken).ConfigureAwait(false);

            if (response.IsOk && response.TryOk(out var ok) && ok is not null)
            {
                return $"Played random '{ok.ResolvedTrigger}' in guild {ok.GuildId}.";
            }

            if (response.IsConflict && response.TryConflict(out var conflict) && conflict is not null)
            {
                return $"Conflict ({conflict.Code}): {conflict.Message}";
            }

            if (response.IsNotFound && response.TryNotFound(out var notFound) && notFound is not null)
            {
                return $"Not found ({notFound.Code}): {notFound.Message}";
            }

            if (response.IsInternalServerError &&
                response.TryInternalServerError(out var internalError) &&
                internalError is not null)
            {
                return $"Server error ({internalError.Code}): {internalError.Message}";
            }

            if (response.IsUnprocessableContent)
            {
                return "Validation error (422). Check the guild path value.";
            }

            return $"Unexpected status: {(int)response.StatusCode} ({response.StatusCode}).";
        }

        public async Task<string> StopClipAsync(CancellationToken cancellationToken = default)
        {
            var request = new StopClipBody()
            {
                RequestId = Guid.NewGuid().ToString("N")
            };

            var response = await _api.StopClipAsync(_guildId, request, cancellationToken).ConfigureAwait(false);

            if (response.IsOk && response.TryOk(out var ok) && ok is not null)
            {
                return $"Stopped playback in guild {ok.GuildId}.";
            }

            if (response.IsConflict && response.TryConflict(out var conflict) && conflict is not null)
            {
                return $"Conflict ({conflict.Code}): {conflict.Message}";
            }

            if (response.IsNotFound && response.TryNotFound(out var notFound) && notFound is not null)
            {
                return $"Not found ({notFound.Code}): {notFound.Message}";
            }

            if (response.IsInternalServerError &&
                response.TryInternalServerError(out var internalError) &&
                internalError is not null)
            {
                return $"Server error ({internalError.Code}): {internalError.Message}";
            }

            if (response.IsUnprocessableContent)
            {
                return "Validation error (422). Check the guild path value.";
            }

            return $"Unexpected status: {(int)response.StatusCode} ({response.StatusCode}).";
        }

        public async Task<CurrentIntroState> GetCurrentIntroAsync(CancellationToken cancellationToken = default)
        {
            var response = await _api.GetCurrentIntroAsync(_guildId, cancellationToken).ConfigureAwait(false);
            if (response.IsOk && response.TryOk(out var ok) && ok is not null)
            {
                return new CurrentIntroState(ok.Trigger);
            }

            if (response.IsUnauthorized && response.TryUnauthorized(out var unauthorized) && unauthorized is not null)
            {
                throw new InvalidOperationException($"Get current intro failed: {unauthorized.Message}");
            }

            if (response.IsBadRequest && response.TryBadRequest(out var badRequest) && badRequest is not null)
            {
                throw new InvalidOperationException($"Get current intro failed: {badRequest.Message}");
            }

            if (response.IsNotFound && response.TryNotFound(out var notFound) && notFound is not null)
            {
                throw new InvalidOperationException($"Get current intro failed: {notFound.Message}");
            }

            if (response.IsInternalServerError &&
                response.TryInternalServerError(out var internalError) &&
                internalError is not null)
            {
                throw new InvalidOperationException($"Get current intro failed: {internalError.Message}");
            }

            if (response.IsUnprocessableContent)
            {
                throw new InvalidOperationException("Get current intro failed: validation error (422).");
            }

            throw new InvalidOperationException(
                $"Get current intro failed: unexpected status {(int)response.StatusCode} ({response.StatusCode}).");
        }

        public async Task<CurrentIntroState> SetCurrentIntroAsync(string trigger, CancellationToken cancellationToken = default)
        {
            var request = new SetCurrentIntroBody(trigger);
            var response = await _api.SetCurrentIntroAsync(_guildId, request, cancellationToken).ConfigureAwait(false);

            if (response.IsOk && response.TryOk(out var ok) && ok is not null)
            {
                return new CurrentIntroState(ok.Trigger);
            }

            if (response.IsBadRequest && response.TryBadRequest(out var badRequest) && badRequest is not null)
            {
                throw new InvalidOperationException($"Set current intro failed: {badRequest.Message}");
            }

            if (response.IsNotFound && response.TryNotFound(out var notFound) && notFound is not null)
            {
                throw new InvalidOperationException($"Set current intro failed: {notFound.Message}");
            }

            if (response.IsInternalServerError &&
                response.TryInternalServerError(out var internalError) &&
                internalError is not null)
            {
                throw new InvalidOperationException($"Set current intro failed: {internalError.Message}");
            }

            if (response.IsUnprocessableContent)
            {
                throw new InvalidOperationException("Set current intro failed: validation error (422).");
            }

            throw new InvalidOperationException(
                $"Set current intro failed: unexpected status {(int)response.StatusCode} ({response.StatusCode}).");
        }

        public async Task<string> GetHealthSummaryAsync(CancellationToken cancellationToken = default)
        {
            var response = await _api.GetHealthAsync(cancellationToken).ConfigureAwait(false);
            if (response.IsOk && response.TryOk(out var health) && health is not null)
            {
                return $"API healthy: service={health.Service} version={health.VarVersion}";
            }

            return $"Health call returned status: {(int)response.StatusCode} ({response.StatusCode}).";
        }

        public void Dispose()
        {
            _serviceProvider.Dispose();
        }

        internal sealed class ClipCatalog
        {
            public ClipCatalog(IReadOnlyList<ClipCatalogEntry> clips, int total)
            {
                Clips = clips;
                Total = total;
            }

            public IReadOnlyList<ClipCatalogEntry> Clips { get; }
            public int Total { get; }
        }

        internal sealed class ClipCatalogEntry
        {
            public ClipCatalogEntry(
                string trigger,
                string? sourceUrl,
                string? startOffsetText,
                string? clipLengthText,
                string? addedByText)
            {
                Trigger = trigger;
                SourceUrl = string.IsNullOrWhiteSpace(sourceUrl) ? null : sourceUrl.Trim();
                StartOffsetText = string.IsNullOrWhiteSpace(startOffsetText) ? string.Empty : startOffsetText.Trim();
                ClipLengthText = string.IsNullOrWhiteSpace(clipLengthText) ? string.Empty : clipLengthText.Trim();
                AddedByText = string.IsNullOrWhiteSpace(addedByText)
                    ? string.Empty
                    : addedByText.Trim();
            }

            public string Trigger { get; }
            public string? SourceUrl { get; }
            public string StartOffsetText { get; }
            public string ClipLengthText { get; }
            public string AddedByText { get; }
        }

        internal sealed class TagCatalog
        {
            public TagCatalog(IReadOnlyList<string> tagNames, int total)
            {
                TagNames = tagNames;
                Total = total;
            }

            public IReadOnlyList<string> TagNames { get; }
            public int Total { get; }
        }

        internal sealed class TagClipCatalog
        {
            public TagClipCatalog(string tagName, IReadOnlyList<string> triggers, int total)
            {
                TagName = tagName;
                Triggers = triggers;
                Total = total;
            }

            public string TagName { get; }
            public IReadOnlyList<string> Triggers { get; }
            public int Total { get; }
        }

        internal sealed class TopClipStatsCatalog
        {
            public TopClipStatsCatalog(
                IReadOnlyList<TopClipStatsRow> rows,
                string days,
                bool includeRandom,
                bool guildWide)
            {
                Rows = rows;
                Days = days;
                IncludeRandom = includeRandom;
                GuildWide = guildWide;
            }

            public IReadOnlyList<TopClipStatsRow> Rows { get; }
            public string Days { get; }
            public bool IncludeRandom { get; }
            public bool GuildWide { get; }
        }

        internal sealed class TopClipStatsRow
        {
            public TopClipStatsRow(string trigger, int playCount, string lastPlayedAtUtc)
            {
                Trigger = trigger;
                PlayCount = playCount;
                LastPlayedAtUtc = lastPlayedAtUtc;
            }

            public string Trigger { get; }
            public int PlayCount { get; }
            public string LastPlayedAtUtc { get; }
        }

        internal sealed class RecentClipStatsCatalog
        {
            public RecentClipStatsCatalog(
                IReadOnlyList<RecentClipStatsRow> rows,
                bool includeRandom,
                bool guildWide)
            {
                Rows = rows;
                IncludeRandom = includeRandom;
                GuildWide = guildWide;
            }

            public IReadOnlyList<RecentClipStatsRow> Rows { get; }
            public bool IncludeRandom { get; }
            public bool GuildWide { get; }
        }

        internal sealed class RecentClipStatsRow
        {
            public RecentClipStatsRow(string trigger, string mode, string? requesterDisplayName, string playedAtUtc)
            {
                Trigger = trigger;
                Mode = mode;
                RequesterDisplayName = string.IsNullOrWhiteSpace(requesterDisplayName)
                    ? string.Empty
                    : requesterDisplayName.Trim();
                PlayedAtUtc = playedAtUtc;
            }

            public string Trigger { get; }
            public string Mode { get; }
            public string RequesterDisplayName { get; }
            public string PlayedAtUtc { get; }
        }

        internal sealed class CurrentIntroState
        {
            public CurrentIntroState(string? trigger)
            {
                Trigger = string.IsNullOrWhiteSpace(trigger) ? null : trigger.Trim();
            }

            public string? Trigger { get; }
            public bool IsAssigned => !string.IsNullOrWhiteSpace(Trigger);
        }
    }
}
