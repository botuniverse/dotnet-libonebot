using System;

namespace LibOneBot
{
    /// <summary>
    ///     动作处理器需要实现的接口
    /// </summary>
    public interface IHandler
    {
        public void HandleAction(object response, object request);
    }

    /// <summary>
    ///     表示一个实现 <see cref="IHandler" /> 接口的函数
    /// </summary>
    public sealed class HandlerFunc<TData, TParams> : IHandler
    {
        public HandlerFunc(
            Action<Response<TData>, Request<TParams>> func) =>
            _func = func;

        private readonly Action<Response<TData>, Request<TParams>> _func;

        public void HandleAction(object response, object request) =>
            _func((Response<TData>)response, (Request<TParams>)request);
    }
}
