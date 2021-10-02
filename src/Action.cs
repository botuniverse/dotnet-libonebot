namespace LibOneBot
{
    /// <summary>
    ///     表示 OneBot 标准定义的核心动作名称
    /// </summary>
    public static class Actions
    {
        #region LibOneBot 自动处理的特殊动作

        /// <summary>
        ///     获取最新事件列表
        /// </summary>
        public const string ActionGetLatestEvents = "get_latest_events";

        /// <summary>
        ///     获取支持的动作列表
        /// </summary>
        public const string ActionGetSupportedActions = "get_supported_actions";

        #endregion

        #region 元信息相关动作

        /// <summary>
        ///     获取 OneBot 运行状态
        /// </summary>
        public const string ActionGetStatus = "get_status";

        /// <summary>
        ///     获取 OneBot 版本
        /// </summary>
        public const string ActionGetVersion = "get_version";

        #endregion


        #region 消息相关动作

        /// <summary>
        ///     发送消息
        /// </summary>
        public const string ActionSendMessage = "send_message";

        /// <summary>
        ///     删除消息
        /// </summary>
        public const string ActionDeleteMessage = "delete_message";

        #endregion


        #region 用户相关动作

        /// <summary>
        ///     获取机器人自身信息
        /// </summary>
        public const string ActionGetSelfInfo = "get_self_info";

        /// <summary>
        ///     获取用户信息
        /// </summary>
        public const string ActionGetUserInfo = "get_user_info";

        /// <summary>
        ///     获取好友列表
        /// </summary>
        public const string ActionGetFriendList = "get_friend_list";

        #endregion


        #region 群相关动作

        /// <summary>
        ///     获取群信息
        /// </summary>
        public const string ActionGetGroupInfo = "get_group_info";

        /// <summary>
        ///     获取群列表
        /// </summary>
        public const string ActionGetGroupList = "get_group_list";

        /// <summary>
        ///     获取群成员信息
        /// </summary>
        public const string ActionGetGroupMemberInfo = "get_group_member_info";

        /// <summary>
        ///     获取群成员列表
        /// </summary>
        public const string ActionGetGroupMemberList = "get_group_member_list";

        #endregion
    }

    /// <summary>
    ///     表示一个动作名称
    /// </summary>
    public sealed class Action
    {
        /// <summary>
        ///     动作名称前缀
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        ///     动作名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     是否为扩展动作
        /// </summary>
        public bool IsExtended { get; set; }

        /// <summary>
        ///     返回动作名称的字符串表示, 即动作请求中的 <c>action</c> 字段值
        /// </summary>
        /// <returns></returns>
        public override string ToString() => IsExtended ? Prefix + "_" + Name : Name;
    }
}
