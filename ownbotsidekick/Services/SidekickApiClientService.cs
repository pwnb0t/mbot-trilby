using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SidekickApi.Api;
using SidekickApi.Client;
using SidekickApi.Extensions;
using SidekickApi.Model;

namespace ownbotsidekick.Services
{
    internal sealed class SidekickApiClientService : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly IDefaultApi _api;
        private readonly HttpClient _httpClient;
        private readonly long _guildId;
        private readonly long _requestingUserId;
        private readonly string _apiTokenHeaderValue;

        public SidekickApiClientService(string baseUrl, string apiToken, long guildId, long requestingUserId)
        {
            _guildId = guildId;
            _requestingUserId = requestingUserId;
            _apiTokenHeaderValue = apiToken;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl, UriKind.Absolute)
            };
            _httpClient.DefaultRequestHeaders.Add("X-Sidekick-Token", _apiTokenHeaderValue);

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddApi(options =>
            {
                var apiKeyToken = new ApiKeyToken(
                    apiToken,
                    ClientUtils.ApiKeyHeader.X_Sidekick_Token,
                    prefix: string.Empty
                );
                options.AddTokens(apiKeyToken);
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
            var requestUri = $"/v1/clips?guild_id={Uri.EscapeDataString(_guildId.ToString())}";
            using var response = await _httpClient.GetAsync(requestUri, cancellationToken).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var message = TryReadApiErrorMessage(content) ?? $"HTTP {(int)response.StatusCode} ({response.StatusCode})";
                throw new InvalidOperationException($"List clips failed: {message}");
            }

            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;
            var clipsElement = root.GetProperty("clips");
            var triggers = new List<string>(clipsElement.GetArrayLength());
            foreach (var item in clipsElement.EnumerateArray())
            {
                if (item.TryGetProperty("trigger", out var triggerProperty))
                {
                    var trigger = triggerProperty.GetString();
                    if (!string.IsNullOrWhiteSpace(trigger))
                    {
                        triggers.Add(trigger);
                    }
                }
            }

            var total = root.TryGetProperty("total", out var totalProperty)
                ? totalProperty.GetInt32()
                : triggers.Count;

            return new ClipCatalog(triggers.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList(), total);
        }

        public async Task<string> PlayClipAsync(string trigger, CancellationToken cancellationToken = default)
        {
            var request = new PlayClipRequest(_guildId, trigger)
            {
                RequestId = Guid.NewGuid().ToString("N"),
                RequesterUserId = _requestingUserId > 0 ? _requestingUserId : null
            };

            var response = await _api.PlayClipAsync(request, cancellationToken).ConfigureAwait(false);

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
                return "Validation error (422). Check guild_id and trigger values.";
            }

            return $"Unexpected status: {(int)response.StatusCode} ({response.StatusCode}).";
        }

        public async Task<string> StopClipAsync(CancellationToken cancellationToken = default)
        {
            var request = new StopClipRequest(_guildId)
            {
                RequestId = Guid.NewGuid().ToString("N")
            };

            var response = await _api.StopClipAsync(request, cancellationToken).ConfigureAwait(false);

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
                return "Validation error (422). Check guild_id value.";
            }

            return $"Unexpected status: {(int)response.StatusCode} ({response.StatusCode}).";
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
            _httpClient.Dispose();
            _serviceProvider.Dispose();
        }

        private static string? TryReadApiErrorMessage(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }

            try
            {
                using var document = JsonDocument.Parse(content);
                if (document.RootElement.TryGetProperty("message", out var messageProperty))
                {
                    return messageProperty.GetString();
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        internal sealed class ClipCatalog
        {
            public ClipCatalog(IReadOnlyList<string> triggers, int total)
            {
                Triggers = triggers;
                Total = total;
            }

            public IReadOnlyList<string> Triggers { get; }
            public int Total { get; }
        }
    }
}
