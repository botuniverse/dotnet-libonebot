using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace LibOneBot
{
    /// <summary>
    ///     表示一个 OneBot 实例
    /// </summary>
    public partial class OneBot : IHostedService, IDisposable
    {
        #region Constructor

        /// <summary>
        ///     创建一个新的 OneBot 实例
        /// </summary>
        /// <param name="platform">OneBot 实现平台名称</param>
        /// <param name="config">OneBot 配置</param>
        public OneBot(
            string platform,
            Config config,
            ILogger? logger = null)
        {
            if (string.IsNullOrWhiteSpace(platform))
                throw new ArgumentNullException(nameof(platform), "必须提供 OneBot 平台名称");

            if (config is null)
                throw new ArgumentNullException(nameof(config), "必须提供 OneBot 配置");

            Platform = platform;
            Config = config;
            Logger = logger ?? NullLogger.Instance;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     OneBot 实现平台名称
        /// </summary>
        /// <remarks>
        ///     <para>应和扩展动作名称、扩展参数等前缀相同, 不能为空</para>
        /// </remarks>
        public string Platform { get; set; }

        /// <summary>
        ///     OneBot 配置
        /// </summary>
        /// <remarks>
        ///     <para>不能为空</para>
        /// </remarks>
        public Config Config { get; set; }

        public ILogger Logger { get; set; }

        #endregion
    }

    public static class OneBotExtensions
    {
        /// <summary>
        ///     注册一个新的 OneBot 实例
        /// </summary>
        /// <param name="platform">OneBot 实现平台名称</param>
        public static IServiceCollection AddOneBot(
            this IServiceCollection serviceCollection,
            string platform) =>
            serviceCollection
                .AddSingleton<OneBot>(
                    provider => new(
                        platform,
                        provider.GetService<Config>()!,
                        provider.GetService<ILogger<OneBot>>()));
    }
}
