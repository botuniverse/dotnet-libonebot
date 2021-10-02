using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using MessagePack;
using Newtonsoft.Json;

#pragma warning disable CS8618

namespace LibOneBot
{
    /// <summary>
    ///     表示一个动作请求
    /// </summary>
    [DataContract]
    public class Request<TParams>
    {
        /// <summary>
        ///     动作名称
        /// </summary>
        [DataMember(Name = "action")]
        public string Action { get; set; }

        /// <summary>
        ///     动作参数
        /// </summary>
        [DataMember(Name = "params")]
        public TParams Params { get; set; }

        /// <summary>
        ///     动作请求的 <c>echo</c> 字段
        /// </summary>
        [DataMember(Name = "echo")]
        public object? Echo { get; set; }

        /// <summary>
        ///     解析动作请求
        /// </summary>
        /// <exception cref="InvalidDataException">动作请求解析错误时产生</exception>
        public static Request<TParams> ParseActionRequest(
            byte[] actionBytes,
            bool isBinary)
        {
            if (isBinary)
                try
                {
                    return MessagePackSerializer.Deserialize<Request<TParams>>(actionBytes);
                }
                catch (Exception e)
                {
                    throw new InvalidDataException("动作请求不是一个 MsgPack 映射", e);
                }

            try
            {
                Request<TParams>? result =
                    JsonConvert.DeserializeObject<Request<TParams>>(Encoding.UTF8.GetString(actionBytes));
                if (result is null)
                    throw new NullReferenceException();
                return result;
            }
            catch (Exception e)
            {
                throw new InvalidDataException("动作请求体不是合法的 JSON 对象", e);
            }
        }
    }
}
