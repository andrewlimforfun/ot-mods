using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BepInEx.Logging;
using Fomo;
using Alpha.Core.Util;
using Newtonsoft.Json.Linq;

namespace FomoTelegram
{
    /// <summary>
    /// Owns a plain <see cref="HttpClient"/> and talks to the Telegram Bot HTTP API directly,
    /// avoiding the <c>Telegram.Bot</c> library which relies on <c>System.Text.Json</c>
    /// and fails on Mono with "VTable setup of type Utf8JsonWriter failed".
    /// <list type="bullet">
    ///   <item>A thread-safe, rate-limited outbound queue (game => Telegram).</item>
    ///   <item>A long-poll receiver loop that injects Telegram replies back into the game as chat messages (Telegram => game).</item>
    /// </list>
    /// </summary>
    public sealed class FomoTelegramManager : IDisposable
    {
        // -- Sentinel values shown in the config file before the user fills them in --
        public const string PlaceholderApiKey = "YOUR_BOT_TOKEN_HERE";
        public const string PlaceholderChatId = "YOUR_CHAT_ID_HERE";

        /// <summary>Minimum delay between outgoing Telegram messages (≈ 25 msgs/s headroom).</summary>
        private static readonly TimeSpan MinSendInterval = TimeSpan.FromMilliseconds(50);

        private readonly ManualLogSource _log =
            BepInEx.Logging.Logger.CreateLogSource($"{FomoTelegramPlugin.ModName}.Manager");

        private readonly HttpClient _http = new HttpClient { Timeout = TimeSpan.FromSeconds(40) };
        private readonly string _apiBase;
        private readonly string _chatId;
        private readonly long _chatIdLong;

        // Single-consumer send queue - keeps message ordering and avoids hammering the API
        private readonly ConcurrentQueue<string> _sendQueue = new ConcurrentQueue<string>();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly Task _workerTask;

        public bool IsReady { get; private set; }

        public FomoTelegramManager(string apiKey, string chatId)
        {
            // Mono/Unity may default to TLS 1.0 or have cert-store gaps; force TLS 1.2.
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            _apiBase = $"https://api.telegram.org/bot{apiKey}";
            _chatId = chatId;
            long.TryParse(chatId, out _chatIdLong);

            // Start the outbound worker immediately (it will idle until IsReady = true)
            _workerTask = Task.Run(() => WorkerLoopAsync(_cts.Token));

            // Validate credentials and, on success, start the inbound receiver
            _ = ValidateAndStartReceiverAsync();
        }

        // -- Public API --------------------------------------------------------

        /// <summary>Enqueues a plain-text message to be forwarded to the configured Telegram chat.</summary>
        public void Enqueue(string text)
        {
            if (!IsReady) return;
            _sendQueue.Enqueue(text);
        }

        /// <summary>Sends a message directly, bypassing the queue. Prefer <see cref="Enqueue"/> for normal use.</summary>
        public async Task SendDirectAsync(string text)
        {
            try
            {
                // Serialize with JsonConvert (stable across all Newtonsoft versions).
                // parse_mode is intentionally omitted; "None" is not a valid Telegram value.
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(new { chat_id = _chatId, text });
                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                var resp = await _http.PostAsync($"{_apiBase}/sendMessage", content);
                if (!resp.IsSuccessStatusCode)
                {
                    string respBody = await resp.Content.ReadAsStringAsync();
                    _log.LogWarning($"Telegram send failed ({resp.StatusCode}): {respBody}");
                }
            }
            catch (Exception ex)
            {
                _log.LogWarning($"Telegram send failed: {ex.Message}");
            }
        }

        // -- Internal helpers --------------------------------------------------

        private async Task ValidateAndStartReceiverAsync()
        {
            try
            {
                _log.LogInfo($"Validating Telegram credentials against {_apiBase}/getMe ...");
                var resp = await _http.GetAsync($"{_apiBase}/getMe");
                string raw = await resp.Content.ReadAsStringAsync();
                _log.LogDebug($"getMe HTTP {(int)resp.StatusCode}: {raw}");

                var json = JObject.Parse(raw);

                if (json["ok"]?.Value<bool>() != true)
                {
                    // Telegram returned a well-formed error - the API key itself is likely wrong.
                    string description = json["description"]?.Value<string>() ?? raw;
                    int errorCode = json["error_code"]?.Value<int>() ?? 0;
                    _log.LogError($"Telegram validation failed (HTTP {(int)resp.StatusCode}, error_code={errorCode}): {description}. Check your API key in the config.");
                    return;
                }

                string username = json["result"]?["username"]?.Value<string>() ?? "unknown";
                long id = json["result"]?["id"]?.Value<long>() ?? 0;
                _log.LogInfo($"Connected to Telegram as @{username} (id={id}).");
                IsReady = true;
            }
            catch (HttpRequestException ex)
            {
                // Network-level failure (DNS, TLS, firewall, etc.) - not an API key problem.
                _log.LogError($"Telegram validation failed - network error reaching api.telegram.org. This is NOT an API key issue.");
                _log.LogError($"  HttpRequestException: {ex.Message}");
                if (ex.InnerException != null)
                    _log.LogError($"  Caused by ({ex.InnerException.GetType().Name}): {ex.InnerException.Message}");
                if (ex.InnerException?.InnerException != null)
                    _log.LogError($"  Root cause ({ex.InnerException.InnerException.GetType().Name}): {ex.InnerException.InnerException.Message}");
                _log.LogError("  Possible causes: no internet, firewall blocking api.telegram.org, TLS negotiation failure on Mono, or missing root certificates.");
                return;
            }
            catch (Exception ex)
            {
                _log.LogError($"Telegram validation failed - unexpected {ex.GetType().Name}: {ex.Message}");
                if (ex.InnerException != null)
                    _log.LogError($"  Caused by ({ex.InnerException.GetType().Name}): {ex.InnerException.Message}");
                return;
            }

            // Start the long-poll receiver in a background task
            _ = Task.Run(() => ReceiveLoopAsync(_cts.Token));
            _log.LogInfo("Telegram inbound receiver started.");
        }

