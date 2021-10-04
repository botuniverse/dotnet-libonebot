using System.Runtime.Serialization;

namespace LibOneBot
{
    /// <summary>
    ///     表示 OneBot 标准定义的核心消息段类型
    /// </summary>
    public static class SegTypes
    {
        /// <summary>
        ///     纯文本消息段
        /// </summary>
        public const string SegTypeText = "text";

        /// <summary>
        ///     提及 (即 @) 消息段
        /// </summary>
        public const string SegTypeMention = "mention";
    }

    public interface ISegment
    {
        public string Type { get; set; }
    }

    public static class Segment
    {
        /// <summary>
        ///     构造一个指定类型的消息段
        /// </summary>
        public static Segment<TData> Create<TData>(
            string type,
            TData data) =>
            new(type, data);

        /// <summary>
        ///     构造一个指定类型的扩展消息段
        /// </summary>
        public static Segment<TData> CreateExtended<TData>(
            string prefix,
            string type,
            TData data) =>
            Create($"{prefix}_{type}", data);

        /// <summary>
        ///     构造一个纯文本消息段
        /// </summary>
        public static Segment<SegmentDataText> CreateText(
            string text) =>
            Create(SegTypes.SegTypeText, new SegmentDataText(text));

        public static Segment<SegmentDataMention> CreateMention(
            string userID) =>
            Create(SegTypes.SegTypeMention, new SegmentDataMention(userID));
    }

    [DataContract]
    public class Segment<TData> : ISegment
    {
        #region Constructor

        internal Segment(
            string type,
            TData data)
        {
            Type = type;
            Data = data;
        }

        #endregion

        #region Properties

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "data")]
        public TData Data { get; set; }

        #endregion
    }

    public static class SegmentExtensions
    {
        internal static bool TryMerge(
            this ISegment s,
            ISegment next)
        {
            switch (s.Type)
            {
                case SegTypes.SegTypeText:
                {
                    if (next.Type != SegTypes.SegTypeText)
                        break;
                    if (s is not Segment<SegmentDataText> sText ||
                        next is not Segment<SegmentDataText> nextText)
                        break;

                    sText.Data.Text = $"{sText.Data.Text}{nextText.Data.Text}";
                    break;
                }
            }

            return false;
        }
    }

    [DataContract]
    public class SegmentDataText
    {
        public SegmentDataText(
            string text)
        {
            Text = text;
        }

        [DataMember(Name = "text")]
        public string Text { get; set; }
    }

    [DataContract]
    public class SegmentDataMention
    {
        public SegmentDataMention(
            string userId)
        {
            UserID = userId;
        }

        [DataMember(Name = "user_id")]
        public string UserID { get; set; }
    }
}
