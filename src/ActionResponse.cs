using System;
using System.Runtime.Serialization;
using System.Text;
using MessagePack;
using Newtonsoft.Json;

#pragma warning disable CS8618

namespace LibOneBot
{
    public static class ActionStatus
    {
        public const string StatusOK = "ok";

        public const string StatusFailed = "failed";
    }

    /// <summary>
    ///     表示动作响应返回码
    /// </summary>
    /// <remarks>
    ///     <para>处于对可扩展性的考虑，此处不使用 <see langword="enum" />。</para>
    /// </remarks>
    public static class RetCode
    {
        /// <summary>
        ///     成功
        /// </summary>
        public const int RetCodeOK = 0;

        #region 动作处理器错误

        /// <summary>
        ///     动作处理器实现错误
        /// </summary>
        public const int RetCodeBadActionHandler = 13001;

        #endregion

        #region 动作请求错误

        /// <summary>
        ///     动作请求无效 (格式错误, 必要字段缺失或字段类型错误)
        /// </summary>
        public const int RetCodeInvalidRequest = 11001;

        /// <summary>
        ///     动作请求不存在 (OneBot 实现没有实现该动作)
        /// </summary>
        public const int RetCodeActionNotFound = 11002;

        /// <summary>
        ///     动作请求参数错误 (参数缺失或参数类型错误)
        /// </summary>
        public const int RetCodeParamError = 11003;

        #endregion

        #region 动作执行错误

        /// <summary>
        ///     数据库错误
        /// </summary>
        public const int RetCodeDatabaseError = 12100;

        /// <summary>
        ///     文件系统错误
        /// </summary>
        public const int RetCodeFilesystemError = 12200;

        /// <summary>
        ///     聊天平台错误
        /// </summary>
        public const int RetCodePlatformError = 12300;

        /// <summary>
        ///     动作逻辑错误 (如尝试向不存在的用户发送消息等)
        /// </summary>
        public const int RetCodeLogicError = 12400;

        #endregion
    }

    /// <summary>
    ///     表示一个动作响应
    /// </summary>
    [DataContract]
    public class Response<TData>
    {
        /// <summary>
        ///     执行状态 (成功与否)
        /// </summary>
        [DataMember(Name = "status")]
        public string? Status { get; set; }

        /// <summary>
        ///     返回码
        /// </summary>
        [DataMember(Name = "retcode")]
        public int RetCode { get; set; }

        /// <summary>
        ///     返回数据
        /// </summary>
        [DataMember(Name = "data")]
        public TData Data { get; set; }

        /// <summary>
        ///     错误信息
        /// </summary>
        [DataMember(Name = "message")]
        public string? Message { get; set; }

        /// <summary>
        ///     动作请求的 echo 字段 (原样返回)
        /// </summary>
        [DataMember(Name = "echo")]
        public object? Echo { get; set; }

        public static Response<TData> FailedResponse(int retCode, Exception exception) =>
            new()
            {
                Status = ActionStatus.StatusFailed,
                RetCode = retCode,
                Message = exception.Message
            };

        public byte[] Encode(bool isBinary) =>
            isBinary
                ? MessagePackSerializer.Serialize(this)
                : Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this));
    }
}
