using System;
using System.IO;
using System.Net.WebSockets;
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

        private readonly Uri _eventsUri;
        private readonly string _accessToken;
        private readonly Func<ClipPlayedEvent, Task> _onClipPlayedAsync;
        private readonly Action<string>? _log;
        private CancellationTokenSource? _stopCts;
        private Task? _runTask;

        public TrilbyEventsClientService(
            string baseUrl,
            string accessToken,
            long guildId,
            Func<ClipPlayedEvent, Task> onClipPlayedAsync,
            Action<string>? log = null)
        {
            _ = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
            _ = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
            _onClipPlayedAsync = onClipPlayedAsync ?? throw new ArgumentNullException(nameof(onClipPlayedAsync));
            _log = log;
            _accessToken = accessToken;
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

        internal static ClipPlayedEvent? ParseClipPlayedEvent(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            var envelope = JsonSerializer.Deserialize<EventEnvelopePayload>(json, JsonOptions);
            if (envelope is null ||
                !string.Equals(envelope.EventType, "clip_played", StringComparison.OrdinalIgnoreCase) ||
                envelope.Payload is null ||
                string.IsNullOrWhiteSpace(envelope.Payload.Trigger) ||
                string.IsNullOrWhiteSpace(envelope.Payload.Mode) ||
                string.IsNullOrWhiteSpace(envelope.Payload.PlayedAtUtc))
            {
                return null;
            }

            return new ClipPlayedEvent(
                envelope.GuildId,
                envelope.Payload.Trigger,
                envelope.Payload.Mode,
                envelope.Payload.RequesterUserId,
                string.IsNullOrWhiteSpace(envelope.Payload.RequesterDisplayName)
                    ? (envelope.Payload.RequesterUserId?.ToString() ?? string.Empty)
                    : envelope.Payload.RequesterDisplayName.Trim(),
                envelope.Payload.PlayedAtUtc);
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            var attempt = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using var websocket = new ClientWebSocket();
                    websocket.Options.SetRequestHeader("Authorization", $"Bearer {_accessToken}");
                    _log?.Invoke($"Connecting to Trilby events at {_eventsUri}.");
                    await websocket.ConnectAsync(_eventsUri, cancellationToken).ConfigureAwait(false);
                    attempt = 0;
                    _log?.Invoke($"Connected to Trilby events for server {_eventsUri.Query}.");
                    await ReceiveLoopAsync(websocket, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (WebSocketException ex) when (!cancellationToken.IsCancellationRequested)
                {
                    _log?.Invoke($"Trilby events connection failed: {ex.Message}");
                }
                catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
                {
                    _log?.Invoke($"Trilby events error: {ex.Message}");
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

                var clipPlayedEvent = ParseClipPlayedEvent(message);
                if (clipPlayedEvent is null)
                {
                    continue;
                }

                await _onClipPlayedAsync(clipPlayedEvent).ConfigureAwait(false);
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

        internal sealed class ClipPlayedEvent
        {
            public ClipPlayedEvent(
                long guildId,
                string trigger,
                string mode,
                long? requesterUserId,
                string requesterDisplayName,
                string playedAtUtc)
            {
                GuildId = guildId;
                Trigger = trigger;
                Mode = mode;
                RequesterUserId = requesterUserId;
                RequesterDisplayName = requesterDisplayName;
                PlayedAtUtc = playedAtUtc;
            }

            public long GuildId { get; }
            public string Trigger { get; }
            public string Mode { get; }
            public long? RequesterUserId { get; }
            public string RequesterDisplayName { get; }
            public string PlayedAtUtc { get; }
            public bool IsRandom => string.Equals(Mode, "random", StringComparison.OrdinalIgnoreCase);
        }

        private sealed class EventEnvelopePayload
        {
            [JsonPropertyName("event_type")]
            public string? EventType { get; set; }

            [JsonPropertyName("guild_id")]
            public long GuildId { get; set; }

            [JsonPropertyName("payload")]
            public ClipPlayedEventPayload? Payload { get; set; }
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
    }
}
