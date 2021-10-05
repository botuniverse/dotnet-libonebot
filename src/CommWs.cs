using System;
using System.Threading.Tasks;
using Fleck;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LibOneBot
{
    public class WebSocketComm
    {
        #region Fields

        private readonly ConfigCommWS _config;

        private readonly OneBot _ob;

        private readonly WebSocketServer _server;
        private IWebSocketConnection? _socket;

        #endregion

        #region Constructors

        private WebSocketComm(
            ConfigCommWS config,
            OneBot ob)
        {
            _config = config;
            _ob = ob;

            _server = new($"ws://{_config.Host}:{_config.Port}");
        }

        public static async Task<WebSocketComm> CreateAndStart(
            ConfigCommWS config,
            OneBot ob)
        {
            ob.Logger.LogInformation($"正在启动 WebSocket ws://{config.Host}:{config.Port}");
            WebSocketComm ws = new(config, ob);
            await ws.StartAsync();
            return ws;
        }

        #endregion

        #region Lifecycle

        private async Task StartAsync()
        {
            _server.Start(socket =>
            {
                socket.OnOpen = () =>
                    _ob.Logger.LogInformation($"已启动 WebSocket ws://{_config.Host}:{_config.Port}");
                socket.OnError = exception =>
                    _ob.Logger.LogError(exception, $"WebSocket ws://{_config.Host}:{_config.Port} 遇到错误");
                socket.OnBinary = BinaryMessageHandler;
                socket.OnMessage = TextMessageHandler;
                _socket = socket;
            });

            _ob.Event += OnEvent;
        }

        public async ValueTask DisposeAsync()
        {
            _ob.Event -= OnEvent;
            _server.Dispose();
            _ob.Logger.LogInformation($"已关闭 WebSocket ws://{_config.Host}:{_config.Port}");
        }

        #endregion

        #region Event Handlers

        private void BinaryMessageHandler(byte[] raw)
        {
            throw new NotImplementedException();
        }

        private async void TextMessageHandler(string raw)
        {
            JObject payload;
            string action;

            if (string.IsNullOrWhiteSpace(raw))
            {
                await WriteFailed(RetCode.RetCodeInvalidRequest, "动作请求体读取失败: body 为空");
                return;
            }

            try
            {
                payload = JObject.Parse(raw);
                action = payload["action"]?.ToObject<string>()!;
                if (string.IsNullOrWhiteSpace(action))
                    throw new("action 为空");
            }
            catch (Exception e)
            {
                await WriteFailed(RetCode.RetCodeInvalidRequest, $"动作请求解析失败, 错误: {e.Message}");
                return;
            }

            IResponseIntl response = (_ob.HandleActionRequest(action, payload) as IResponseIntl)!;

            if (_socket is null || !_socket.IsAvailable)
                return;
            await _socket.Send(JsonConvert.SerializeObject(response));
        }

        private async Task WriteFailed(
            int retCode,
            string error)
        {
            _ob.Logger.LogWarning(error);

            if (_socket is null || !_socket.IsAvailable)
                return;
            await _socket.Send(
                JsonConvert.SerializeObject(
                    Response.CreateFailed(retCode, error)));
        }

        private async void OnEvent(object? sender, OneBotEventArgs e)
        {
            if (_socket is null || !_socket.IsAvailable)
                return;

            _ob.Logger.LogDebug($"通过 WebSocket ws://{_config.Host}:{_config.Port} 推送事件 {e.Event.Name}");
            await _socket.Send(JsonConvert.SerializeObject(e.Event));
        }

        #endregion
    }
}
