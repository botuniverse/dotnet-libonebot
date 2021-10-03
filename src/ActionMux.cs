using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace LibOneBot
{
    /// <summary>
    ///     将动作请求按动作名称分发到不同的 <see cref="IHandler" /> 对象处理
    /// </summary>
    public class ActionMux : IHandler
    {
        private readonly Dictionary<string, IHandler> _handlers = new();

        /// <summary>
        ///     创建一个新的 <see cref="ActionMux" /> 对象
        /// </summary>
        public ActionMux()
        {
            HandleFunc<ImmutableSortedSet<string>, object>(
                Actions.ActionGetSupportedActions,
                HandleGetSupportedActions);
        }

        /// <summary>
        ///     为 <see cref="ActionMux" /> 实现 <see cref="IHandler" /> 接口
        /// </summary>
        public void HandleAction(object response, object request)
        {
            string action = ((IRequestIntl)request).Action;
            bool hasValue = _handlers.TryGetValue(action, out IHandler? handler);
            if (!hasValue)
            {
                ((IResponseIntl)response).WriteFailedIntl(
                    RetCode.RetCodeActionNotFound,
                    new InvalidOperationException($"动作 {action} 不存在"));
                return;
            }

            handler!.HandleAction(response, request);
        }

        private void HandleGetSupportedActions(
            Response<ImmutableSortedSet<string>> response,
            Request<object> request) =>
            response.WriteData(
                _handlers
                    .Keys
                    .ToImmutableSortedSet());

        /// <summary>
        ///     将一个 <see cref="Action{T1, T2}" /> 对象注册为指定动作的请求处理器
        /// </summary>
        /// <remarks>
        ///     <para>若要注册为核心动作的请求处理器, 建议使用 <see cref="Actions" /> 常量作为动作名</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">动作名称为空时引发</exception>
        public void HandleFunc<TData, TParams>(
            string action,
            Action<Response<TData>, Request<TParams>> handler) =>
            Handle(action, new HandlerFunc<TData, TParams>(handler));

        /// <summary>
        ///     将一个 <see cref="IHandler" /> 对象注册为指定动作的请求处理器
        /// </summary>
        /// <remarks>
        ///     <para>若要注册为核心动作的请求处理器, 建议使用 <see cref="Actions" /> 常量作为动作名</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">动作名称为空时引发</exception>
        public void Handle(string action, IHandler handler)
        {
            if (string.IsNullOrWhiteSpace(action))
                throw new ArgumentNullException(nameof(action), "动作名称不能为空");
            _handlers[action] = handler;
        }
    }
}
