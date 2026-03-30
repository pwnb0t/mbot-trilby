using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
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
            var html = BuildLoopbackResponseHtml(success, errorText);
            var buffer = Encoding.UTF8.GetBytes(html);
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
            response.OutputStream.Close();
        }

        private static string BuildLoopbackResponseHtml(bool success, string? errorText)
        {
            var title = success ? "Trilby sign-in complete" : "Trilby sign-in failed";
            var message = success
                ? "You can safely close this page."
                : WebUtility.HtmlEncode(errorText ?? "Authentication failed.");
            var secondaryMessage = success
                ? string.Empty
                : "You can close this window and try again.";
            var imageMarkup = BuildMbotImageMarkup();
            var autoCloseScript = success
                ? """
                <script>
                window.setTimeout(function () {
                    window.close();
                }, 900);
                </script>
                """
                : string.Empty;

            return $@"
<html>
<head>
    <meta charset=""utf-8"">
    <title>{title}</title>
    <style>
        :root {{
            color-scheme: light;
        }}
        body {{
            margin: 0;
            min-height: 100vh;
            display: grid;
            place-items: center;
            background: radial-gradient(circle at top, #203043 0%, #101923 55%, #0a1219 100%);
            font-family: ""Segoe UI Variable Text"", ""Segoe UI"", sans-serif;
            color: #ecf2f7;
        }}
        .card {{
            width: min(520px, calc(100vw - 40px));
            box-sizing: border-box;
            padding: 28px 28px 24px;
            border-radius: 18px;
            background: rgba(16, 23, 31, 0.88);
            border: 1px solid rgba(120, 187, 255, 0.28);
            box-shadow: 0 24px 60px rgba(0, 0, 0, 0.38);
            text-align: center;
        }}
        .badge {{
            display: inline-block;
            margin-bottom: 14px;
            padding: 6px 10px;
            border-radius: 999px;
            background: rgba(120, 187, 255, 0.14);
            color: #98d0ff;
            font-size: 12px;
            font-weight: 700;
            letter-spacing: 0.08em;
            text-transform: uppercase;
        }}
        .image-wrap {{
            margin: 4px 0 18px;
        }}
        .image-wrap img {{
            width: 112px;
            height: 112px;
            object-fit: cover;
            border-radius: 24px;
            border: 1px solid rgba(120, 187, 255, 0.22);
            box-shadow: 0 10px 30px rgba(0, 0, 0, 0.28);
        }}
        h1 {{
            margin: 0 0 10px;
            font-size: 28px;
            line-height: 1.15;
        }}
        .message {{
            margin: 0;
            color: #d7e2ec;
            font-size: 16px;
            line-height: 1.5;
        }}
        .secondary {{
            margin-top: 10px;
            color: #9eb2c3;
            font-size: 14px;
        }}
    </style>
    {autoCloseScript}
</head>
<body>
    <div class=""card"">
        <div class=""badge"">m'bot Trilby</div>
        {imageMarkup}
        <h1>{title}</h1>
        <p class=""message"">{message}</p>
        {(string.IsNullOrWhiteSpace(secondaryMessage) ? string.Empty : $@"<p class=""secondary"">{secondaryMessage}</p>")}
    </div>
</body>
</html>";
        }

        private static string BuildMbotImageMarkup()
        {
            var imagePath = Path.Combine(AppContext.BaseDirectory, "mbot.jpg");
            if (!File.Exists(imagePath))
            {
                return string.Empty;
            }

            try
            {
                var bytes = File.ReadAllBytes(imagePath);
                var base64 = Convert.ToBase64String(bytes);
                return $"""<div class="image-wrap"><img src="data:image/jpeg;base64,{base64}" alt="m'bot" /></div>""";
            }
            catch
            {
                return string.Empty;
            }
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
                    DefaultGuildId = summary.DefaultGuildId,
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

            [JsonPropertyName("default_guild_id")]
            public long? DefaultGuildId { get; set; }

            public TrilbySessionSettings ToSettings()
            {
                return new TrilbySessionSettings
                {
                    AccessToken = AccessToken,
                    RefreshToken = RefreshToken,
                    ExpiresAtUtc = ExpiresAtUtc,
                    UserId = UserId,
                    Username = Username,
                    DefaultGuildId = DefaultGuildId,
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

            [JsonPropertyName("default_guild_id")]
            public long? DefaultGuildId { get; set; }
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
