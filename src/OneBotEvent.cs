using System;
using Microsoft.Extensions.Logging;

namespace LibOneBot
{
    public class OneBotEventArgs : EventArgs
    {
        public OneBotEventArgs(
            IEvent e)
        {
            Event = e;
        }

        public IEvent Event { get; set; }
    }

    public partial class OneBot
    {
        public event EventHandler<OneBotEventArgs> Event;

        /// <summary>
        ///     向与 OneBot 实例连接的接受端推送一个事件
        /// </summary>
        public void Push(IEvent e)
        {
            if (e is null)
            {
                //throw new ArgumentNullException(nameof(e), "事件为空");
                Logger.LogWarning("事件为空");
                return;
            }

            if (!e.TryFixUp(Platform))
            {
                //throw new InvalidOperationException("事件字段值无效");
                Logger.LogWarning("事件字段值无效");
                return;
            }

            Logger.LogInformation($"事件 {e.Name} 开始推送");

            EventHandler<OneBotEventArgs> raiseEvent = Event;
            if (raiseEvent is not null)
                raiseEvent(this, new(e));
        }
    }
}
