using System.Collections.Generic;

namespace LibOneBot
{
    public static class MessageExtensions
    {
        /// <summary>
        ///     合并消息中连续的可合并消息段 (如连续的纯文本消息段)
        /// </summary>
        public static void Reduce(
            this List<ISegment> m)
        {
            for (int i = 0; i < m.Count - 1; i++)
            {
                int j = i + 1;
                while (j < m.Count && m[i].TryMerge(m[j])) j++;
                if (i + 1 != j) m.RemoveRange(i + 1, j - i - 1);
            }
        }

        /// <summary>
        ///     提取消息中的纯文本消息段, 并合并为字符串
        /// </summary>
        public static string ExtractText(
            this List<ISegment> m)
        {
            string text = "";
            foreach (ISegment segment in m)
                if (segment is Segment<SegmentDataText> textSegment)
                    text += textSegment.Data.Text;
            return text;
        }
    }
}
