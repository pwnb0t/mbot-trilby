using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace mbottrilby.Services
{
    internal sealed class TrilbyEventsClientService : IDisposable
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };
        private const string ClipPlayedEventType = "clip_played";
        private const string ClipPlayCountChangedEventType = "clip_play_count_changed";
        private const string ClipCreatedEventType = "clip_created";
        private const string ClipDeletedEventType = "clip_deleted";
        private const string ClipTaggedEventType = "clip_tagged";
        private const string ClipUntaggedEventType = "clip_untagged";
        private const string TagCreatedEventType = "tag_created";
        private const string TagDeletedEventType = "tag_deleted";
        private const string CurrentIntroUpdatedEventType = "current_intro_updated";
        private const string SharedTagSelectedEventType = "shared_tag_selected";
        private const string SharedTagClearedEventType = "shared_tag_cleared";

        private readonly Uri _eventsUri;
        private readonly string _accessToken;
        private readonly Func<TrilbyEvent, Task> _onEventAsync;
        private readonly Action<string>? _log;
        private readonly Action<HttpStatusCode, string>? _onAuthenticationFailure;
        private readonly string? _environmentName;
        private readonly long? _userId;
        private readonly string? _username;
        private readonly string? _expiresAtUtc;
        private readonly string _trilbyVersion;
        private readonly string _tokenFingerprint;
        private CancellationTokenSource? _stopCts;
        private Task? _runTask;

        public TrilbyEventsClientService(
            string baseUrl,
            string accessToken,
            long guildId,
            Func<TrilbyEvent, Task> onEventAsync,
            Action<HttpStatusCode, string>? onAuthenticationFailure = null,
            string? environmentName = null,
            long? userId = null,
            string? username = null,
            string? expiresAtUtc = null,
            Action<string>? log = null)
        {
            _ = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
            _ = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
            _onEventAsync = onEventAsync ?? throw new ArgumentNullException(nameof(onEventAsync));
            _onAuthenticationFailure = onAuthenticationFailure;
            _log = log;
            _accessToken = accessToken;
            _environmentName = environmentName;
            _userId = userId;
            _username = username;
            _expiresAtUtc = expiresAtUtc;
            _trilbyVersion = TrilbyVersionInfo.CurrentVersion;
            _tokenFingerprint = ComputeTokenFingerprint(accessToken);
            _eventsUri = BuildEventsUri(baseUrl, guildId);
        }

        public void Start()
        {
            if (_runTask is not null)
            {
                return;
            }

            _stopCts = new CancellationTokenSource();
            _runTask = RunAsync(_stopCts.Token);
        }

        public async Task StopAsync()
        {
            if (_runTask is null)
            {
                return;
            }

            _stopCts?.Cancel();

            try
            {
                await _runTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _stopCts?.Dispose();
                _stopCts = null;
                _runTask = null;
            }
        }

        public void Dispose()
        {
            StopAsync().GetAwaiter().GetResult();
        }

        internal static TrilbyEvent? ParseEvent(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            var envelope = JsonSerializer.Deserialize<EventEnvelopePayload>(json, JsonOptions);
            if (envelope is null || string.IsNullOrWhiteSpace(envelope.EventType))
            {
                return null;
            }

            return envelope.EventType switch
            {
                var eventType when string.Equals(eventType, ClipPlayedEventType, StringComparison.OrdinalIgnoreCase)
                    => ParseClipPlayedEvent(envelope),
                var eventType when string.Equals(eventType, ClipPlayCountChangedEventType, StringComparison.OrdinalIgnoreCase)
                    => ParseClipPlayCountChangedEvent(envelope),
                var eventType when string.Equals(eventType, ClipCreatedEventType, StringComparison.OrdinalIgnoreCase)
                    => ParseClipCreatedEvent(envelope),
                var eventType when string.Equals(eventType, ClipDeletedEventType, StringComparison.OrdinalIgnoreCase)
                    => ParseClipDeletedEvent(envelope),
                var eventType when string.Equals(eventType, ClipTaggedEventType, StringComparison.OrdinalIgnoreCase)
                    => ParseClipTaggedEvent(envelope),
                var eventType when string.Equals(eventType, ClipUntaggedEventType, StringComparison.OrdinalIgnoreCase)
                    => ParseClipUntaggedEvent(envelope),
                var eventType when string.Equals(eventType, TagCreatedEventType, StringComparison.OrdinalIgnoreCase)
                    => ParseTagCreatedEvent(envelope),
                var eventType when string.Equals(eventType, TagDeletedEventType, StringComparison.OrdinalIgnoreCase)
                    => ParseTagDeletedEvent(envelope),
                var eventType when string.Equals(eventType, CurrentIntroUpdatedEventType, StringComparison.OrdinalIgnoreCase)
                    => ParseCurrentIntroUpdatedEvent(envelope),
                var eventType when string.Equals(eventType, SharedTagSelectedEventType, StringComparison.OrdinalIgnoreCase)
                    => ParseSharedTagSelectedEvent(envelope),
                var eventType when string.Equals(eventType, SharedTagClearedEventType, StringComparison.OrdinalIgnoreCase)
                    => ParseSharedTagClearedEvent(envelope),
                _ => null
            };
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            var attempt = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                ClientWebSocket? websocket = null;
                try
                {
                    websocket = new ClientWebSocket();
                    websocket.Options.SetRequestHeader("Authorization", $"Bearer {_accessToken}");
                    websocket.Options.SetRequestHeader("X-Trilby-Version", _trilbyVersion);
                    _log?.Invoke(
                        $"Connecting to Trilby events. env={_environmentName ?? "<unknown>"} " +
                        $"user_id={_userId?.ToString() ?? "<unknown>"} username={_username ?? "<unknown>"} " +
                        $"guild_id={GetGuildIdFromEventsUri(_eventsUri)} expires_at={_expiresAtUtc ?? "<unknown>"} " +
                        $"trilby_version={_trilbyVersion} " +
                        $"token_fingerprint={_tokenFingerprint} uri={_eventsUri}.");
                    await websocket.ConnectAsync(_eventsUri, cancellationToken).ConfigureAwait(false);
                    attempt = 0;
                    _log?.Invoke(
                        $"Connected to Trilby events. env={_environmentName ?? "<unknown>"} " +
                        $"user_id={_userId?.ToString() ?? "<unknown>"} username={_username ?? "<unknown>"} " +
                        $"guild_id={GetGuildIdFromEventsUri(_eventsUri)}.");
                    await ReceiveLoopAsync(websocket, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (WebSocketException ex) when (!cancellationToken.IsCancellationRequested)
                {
                    var handshakeStatus = GetHandshakeStatusCode(websocket);
                    _log?.Invoke(
                        $"Trilby events connection failed. env={_environmentName ?? "<unknown>"} " +
                        $"user_id={_userId?.ToString() ?? "<unknown>"} username={_username ?? "<unknown>"} " +
                        $"guild_id={GetGuildIdFromEventsUri(_eventsUri)} expires_at={_expiresAtUtc ?? "<unknown>"} " +
                        $"trilby_version={_trilbyVersion} " +
                        $"token_fingerprint={_tokenFingerprint} status={DescribeHandshakeStatus(websocket)} " +
                        $"error={ex.Message}");
                    if (handshakeStatus is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                    {
                        _onAuthenticationFailure?.Invoke(handshakeStatus.Value, ex.Message);
                        break;
                    }
                }
                catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
                {
                    _log?.Invoke(
                        $"Trilby events error. env={_environmentName ?? "<unknown>"} " +
                        $"user_id={_userId?.ToString() ?? "<unknown>"} username={_username ?? "<unknown>"} " +
                        $"guild_id={GetGuildIdFromEventsUri(_eventsUri)} expires_at={_expiresAtUtc ?? "<unknown>"} " +
                        $"trilby_version={_trilbyVersion} " +
                        $"token_fingerprint={_tokenFingerprint} error={ex.Message}");
                }
                finally
                {
                    websocket?.Dispose();
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var delay = TimeSpan.FromSeconds(Math.Min(30, Math.Pow(2, Math.Min(attempt, 4))));
                attempt += 1;
                try
                {
                    await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        private async Task ReceiveLoopAsync(ClientWebSocket websocket, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && websocket.State == WebSocketState.Open)
            {
                var message = await ReceiveTextMessageAsync(websocket, cancellationToken).ConfigureAwait(false);
                if (message is null)
                {
                    return;
                }

                var trilbyEvent = ParseEvent(message);
                if (trilbyEvent is null)
                {
                    continue;
                }

                await _onEventAsync(trilbyEvent).ConfigureAwait(false);
            }
        }

        private static async Task<string?> ReceiveTextMessageAsync(
            ClientWebSocket websocket,
            CancellationToken cancellationToken)
        {
            var buffer = new byte[4096];
            using var stream = new MemoryStream();

            while (true)
            {
                var result = await websocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken)
                    .ConfigureAwait(false);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    return null;
                }

                if (result.Count > 0)
                {
                    stream.Write(buffer, 0, result.Count);
                }

                if (result.EndOfMessage)
                {
                    break;
                }
            }

            return Encoding.UTF8.GetString(stream.ToArray());
        }

        private static ClipPlayedEvent? ParseClipPlayedEvent(EventEnvelopePayload envelope)
        {
            var payload = DeserializePayload<ClipPlayedEventPayload>(envelope.Payload);
            if (payload is null ||
                string.IsNullOrWhiteSpace(payload.Trigger) ||
                string.IsNullOrWhiteSpace(payload.Mode) ||
                string.IsNullOrWhiteSpace(payload.PlayedAtUtc))
            {
                return null;
            }

            return new ClipPlayedEvent(
                envelope.GuildId,
                payload.Trigger,
                payload.Mode,
                payload.RequesterUserId,
                string.IsNullOrWhiteSpace(payload.RequesterDisplayName)
                    ? (payload.RequesterUserId?.ToString() ?? string.Empty)
                    : payload.RequesterDisplayName.Trim(),
                payload.PlayedAtUtc);
        }

        private static ClipTaggedEvent? ParseClipTaggedEvent(EventEnvelopePayload envelope)
        {
            var payload = DeserializePayload<ClipTagMembershipEventPayload>(envelope.Payload);
            if (payload is null ||
                string.IsNullOrWhiteSpace(payload.TagName) ||
                string.IsNullOrWhiteSpace(payload.ClipTrigger))
            {
                return null;
            }

            return new ClipTaggedEvent(envelope.GuildId, payload.TagName, payload.ClipTrigger);
        }

        private static ClipUntaggedEvent? ParseClipUntaggedEvent(EventEnvelopePayload envelope)
        {
            var payload = DeserializePayload<ClipTagMembershipEventPayload>(envelope.Payload);
            if (payload is null ||
                string.IsNullOrWhiteSpace(payload.TagName) ||
                string.IsNullOrWhiteSpace(payload.ClipTrigger))
            {
                return null;
            }

            return new ClipUntaggedEvent(envelope.GuildId, payload.TagName, payload.ClipTrigger);
        }

        private static CurrentIntroUpdatedEvent? ParseCurrentIntroUpdatedEvent(EventEnvelopePayload envelope)
        {
            var payload = DeserializePayload<CurrentIntroUpdatedEventPayload>(envelope.Payload);
            if (payload is null || payload.UserId <= 0)
            {
                return null;
            }

            return new CurrentIntroUpdatedEvent(envelope.GuildId, payload.UserId, payload.Trigger);
        }

        private static ClipPlayCountChangedEvent? ParseClipPlayCountChangedEvent(EventEnvelopePayload envelope)
        {
            var payload = DeserializePayload<ClipPlayedEventPayload>(envelope.Payload);
            if (payload is null ||
                string.IsNullOrWhiteSpace(payload.Trigger) ||
                string.IsNullOrWhiteSpace(payload.Mode) ||
                string.IsNullOrWhiteSpace(payload.PlayedAtUtc))
            {
                return null;
            }

            return new ClipPlayCountChangedEvent(
                envelope.GuildId,
                payload.Trigger,
                payload.Mode,
                payload.RequesterUserId,
                string.IsNullOrWhiteSpace(payload.RequesterDisplayName)
                    ? (payload.RequesterUserId?.ToString() ?? string.Empty)
                    : payload.RequesterDisplayName.Trim(),
                payload.PlayedAtUtc);
        }

        private static ClipCreatedEvent? ParseClipCreatedEvent(EventEnvelopePayload envelope)
        {
            var payload = DeserializePayload<ClipCreatedEventPayload>(envelope.Payload);
            if (payload is null || string.IsNullOrWhiteSpace(payload.Trigger))
            {
                return null;
            }

            return new ClipCreatedEvent(
                envelope.GuildId,
                payload.Trigger.Trim(),
                payload.SourceUrl,
                payload.StartOffsetText,
                payload.ClipLengthText,
                payload.AddedByText,
                payload.TagNames ?? Array.Empty<string>());
        }

        private static ClipDeletedEvent? ParseClipDeletedEvent(EventEnvelopePayload envelope)
        {
            var payload = DeserializePayload<ClipDeletedEventPayload>(envelope.Payload);
            if (payload is null || string.IsNullOrWhiteSpace(payload.Trigger))
            {
                return null;
            }

            return new ClipDeletedEvent(envelope.GuildId, payload.Trigger.Trim());
        }

        private static TagCreatedEvent? ParseTagCreatedEvent(EventEnvelopePayload envelope)
        {
            var payload = DeserializePayload<TagLifecycleEventPayload>(envelope.Payload);
            if (payload is null || string.IsNullOrWhiteSpace(payload.TagName))
            {
                return null;
            }

            return new TagCreatedEvent(envelope.GuildId, payload.TagName.Trim());
        }

        private static TagDeletedEvent? ParseTagDeletedEvent(EventEnvelopePayload envelope)
        {
            var payload = DeserializePayload<TagLifecycleEventPayload>(envelope.Payload);
            if (payload is null || string.IsNullOrWhiteSpace(payload.TagName))
            {
                return null;
            }

            return new TagDeletedEvent(envelope.GuildId, payload.TagName.Trim());
        }

        private static SharedTagSelectedEvent? ParseSharedTagSelectedEvent(EventEnvelopePayload envelope)
        {
            var payload = DeserializePayload<TagLifecycleEventPayload>(envelope.Payload);
            if (payload is null || string.IsNullOrWhiteSpace(payload.TagName))
            {
                return null;
            }

            return new SharedTagSelectedEvent(envelope.GuildId, payload.TagName.Trim());
        }

        private static SharedTagClearedEvent? ParseSharedTagClearedEvent(EventEnvelopePayload envelope)
        {
            var payload = DeserializePayload<TagLifecycleEventPayload>(envelope.Payload);
            return new SharedTagClearedEvent(
                envelope.GuildId,
                string.IsNullOrWhiteSpace(payload?.TagName) ? null : payload.TagName.Trim());
        }

        private static TPayload? DeserializePayload<TPayload>(JsonElement payload)
            where TPayload : class
        {
            if (payload.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            {
                return null;
            }

            return payload.Deserialize<TPayload>(JsonOptions);
        }

        private static Uri BuildEventsUri(string baseUrl, long guildId)
        {
            var baseUri = new Uri(baseUrl, UriKind.Absolute);
            var scheme = string.Equals(baseUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
                ? "wss"
                : "ws";
            var builder = new UriBuilder(baseUri)
            {
                Scheme = scheme,
                Path = "/v1/events",
                Query = $"guild_id={guildId}"
            };
            return builder.Uri;
        }

        private static string ComputeTokenFingerprint(string accessToken)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(accessToken));
            return Convert.ToHexString(hash[..6]).ToLowerInvariant();
        }

        private static string DescribeHandshakeStatus(ClientWebSocket? websocket)
        {
            var statusCode = GetHandshakeStatusCode(websocket);
            return statusCode is null ? "<unknown>" : $"{(int)statusCode.Value} ({statusCode.Value})";
        }

        private static HttpStatusCode? GetHandshakeStatusCode(ClientWebSocket? websocket)
        {
            if (websocket is null)
            {
                return null;
            }

            try
            {
                var statusCode = websocket.HttpStatusCode;
                return statusCode == 0 ? null : statusCode;
            }
            catch
            {
                return null;
            }
        }

        private static string GetGuildIdFromEventsUri(Uri eventsUri)
        {
            var query = eventsUri.Query;
            if (string.IsNullOrWhiteSpace(query))
            {
                return "<unknown>";
            }

            var trimmedQuery = query.TrimStart('?');
            foreach (var part in trimmedQuery.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var pieces = part.Split('=', 2);
                if (pieces.Length == 2 && string.Equals(pieces[0], "guild_id", StringComparison.OrdinalIgnoreCase))
                {
                    return Uri.UnescapeDataString(pieces[1]);
                }
            }

            return "<unknown>";
        }

        internal abstract class TrilbyEvent
        {
            protected TrilbyEvent(string eventType, long guildId)
            {
                EventType = eventType;
                GuildId = guildId;
            }

            public string EventType { get; }
            public long GuildId { get; }
        }

        internal sealed class ClipPlayedEvent : TrilbyEvent
        {
            public ClipPlayedEvent(
                long guildId,
                string trigger,
                string mode,
                long? requesterUserId,
                string requesterDisplayName,
                string playedAtUtc)
                : base(ClipPlayedEventType, guildId)
            {
                Trigger = trigger;
                Mode = mode;
                RequesterUserId = requesterUserId;
                RequesterDisplayName = requesterDisplayName;
                PlayedAtUtc = playedAtUtc;
            }

            public string Trigger { get; }
            public string Mode { get; }
            public long? RequesterUserId { get; }
            public string RequesterDisplayName { get; }
            public string PlayedAtUtc { get; }
            public bool IsRandom => string.Equals(Mode, "random", StringComparison.OrdinalIgnoreCase);
        }

        internal sealed class ClipPlayCountChangedEvent : TrilbyEvent
        {
            public ClipPlayCountChangedEvent(
                long guildId,
                string trigger,
                string mode,
                long? requesterUserId,
                string requesterDisplayName,
                string playedAtUtc)
                : base(ClipPlayCountChangedEventType, guildId)
            {
                Trigger = trigger;
                Mode = mode;
                RequesterUserId = requesterUserId;
                RequesterDisplayName = requesterDisplayName;
                PlayedAtUtc = playedAtUtc;
            }

            public string Trigger { get; }
            public string Mode { get; }
            public long? RequesterUserId { get; }
            public string RequesterDisplayName { get; }
            public string PlayedAtUtc { get; }
            public bool IsRandom => string.Equals(Mode, "random", StringComparison.OrdinalIgnoreCase);
        }

        internal sealed class ClipCreatedEvent : TrilbyEvent
        {
            public ClipCreatedEvent(
                long guildId,
                string trigger,
                string? sourceUrl,
                string? startOffsetText,
                string? clipLengthText,
                string? addedByText,
                IReadOnlyList<string> tagNames)
                : base(ClipCreatedEventType, guildId)
            {
                Trigger = trigger;
                SourceUrl = string.IsNullOrWhiteSpace(sourceUrl) ? null : sourceUrl.Trim();
                StartOffsetText = string.IsNullOrWhiteSpace(startOffsetText) ? string.Empty : startOffsetText.Trim();
                ClipLengthText = string.IsNullOrWhiteSpace(clipLengthText) ? string.Empty : clipLengthText.Trim();
                AddedByText = string.IsNullOrWhiteSpace(addedByText) ? string.Empty : addedByText.Trim();
                TagNames = tagNames;
            }

            public string Trigger { get; }
            public string? SourceUrl { get; }
            public string StartOffsetText { get; }
            public string ClipLengthText { get; }
            public string AddedByText { get; }
            public IReadOnlyList<string> TagNames { get; }
        }

        internal sealed class ClipDeletedEvent : TrilbyEvent
        {
            public ClipDeletedEvent(long guildId, string trigger)
                : base(ClipDeletedEventType, guildId)
            {
                Trigger = trigger;
            }

            public string Trigger { get; }
        }

        internal sealed class ClipTaggedEvent : TrilbyEvent
        {
            public ClipTaggedEvent(long guildId, string tagName, string clipTrigger)
                : base(ClipTaggedEventType, guildId)
            {
                TagName = tagName;
                ClipTrigger = clipTrigger;
            }

            public string TagName { get; }
            public string ClipTrigger { get; }
        }

        internal sealed class ClipUntaggedEvent : TrilbyEvent
        {
            public ClipUntaggedEvent(long guildId, string tagName, string clipTrigger)
                : base(ClipUntaggedEventType, guildId)
            {
                TagName = tagName;
                ClipTrigger = clipTrigger;
            }

            public string TagName { get; }
            public string ClipTrigger { get; }
        }

        internal sealed class CurrentIntroUpdatedEvent : TrilbyEvent
        {
            public CurrentIntroUpdatedEvent(long guildId, long userId, string? trigger)
                : base(CurrentIntroUpdatedEventType, guildId)
            {
                UserId = userId;
                Trigger = string.IsNullOrWhiteSpace(trigger) ? null : trigger.Trim();
            }

            public long UserId { get; }
            public string? Trigger { get; }
        }

        internal sealed class TagCreatedEvent : TrilbyEvent
        {
            public TagCreatedEvent(long guildId, string tagName)
                : base(TagCreatedEventType, guildId)
            {
                TagName = tagName;
            }

            public string TagName { get; }
        }

        internal sealed class TagDeletedEvent : TrilbyEvent
        {
            public TagDeletedEvent(long guildId, string tagName)
                : base(TagDeletedEventType, guildId)
            {
                TagName = tagName;
            }

            public string TagName { get; }
        }

        internal sealed class SharedTagSelectedEvent : TrilbyEvent
        {
            public SharedTagSelectedEvent(long guildId, string tagName)
                : base(SharedTagSelectedEventType, guildId)
            {
                TagName = tagName;
            }

            public string TagName { get; }
        }

        internal sealed class SharedTagClearedEvent : TrilbyEvent
        {
            public SharedTagClearedEvent(long guildId, string? tagName)
                : base(SharedTagClearedEventType, guildId)
            {
                TagName = string.IsNullOrWhiteSpace(tagName) ? null : tagName.Trim();
            }

            public string? TagName { get; }
        }

        private sealed class EventEnvelopePayload
        {
            [JsonPropertyName("event_type")]
            public string? EventType { get; set; }

            [JsonPropertyName("guild_id")]
            public long GuildId { get; set; }

            [JsonPropertyName("occurred_at_utc")]
            public string? OccurredAtUtc { get; set; }

            [JsonPropertyName("payload")]
            public JsonElement Payload { get; set; }
        }

        private sealed class ClipPlayedEventPayload
        {
            [JsonPropertyName("trigger")]
            public string? Trigger { get; set; }

            [JsonPropertyName("mode")]
            public string? Mode { get; set; }

            [JsonPropertyName("requester_user_id")]
            public long? RequesterUserId { get; set; }

            [JsonPropertyName("requester_display_name")]
            public string? RequesterDisplayName { get; set; }

            [JsonPropertyName("played_at_utc")]
            public string? PlayedAtUtc { get; set; }
        }

        private sealed class ClipTagMembershipEventPayload
        {
            [JsonPropertyName("tag_name")]
            public string? TagName { get; set; }

            [JsonPropertyName("clip_trigger")]
            public string? ClipTrigger { get; set; }
        }

        private sealed class CurrentIntroUpdatedEventPayload
        {
            [JsonPropertyName("user_id")]
            public long UserId { get; set; }

            [JsonPropertyName("trigger")]
            public string? Trigger { get; set; }
        }

        private sealed class ClipCreatedEventPayload
        {
            [JsonPropertyName("trigger")]
            public string? Trigger { get; set; }

            [JsonPropertyName("source_url")]
            public string? SourceUrl { get; set; }

            [JsonPropertyName("start_offset_text")]
            public string? StartOffsetText { get; set; }

            [JsonPropertyName("clip_length_text")]
            public string? ClipLengthText { get; set; }

            [JsonPropertyName("added_by_text")]
            public string? AddedByText { get; set; }

            [JsonPropertyName("tag_names")]
            public string[]? TagNames { get; set; }
        }

        private sealed class ClipDeletedEventPayload
        {
            [JsonPropertyName("trigger")]
            public string? Trigger { get; set; }
        }

        private sealed class TagLifecycleEventPayload
        {
            [JsonPropertyName("tag_name")]
            public string? TagName { get; set; }
        }
    }
}
