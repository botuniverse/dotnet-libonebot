using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HttpClientFactoryLite;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LibOneBot
{
    public class HttpWebhookComm : IAsyncDisposable
    {
        #region Fields

        private readonly ConfigCommHTTPWebhook _config;

        private readonly OneBot _ob;

        private readonly HttpClientFactory _http = new();

        #endregion

        #region Constructors

        private HttpWebhookComm(
            ConfigCommHTTPWebhook config,
            OneBot ob)
        {
            _config = config;
            _ob = ob;
        }

        public static async Task<HttpWebhookComm> CreateAndStart(
            ConfigCommHTTPWebhook config,
            OneBot ob)
        {
            ob.Logger.LogInformation($"正在启动 HTTP Webhook {config.URL}");
            HttpWebhookComm webhook = new(config, ob);
            await webhook.StartAsync();
            return webhook;
        }

        #endregion

        #region Lifecycle

        private async Task StartAsync()
        {
            if (!IsValidHttpUri(_config.URL))
            {
                _ob.Logger.LogError($"HTTP Webhook ({_config.URL}) 启动失败, URL 不合法");
                return;
            }

            Uri baseAddress = new(_config.URL);
            _http.Register(
                "LibOneBot.HttpWebhookComm",
                builder =>
                    builder.ConfigureHttpClient(client =>
                        client.BaseAddress = baseAddress));

            _ob.Event += OnEvent;
            _ob.Logger.LogInformation($"已启动 HTTP Webhook {_config.URL}");
        }

        public async ValueTask DisposeAsync()
        {
            _ob.Event -= OnEvent;
            _ob.Logger.LogInformation($"已关闭 HTTP Webhook {_config.URL}");
        }

        #endregion

        #region Event Handlers

        private async void OnEvent(object? sender, OneBotEventArgs e)
        {
            _ob.Logger.LogDebug($"通过 HTTP Webhook {_config.URL} 推送事件 {e.Event.Name}");

            HttpClient client = _http.CreateClient($"HTTP Webhook Push: {e.Event.Name}");
            await client.PostAsync(
                "/",
                new StringContent(JsonConvert.SerializeObject(e.Event), Encoding.UTF8)
                {
                    Headers =
                    {
                        { "Content-Type", "application/json" }
                    }
                });
        }

        #endregion

        #region Utils

        public static bool IsValidHttpUri(string uriString) =>
            (uriString.StartsWith("http:") ||
             uriString.StartsWith("https:")) &&
            Uri.IsWellFormedUriString(uriString, UriKind.Absolute);

        #endregion
    }
}