        /// <summary>
        /// Long-polls <c>getUpdates</c> continuously, filters text messages from the
        /// watched chat, and injects them into the game on the main thread.
        /// </summary>
        private async Task ReceiveLoopAsync(CancellationToken ct)
        {
            long offset = 0;

            // Drop stale messages that arrived before the bot connected
            offset = await DropPendingUpdatesAsync(ct);

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    string url = $"{_apiBase}/getUpdates?offset={offset}&timeout=30&allowed_updates=%5B%22message%22%5D";
                    var resp = await _http.GetAsync(url, ct);
                    string raw = await resp.Content.ReadAsStringAsync();
                    var json = JObject.Parse(raw);

                    if (json["ok"]?.Value<bool>() != true) continue;

                    var updates = json["result"] as JArray;
                    if (updates == null) continue;

                    foreach (JToken update in updates)
                    {
                        long updateId = update["update_id"]?.Value<long>() ?? 0;
                        offset = updateId + 1;

                        JToken? msg = update["message"];
                        if (msg == null) continue;

                        string? text = msg["text"]?.Value<string>();
                        if (string.IsNullOrEmpty(text)) continue;

                        long chatId = msg["chat"]?["id"]?.Value<long>() ?? 0;
                        if (_chatIdLong != 0 && chatId != _chatIdLong) continue;

                        bool isBot = msg["from"]?["is_bot"]?.Value<bool>() ?? false;
                        if (isBot) continue;
                        
                        // execute commands or send chat
                        FomoPlugin.DispatchIncomingText(text);
                    }
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    _log.LogWarning($"Telegram polling error: {ex.Message}");
                    // Back-off before retrying so we don't spam the API on transient failures
                    try { await Task.Delay(5_000, ct); } catch (OperationCanceledException) { break; }
                }
            }
        }

        /// <summary>Fetches all pending updates with <c>timeout=0</c> and returns the next offset, discarding them.</summary>
        private async Task<long> DropPendingUpdatesAsync(CancellationToken ct)
        {
            try
            {
                string url = $"{_apiBase}/getUpdates?timeout=0";
                var resp = await _http.GetAsync(url, ct);
                string raw = await resp.Content.ReadAsStringAsync();
                var json = JObject.Parse(raw);

                if (json["result"] is JArray updates && updates.Count > 0)
                {
                    long lastId = updates[updates.Count - 1]["update_id"]?.Value<long>() ?? 0;
                    return lastId + 1;
                }
            }
            catch { /* non-fatal - just start from offset 0 */ }

            return 0;
        }

        /// <summary>
        /// Background worker that drains <see cref="_sendQueue"/> at a controlled rate.
        /// This keeps the main thread unblocked and respects Telegram's rate limits.
        /// </summary>
        private async Task WorkerLoopAsync(CancellationToken ct)
        {
            _log.LogInfo("Telegram send-worker started.");

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    if (_sendQueue.TryDequeue(out string? text))
                    {
                        await SendDirectAsync(text);
                        // Throttle to stay well below Telegram's 30 msg/s global limit
                        try { await Task.Delay(MinSendInterval, ct); } catch (OperationCanceledException) { break; }
                    }
                    else
                    {
                        // Nothing to send - idle wait before checking again
                        try { await Task.Delay(100, ct); } catch (OperationCanceledException) { break; }
                    }
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    // Log and keep going - a single crash must not kill the send worker
                    _log.LogError($"Unexpected error in Telegram send-worker: {ex}");
                    try { await Task.Delay(1_000, ct); } catch (OperationCanceledException) { break; }
                }
            }

            _log.LogInfo("Telegram send-worker stopped.");
        }

        // -- IDisposable -------------------------------------------------------

        public void Dispose()
        {
            _cts.Cancel();
            try { _workerTask.Wait(TimeSpan.FromSeconds(2)); } catch { /* shutting down */ }
            _cts.Dispose();
            _http.Dispose();
        }
    }
}
