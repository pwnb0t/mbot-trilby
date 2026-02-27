using System;
using System.Net;
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
        private readonly long _guildId;

        public SidekickApiClientService(string baseUrl, long guildId)
        {
            _guildId = guildId;
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddApi(options =>
            {
                options.AddApiHttpClients(client =>
                {
                    client.BaseAddress = new Uri(baseUrl, UriKind.Absolute);
                });
            });

            _serviceProvider = services.BuildServiceProvider();
            _api = _serviceProvider.GetRequiredService<IDefaultApi>();
        }

        public async Task<string> PlayTriggerAsync(string trigger, CancellationToken cancellationToken = default)
        {
            var request = new PlayTriggerRequest(_guildId, trigger)
            {
                RequestId = Guid.NewGuid().ToString("N")
            };

            var response = await _api.PlayTriggerAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.IsOk)
            {
                return BuildPlaySuccessMessage(response.RawContent);
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

        private static string BuildPlaySuccessMessage(string rawContent)
        {
            if (string.IsNullOrWhiteSpace(rawContent))
            {
                return "Play request succeeded.";
            }

            try
            {
                using var document = JsonDocument.Parse(rawContent);
                var root = document.RootElement;
                var resolvedTrigger = root.TryGetProperty("resolved_trigger", out var resolved)
                    ? resolved.GetString()
                    : null;
                var guildId = root.TryGetProperty("guild_id", out var guildIdElement) &&
                              guildIdElement.TryGetInt64(out var guildIdValue)
                    ? guildIdValue.ToString()
                    : "unknown";

                if (!string.IsNullOrWhiteSpace(resolvedTrigger))
                {
                    return $"Played '{resolvedTrigger}' in guild {guildId}.";
                }

                return $"Play request succeeded in guild {guildId}.";
            }
            catch
            {
                return "Play request succeeded.";
            }
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
    }
}
