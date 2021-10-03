using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace LibOneBot
{
    /// <summary>
    ///     表示 OneBot 标准定义的事件类型
    /// </summary>
    public static class EventTypes
    {
        /// <summary>
        ///     消息事件
        /// </summary>
        public const string EventTypeMessage = "message";

        /// <summary>
        ///     通知事件
        /// </summary>
        public const string EventTypeNotice = "notice";

        /// <summary>
        ///     请求事件
        /// </summary>
        public const string EventTypeRequest = "request";

        /// <summary>
        ///     元事件
        /// </summary>
        public const string EventTypeMeta = "meta";
    }

    /// <summary>
    ///     所有事件对象共同实现的接口
    /// </summary>
    public interface IEvent
    {
        public string Name { get; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool TryFixUp(string platform);
    }

    /// <summary>
    ///     包含所有类型事件的共同字段
    /// </summary>
    [DataContract]
    public class Event : IEvent
    {
        #region Properties

        /// <summary>
        ///     OneBot 实现平台名称
        /// </summary>
        /// <remarks>
        ///     <para>无需在构造时传入</para>
        /// </remarks>
        [DataMember(Name = "platform")]
        public string Platform { get; set; }

        /// <summary>
        ///     事件发生时间
        /// </summary>
        /// <remarks>
        ///     <para>可选, 若不传入则使用当前时间</para>
        /// </remarks>
        [DataMember(Name = "time")]
        public long? Time { get; set; }

        /// <summary>
        ///     机器人自身 ID
        /// </summary>
        [DataMember(Name = "self_id")]
        public string SelfID { get; set; }

        /// <summary>
        ///     事件类型
        /// </summary>
        [DataMember(Name = "type")]
        public string Type { get; set; }

        /// <summary>
        ///     事件详细类型
        /// </summary>
        [DataMember(Name = "detail_type")]
        public string DetailType { get; set; }

        #endregion

        #region IEvent Implements

        public string Name => $"{Type}.{DetailType}";

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool TryFixUp(string platform)
        {
            if (string.IsNullOrWhiteSpace(SelfID) ||
                string.IsNullOrWhiteSpace(Type) ||
                string.IsNullOrWhiteSpace(DetailType))
                return false;

            if (Time == 0)
                Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            Platform = platform;

            return true;
        }

        #endregion
    }

    /// <summary>
    ///     表示一个消息事件
    /// </summary>
    [DataContract]
    public class MessageEvent : Event
    {
        /// <summary>
        ///     消息内容
        /// </summary>
        [DataMember(Name = "message")]
        public List<ISegment> Message { get; set; }
    }

    /// <summary>
    ///     表示一个私聊消息事件
    /// </summary>
    [DataContract]
    public class PrivateMessageEvent : MessageEvent
    {
        /// <summary>
        ///     用户 ID
        /// </summary>
        [DataMember(Name = "user_id")]
        public string UserID { get; set; }
    }

    /// <summary>
    ///     表示一个群聊消息事件
    /// </summary>
    [DataContract]
    public class GroupMessageEvent : MessageEvent
    {
        /// <summary>
        ///     用户 ID
        /// </summary>
        [DataMember(Name = "user_id")]
        public string UserID { get; set; }

        /// <summary>
        ///     群 ID
        /// </summary>
        [DataMember(Name = "group_id")]
        public string GroupID { get; set; }
    }

    /// <summary>
    ///     表示一个通知事件
    /// </summary>
    [DataContract]
    public class NoticeEvent : Event
    {
    }

    /// <summary>
    ///     表示一个请求事件
    /// </summary>
    [DataContract]
    public class RequestEvent : Event
    {
    }

    /// <summary>
    ///     表示一个元事件
    /// </summary>
    [DataContract]
    public class MetaEvent : Event
    {
    }
}
