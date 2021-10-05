using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;

#pragma warning disable CS1998,CS4014

namespace LibOneBot
{
    public partial class OneBot
    {
        #region Comm Services

        private readonly List<IAsyncDisposable> _commServices = new();

        internal bool IsRunning
        {
            get
            {
                using (_lifecycleLock.Lock())
                    return _commServices.Any();
            }
        }

        #endregion

        #region Lifecycle

        private readonly AsyncLock _lifecycleLock = new();

        /// <summary>
        ///     运行 OneBot 实例
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         该方法不会阻塞线程。若要阻止程序退出，请使用 <c>new ManualResetEvent(false).WaitOne();</c>。
        ///     </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">OneBot 已经在运行时引发</exception>
        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            using (await _lifecycleLock.LockAsync())
            {
                if (IsRunning)
                    throw new InvalidOperationException("OneBot 已经在运行");

                // HTTP
                if (Config.CommMethods.HTTP is not null)
                    foreach (ConfigCommHTTP config in Config.CommMethods.HTTP)
                        _commServices.Add(await HttpComm.CreateAndStart(config, this));

                // HTTPWebhook
                if (Config.CommMethods.HTTPWebhook is not null)
                    foreach (ConfigCommHTTPWebhook config in Config.CommMethods.HTTPWebhook)
                        _commServices.Add(await HttpWebhookComm.CreateAndStart(config, this));

                // WS
                if (Config.CommMethods.WS is not null)
                    foreach (ConfigCommWS config in Config.CommMethods.WS)
                        _commServices.Add(await WebSocketComm.CreateAndStart(config, this));

                // WSReverse
                if (Config.CommMethods.WSReverse is not null)
                    foreach (ConfigCommWSReverse config in Config.CommMethods.WSReverse)
                        _commServices.Add(await WebSocketReverseComm.CreateAndStart(config, this));
            }
        }

        /// <summary>
        ///     停止 OneBot 实例
        /// </summary>
        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            using (await _lifecycleLock.LockAsync())
                foreach (IAsyncDisposable service in _commServices)
                    await service.DisposeAsync();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            StopAsync();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
