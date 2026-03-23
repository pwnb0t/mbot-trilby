using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace mbottrilby.Services
{
    internal sealed class TrilbyAuthenticationService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public async Task<TrilbySessionSettings> SignInAsync(string baseUrl, CancellationToken cancellationToken = default)
        {
            var callbackPort = LoopbackPortAllocator.GetFreePort();
            var callbackUrl = $"http://127.0.0.1:{callbackPort}/callback/";
            var startUrl = $"{baseUrl.TrimEnd('/')}/v1/auth/discord/start?client_callback_url={Uri.EscapeDataString(callbackUrl)}";

            using var listener = new HttpListener();
            listener.Prefixes.Add($"http://127.0.0.1:{callbackPort}/");
            listener.Start();

            Process.Start(new ProcessStartInfo(startUrl) { UseShellExecute = true });

            var contextTask = listener.GetContextAsync();
            var completedTask = await Task.WhenAny(contextTask, Task.Delay(TimeSpan.FromMinutes(5), cancellationToken));
            if (completedTask != contextTask)
            {
                throw new TimeoutException("Timed out waiting for the browser sign-in callback.");
            }

            var context = await contextTask.ConfigureAwait(false);
            try
            {
                var query = context.Request.QueryString;
                var callback = ParseCallback(query);
                var errorText = GetErrorText(query);
                await WriteLoopbackResponseAsync(
                    context.Response,
                    success: callback is not null,
                    errorText: errorText).ConfigureAwait(false);
                if (callback is null)
                {
                    var errorMessage = errorText ?? "Authentication failed.";
                    throw new InvalidOperationException(errorMessage);
                }

                var sessionSummary = await GetSessionSummaryAsync(baseUrl, callback.AccessToken ?? string.Empty, cancellationToken)
                    .ConfigureAwait(false);
                return callback.ApplySummary(sessionSummary);
            }
            finally
            {
                listener.Stop();
            }
        }

        public async Task<TrilbySessionSettings> RefreshSessionAsync(
            string baseUrl,
            string refreshToken,
            CancellationToken cancellationToken = default)
        {
            using var httpClient = new HttpClient { BaseAddress = new Uri(baseUrl, UriKind.Absolute) };
            using var request = new HttpRequestMessage(HttpMethod.Post, "/v1/auth/refresh")
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new { refresh_token = refreshToken }),
                    Encoding.UTF8,
                    "application/json")
            };
            using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            var responseText = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Token refresh failed: {response.StatusCode} {responseText}");
            }

            var payload = JsonSerializer.Deserialize<RefreshSessionPayload>(responseText, JsonOptions);
            if (payload is null)
            {
                throw new InvalidOperationException("Token refresh failed: empty response.");
            }

            return payload.ToSettings();
        }

        public static bool IsExpired(TrilbySessionSettings session)
        {
            if (string.IsNullOrWhiteSpace(session.ExpiresAtUtc))
            {
                return true;
            }

            if (!DateTimeOffset.TryParse(session.ExpiresAtUtc, out var expiresAt))
            {
                return true;
            }

            return expiresAt <= DateTimeOffset.UtcNow.AddMinutes(1);
        }

        private static TrilbySignInCallback? ParseCallback(NameValueCollection query)
        {
            var status = query["status"];
            if (!string.Equals(status, "ok", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (!long.TryParse(query["user_id"], out var userId))
            {
                throw new InvalidOperationException("Authentication callback did not include a valid user id.");
            }

            return new TrilbySignInCallback
            {
                AccessToken = query["access_token"],
                RefreshToken = query["refresh_token"],
                ExpiresAtUtc = query["expires_at_utc"],
                UserId = userId,
                Username = query["username"]
            };
        }

        private static string? GetErrorText(NameValueCollection query)
        {
            var errorCode = query["error_code"];
            var errorMessage = query["error_message"];
            if (string.IsNullOrWhiteSpace(errorCode) && string.IsNullOrWhiteSpace(errorMessage))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(errorCode))
            {
                return errorMessage;
            }

            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                return errorCode;
            }

            return $"{errorCode}: {errorMessage}";
        }

        private static async Task WriteLoopbackResponseAsync(HttpListenerResponse response, bool success, string? errorText)
        {
            response.StatusCode = success ? 200 : 400;
            response.ContentType = "text/html; charset=utf-8";
            var html = success
                ? "<html><body><h1>Trilby sign-in complete</h1><p>You can close this window now.</p></body></html>"
                : $"<html><body><h1>Trilby sign-in failed</h1><p>{WebUtility.HtmlEncode(errorText ?? "Authentication failed.")}</p><p>You can close this window and try again.</p></body></html>";
            var buffer = Encoding.UTF8.GetBytes(html);
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
            response.OutputStream.Close();
        }

        private static async Task<SessionSummaryPayload> GetSessionSummaryAsync(
            string baseUrl,
            string accessToken,
            CancellationToken cancellationToken)
        {
            using var httpClient = new HttpClient { BaseAddress = new Uri(baseUrl, UriKind.Absolute) };
            using var request = new HttpRequestMessage(HttpMethod.Get, "/v1/auth/me");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            var responseText = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Authentication summary failed: {response.StatusCode} {responseText}");
            }

            var payload = JsonSerializer.Deserialize<SessionSummaryPayload>(responseText, JsonOptions);
            if (payload is null)
            {
                throw new InvalidOperationException("Authentication summary failed: empty response.");
            }

            return payload;
        }

        private static class LoopbackPortAllocator
        {
            public static int GetFreePort()
            {
                var listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
                listener.Start();
                var port = ((IPEndPoint)listener.LocalEndpoint).Port;
                listener.Stop();
                return port;
            }
        }

        private sealed class TrilbySignInCallback
        {
            public string? AccessToken { get; set; }
            public string? RefreshToken { get; set; }
            public string? ExpiresAtUtc { get; set; }
            public long UserId { get; set; }
            public string? Username { get; set; }

            public TrilbySessionSettings ApplySummary(SessionSummaryPayload summary)
            {
                return new TrilbySessionSettings
                {
                    AccessToken = AccessToken,
                    RefreshToken = RefreshToken,
                    ExpiresAtUtc = ExpiresAtUtc,
                    UserId = summary.UserId > 0 ? summary.UserId : UserId,
                    Username = summary.Username ?? Username,
                    Servers = summary.Guilds?
                        .Where(guild => guild.GuildId > 0)
                        .Select(guild => guild.ToSettings())
                        .OrderBy(guild => guild.GuildName, StringComparer.OrdinalIgnoreCase)
                        .ToList()
                        ?? new List<TrilbyGuildSettings>()
                };
            }
        }

        private sealed class RefreshSessionPayload
        {
            [JsonPropertyName("access_token")]
            public string? AccessToken { get; set; }

            [JsonPropertyName("refresh_token")]
            public string? RefreshToken { get; set; }

            [JsonPropertyName("expires_at_utc")]
            public string? ExpiresAtUtc { get; set; }

            [JsonPropertyName("user_id")]
            public long UserId { get; set; }

            [JsonPropertyName("username")]
            public string? Username { get; set; }

            [JsonPropertyName("guilds")]
            public List<GuildPayload>? Guilds { get; set; }

            public TrilbySessionSettings ToSettings()
            {
                return new TrilbySessionSettings
                {
                    AccessToken = AccessToken,
                    RefreshToken = RefreshToken,
                    ExpiresAtUtc = ExpiresAtUtc,
                    UserId = UserId,
                    Username = Username,
                    Servers = Guilds?
                        .Where(guild => guild.GuildId > 0)
                        .Select(guild => guild.ToSettings())
                        .OrderBy(guild => guild.GuildName, StringComparer.OrdinalIgnoreCase)
                        .ToList()
                        ?? new List<TrilbyGuildSettings>()
                };
            }
        }

        private sealed class SessionSummaryPayload
        {
            [JsonPropertyName("user_id")]
            public long UserId { get; set; }

            [JsonPropertyName("username")]
            public string? Username { get; set; }

            [JsonPropertyName("expires_at_utc")]
            public string? ExpiresAtUtc { get; set; }

            [JsonPropertyName("guilds")]
            public List<GuildPayload>? Guilds { get; set; }
        }

        private sealed class GuildPayload
        {
            [JsonPropertyName("guild_id")]
            public long GuildId { get; set; }

            [JsonPropertyName("guild_name")]
            public string? GuildName { get; set; }

            public TrilbyGuildSettings ToSettings()
            {
                return new TrilbyGuildSettings
                {
                    GuildId = GuildId,
                    GuildName = GuildName
                };
            }
        }
    }
}
