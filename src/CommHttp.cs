using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LibOneBot
{
    internal class HttpComm : IAsyncDisposable
    {
        #region Fields

        private readonly ConfigCommHTTP _config;

        private readonly OneBot _ob;

        private readonly ConcurrentStack<IEvent> _latestEvents = new();
        private readonly object _latestEventsLock = new();

        #endregion

        #region Constructors

        private HttpComm(
            ConfigCommHTTP config,
            OneBot ob)
        {
            _config = config;
            _ob = ob;
            _webHost = CreateWebHostBuilder().Build();
        }

        public static async Task<HttpComm> CreateAndStart(
            ConfigCommHTTP config,
            OneBot ob)
        {
            ob.Logger.LogInformation($"正在启动 HTTP {config.Host}:{config.Port}");

            HttpComm http = new(config, ob);
            await http.StartAsync();
            return http;
        }

        #endregion

        #region Lifecycle

        private async Task StartAsync()
        {
            try
            {
                await _webHost.StartAsync();
                _ob.Event += OnEvent;
                _ob.Logger.LogInformation($"已启动 HTTP {_config.Host}:{_config.Port}");
            }
            catch (Exception e)
            {
                _ob.Logger.LogError(e, $"HTTP {_config.Host}:{_config.Port} 启动失败");
            }
        }

        public async ValueTask DisposeAsync()
        {
            _ob.Event -= OnEvent;

            await _webHost.StopAsync();
            _webHost.Dispose();
            _ob.Logger.LogInformation($"已关闭 HTTP {_config.Host}:{_config.Port}");
        }

        #endregion

        #region WebHost

        private readonly IWebHost _webHost;

        private IWebHostBuilder CreateWebHostBuilder() =>
            new WebHostBuilder()
                .UseKestrel()
                .UseUrls($"http://{_config.Host}:{_config.Port}")
                .Configure(builder =>
                    builder
                        .UseExceptionHandler(errorApp =>
                            errorApp.Run(ExceptionHandler))
                        .Run(Handler));

        #endregion

        #region Handlers

        private async Task ExceptionHandler(
            HttpContext context)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "text/plain";

            IExceptionHandlerPathFeature exceptionHandlerPathFeature =
                context.Features.Get<IExceptionHandlerPathFeature>();

            if (exceptionHandlerPathFeature.Error is not null)
            {
                _ob.Logger.LogWarning(exceptionHandlerPathFeature.Error,
                    $"HTTP request error: {context.Request.Method} {context.Request.Path}");

                StringBuilder builder = new();
                builder.AppendLine("Error:");
                builder.AppendLine(exceptionHandlerPathFeature.Error.GetType().Name);
                builder.AppendLine("Message:");
                builder.AppendLine(exceptionHandlerPathFeature.Error.Message);

                string e = builder.ToString();
                await context.Response.WriteAsync(e);
            }
            else
            {
                _ob.Logger.LogWarning($"HTTP request error: {context.Request.Method} {context.Request.Path}");
            }
        }

        private async Task Handler(
            HttpContext context)
        {
            _ob.Logger.LogDebug($"HTTP request: {context.Request.Method} {context.Request.Path}");

            // Handle GET
            if (context.Request.Method == "GET" &&
                context.Request.Path == "/")
            {
                context.Response.StatusCode = 200;
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync(
                    $"Hello from LibOneBot v{Assembly.GetExecutingAssembly().GetName().Version}");
                return;
            }

            // Reject Invalid Methods
            if (context.Request.Method != "POST")
            {
                _ob.Logger.LogWarning("动作请求只支持通过 POST 方式请求");

                context.Response.StatusCode = 405;
                return;
            }

            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json; charset=utf-8";
            await context.Response.StartAsync();

            // Reject unsupported content types
            if (!context.Request.ContentType.StartsWith("application/json"))
            {
                await WriteFailed(context, RetCode.RetCodeInvalidRequest, "动作请求体 MIME 类型必须是 application/json");
                return;
            }

            // TODO: Content-Type: application/msgpack
            // Below for Json

            string raw;
            JObject payload;
            string action;

            try
            {
                using (StreamReader reader = new(context.Request.Body))
                    raw = await reader.ReadToEndAsync();

                if (string.IsNullOrWhiteSpace(raw))
                    throw new("body 为空");
            }
            catch (Exception e)
            {
                await WriteFailed(context, RetCode.RetCodeInvalidRequest, $"动作请求体读取失败: {e.Message}");
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
                await WriteFailed(context, RetCode.RetCodeInvalidRequest, $"动作请求解析失败, 错误: {e.Message}");
                return;
            }

            IResponseIntl response = action == Actions.ActionGetLatestEvents
                ? HandleGetLatestEvents(payload.ToObject<Request<object>>()!)
                : (_ob.HandleActionRequest(action, payload) as IResponseIntl)!;

            await context.Response.WriteAsync(JsonConvert.SerializeObject(response));
        }

        private async Task WriteFailed(
            HttpContext context,
            int retCode,
            string error)
        {
            _ob.Logger.LogWarning(error);
            await context.Response.WriteAsync(
                JsonConvert.SerializeObject(
                    Response.CreateFailed(retCode, error)));
        }

        private void OnEvent(object? sender, OneBotEventArgs e) =>
            // ReSharper disable once InconsistentlySynchronizedField
            _latestEvents.Push(e.Event);

        #endregion

        #region Action Handlers

        private Response<List<IEvent>> HandleGetLatestEvents(
            Request<object> request)
        {
            Response<List<IEvent>> response;

            lock (_latestEventsLock)
            {
                response = new()
                {
                    Echo = request.Echo,
                    Data = _latestEvents.ToList()
                };
                _latestEvents.Clear();
            }

            return response;
        }

        #endregion
    }
}
