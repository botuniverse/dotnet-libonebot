namespace LibOneBot
{
    public partial class OneBot
    {
        public ActionMux ActionMux { get; } = new();

        internal object HandleActionRequest(
            string action,
            object payload) =>
            ActionMux.HandleActionIntl(action, payload, false);
    }
}
