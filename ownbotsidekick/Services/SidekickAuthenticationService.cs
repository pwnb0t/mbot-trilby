using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ownbotsidekick.Services
{
    internal sealed class SidekickAuthenticationService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public async Task<SidekickSessionSettings> SignInAsync(string baseUrl, CancellationToken cancellationToken = default)
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
                var session = ParseSession(query);
                var errorText = GetErrorText(query);
                await WriteLoopbackResponseAsync(
                    context.Response,
                    success: session is not null,
                    errorText: errorText).ConfigureAwait(false);
                if (session is null)
                {
                    var errorMessage = errorText ?? "Authentication failed.";
                    throw new InvalidOperationException(errorMessage);
                }

                return session;
            }
            finally
            {
                listener.Stop();
            }
        }

        public async Task<SidekickSessionSettings> RefreshSessionAsync(
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

        public static bool IsExpired(SidekickSessionSettings session)
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

        private static SidekickSessionSettings? ParseSession(NameValueCollection query)
        {
            var status = query["status"];
            if (!string.Equals(status, "ok", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (!long.TryParse(query["user_id"], out var userId) ||
                !long.TryParse(query["guild_id"], out var guildId))
            {
                throw new InvalidOperationException("Authentication callback did not include valid user or guild ids.");
            }

            return new SidekickSessionSettings
            {
                AccessToken = query["access_token"],
                RefreshToken = query["refresh_token"],
                ExpiresAtUtc = query["expires_at_utc"],
                UserId = userId,
                Username = query["username"],
                GuildId = guildId,
                GuildName = query["guild_name"]
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

        private sealed class RefreshSessionPayload
        {
            public string? AccessToken { get; set; }
            public string? RefreshToken { get; set; }
            public string? ExpiresAtUtc { get; set; }
            public long UserId { get; set; }
            public string? Username { get; set; }
            public long GuildId { get; set; }
            public string? GuildName { get; set; }

            public SidekickSessionSettings ToSettings()
            {
                return new SidekickSessionSettings
                {
                    AccessToken = AccessToken,
                    RefreshToken = RefreshToken,
                    ExpiresAtUtc = ExpiresAtUtc,
                    UserId = UserId,
                    Username = Username,
                    GuildId = GuildId,
                    GuildName = GuildName
                };
            }
        }
    }
}
