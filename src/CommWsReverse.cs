using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Websocket.Client;

namespace LibOneBot
{
    public class WebSocketReverseComm
    {
        #region Fields

        private readonly ConfigCommWSReverse _config;

        private readonly OneBot _ob;

        private readonly WebsocketClient _client;

        #endregion

        #region Constructors

        private WebSocketReverseComm(
            ConfigCommWSReverse config,
            OneBot ob)
        {
            _config = config;
            _ob = ob;
            _client = new(new(_config.URL));
            _client.ReconnectTimeout = TimeSpan.FromMilliseconds(_config.ReconnectInterval);
        }

        public static async Task<WebSocketReverseComm> CreateAndStart(
            ConfigCommWSReverse config,
            OneBot ob)
        {
            ob.Logger.LogInformation($"正在启动 WebSocket Reverse {config.URL}");
            WebSocketReverseComm wsReverse = new(config, ob);
            await wsReverse.StartAsync();
            return wsReverse;
        }

        #endregion

        #region Lifecycle

        private async Task StartAsync()
        {
            if (!IsValidWebsocketUri(_config.URL))
            {
                _ob.Logger.LogError($"WebSocket Reverse ({_config.URL}) 启动失败, URL 不合法");
                return;
            }

            _client.MessageReceived.Subscribe(
                Handler,
                exception => _ob.Logger.LogError(exception, $"WebSocket Reverse {_config.URL} 遇到错误"),
                () => { });

            await _client.Start();

            _ob.Event += OnEvent;
            _ob.Logger.LogInformation($"已启动 WebSocket Reverse {_config.URL}");
        }

        public async ValueTask DisposeAsync()
        {
            _ob.Event -= OnEvent;
            _client.Dispose();
            _ob.Logger.LogInformation($"已关闭 WebSocket Reverse {_config.URL}");
        }

        #endregion

        #region Event Handlers

        private async void Handler(ResponseMessage message)
        {
            switch (message.MessageType)
            {
                case WebSocketMessageType.Text:
                {
                    JObject payload;
                    string action;

                    if (string.IsNullOrWhiteSpace(message.Text))
                    {
                        WriteFailed(RetCode.RetCodeInvalidRequest, "动作请求体读取失败: body 为空");
                        return;
                    }

                    try
                    {
                        payload = JObject.Parse(message.Text);
                        action = payload["action"]?.ToObject<string>()!;
                        if (string.IsNullOrWhiteSpace(action))
                            throw new("action 为空");
                    }
                    catch (Exception e)
                    {
                        WriteFailed(RetCode.RetCodeInvalidRequest, $"动作请求解析失败, 错误: {e.Message}");
                        return;
                    }

                    IResponseIntl response = (_ob.HandleActionRequest(action, payload) as IResponseIntl)!;

                    _client.Send(JsonConvert.SerializeObject(response));
                    break;
                }
                case WebSocketMessageType.Binary:
                    throw new NotImplementedException();
                case WebSocketMessageType.Close:
                    _ob.Logger.LogInformation($"已关闭 WebSocket Reverse {_config.URL}");
                    break;
            }
        }

        private void WriteFailed(
            int retCode,
            string error)
        {
            _ob.Logger.LogWarning(error);

            _client.Send(
                JsonConvert.SerializeObject(
                    Response.CreateFailed(retCode, error)));
        }

        private void OnEvent(object? sender, OneBotEventArgs e)
        {
            _ob.Logger.LogDebug($"通过 WebSocket Reverse {_config.URL} 推送事件 {e.Event.Name}");
            _client.Send(JsonConvert.SerializeObject(e.Event));
        }

        #endregion

        #region Utils

        public static bool IsValidWebsocketUri(string uriString) =>
            (uriString.StartsWith("ws:") ||
             uriString.StartsWith("wss:")) &&
            Uri.IsWellFormedUriString(uriString, UriKind.Absolute);

        #endregion
    }
}
