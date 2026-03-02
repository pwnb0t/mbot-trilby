using System;
using System.Net;
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

        public async Task<string> PlayClipAsync(string trigger, CancellationToken cancellationToken = default)
        {
            var request = new PlayClipRequest(_guildId, trigger)
            {
                RequestId = Guid.NewGuid().ToString("N")
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
