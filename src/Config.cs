using System.Collections.Generic;
using System.Runtime.Serialization;

#pragma warning disable CS8618

namespace LibOneBot
{
    /// <summary>
    ///     表示一个 OneBot 配置
    /// </summary>
    [DataContract]
    public class Config
    {
        /// <summary>
        ///     鉴权
        /// </summary>
        [DataMember(Name = "heartbeat")]
        public Heartbeat Heartbeat { get; set; } = new();

        /// <summary>
        ///     心跳
        /// </summary>
        [DataMember(Name = "auth")]
        public Auth Auth { get; set; } = new();

        /// <summary>
        ///     通信方式
        /// </summary>
        [DataMember(Name = "comm_methods")]
        public CommMethods CommMethods { get; set; } = new();
    }

    /// <summary>
    ///     心跳配置
    /// </summary>
    [DataContract]
    public class Heartbeat
    {
        /// <summary>
        ///     是否启用
        /// </summary>
        [DataMember(Name = "enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        ///     心跳间隔
        /// </summary>
        [DataMember(Name = "interval")]
        public int Interval { get; set; }
    }

    /// <summary>
    ///     鉴权配置
    /// </summary>
    [DataContract]
    public class Auth
    {
        /// <summary>
        ///     访问令牌
        /// </summary>
        [DataMember(Name = "access_token")]
        public string? AccessToken { get; set; }
    }

    /// <summary>
    ///     通信方式配置
    /// </summary>
    [DataContract]
    public class CommMethods
    {
        /// <summary>
        ///     HTTP 通信方式
        /// </summary>
        [DataMember(Name = "http")]
        public List<ConfigCommHTTP>? HTTP { get; set; }

        /// <summary>
        ///     HTTP Webhook 通信方式
        /// </summary>
        [DataMember(Name = "http_webhook")]
        public List<ConfigCommHTTPWebhook>? HTTPWebhook { get; set; }

        /// <summary>
        ///     WebSocket 通信方式
        /// </summary>
        [DataMember(Name = "ws")]
        public List<ConfigCommWS>? WS { get; set; }

        /// <summary>
        ///     反向 WebSocket 通信方式
        /// </summary>
        [DataMember(Name = "ws_reverse")]
        public List<ConfigCommWSReverse>? WSReverse { get; set; }
    }

    /// <summary>
    ///     配置一个 HTTP 通信方式
    /// </summary>
    [DataContract]
    public class ConfigCommHTTP
    {
        /// <summary>
        ///     HTTP 服务器监听 IP
        /// </summary>
        [DataMember(Name = "host")]
        public string Host { get; set; }

        /// <summary>
        ///     HTTP 服务器监听端口
        /// </summary>
        [DataMember(Name = "port")]
        public int Port { get; set; }
    }

    /// <summary>
    ///     配置一个 HTTP Webhook 通信方式
    /// </summary>
    [DataContract]
    public class ConfigCommHTTPWebhook
    {
        /// <summary>
        ///     Webhook 上报地址
        /// </summary>
        [DataMember(Name = "url")]
        public string URL { get; set; }

        /// <summary>
        ///     上报请求超时时间
        /// </summary>
        [DataMember(Name = "timeout")]
        public int Timeout { get; set; }

        /// <summary>
        ///     签名密钥
        /// </summary>
        [DataMember(Name = "secret")]
        public string Secret { get; set; }
    }

    /// <summary>
    ///     配置一个 WebSocket 通信方式
    /// </summary>
    [DataContract]
    public class ConfigCommWS
    {
        /// <summary>
        ///     WebSocket 服务器监听 IP
        /// </summary>
        [DataMember(Name = "host")]
        public string Host { get; set; }

        /// <summary>
        ///     WebSocket 服务器监听端口
        /// </summary>
        [DataMember(Name = "port")]
        public int Port { get; set; }
    }

    /// <summary>
    ///     配置一个反向 WebSocket 通信方式
    /// </summary>
    [DataContract]
    public class ConfigCommWSReverse
    {
        /// <summary>
        ///     反向 WebSocket 连接地址
        /// </summary>
        [DataMember(Name = "url")]
        public string URL { get; set; }

        /// <summary>
        ///     反向 WebSocket 重连间隔，单位为毫秒
        /// </summary>
        [DataMember(Name = "reconnect_interval")]
        public int ReconnectInterval { get; set; }
    }
}
