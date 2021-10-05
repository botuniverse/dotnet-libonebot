using Xunit;

namespace LibOneBot.Test
{
    public class Comm
    {
        [Theory]
        [InlineData("http://127.0.0.1", true)]
        [InlineData("http://127.0.0.1:8080", true)]
        [InlineData("https://127.0.0.1", true)]
        [InlineData("http://www.contoso.com/base", true)]
        [InlineData("https://contoso.com", true)]
        [InlineData("https://contoso", true)]
        [InlineData("rtmp://contoso.com/base", false)]
        //[InlineData("https://0.0.0.0", false)]
        [InlineData("https:", false)]
        [InlineData("test_data", false)]
        [InlineData("https", false)]
        public void IsValidHttpUriTest(string url, bool valid)
        {
            Assert.Equal(valid, HttpWebhookComm.IsValidHttpUri(url));
        }

        [Theory]
        [InlineData("ws://127.0.0.1", true)]
        [InlineData("ws://127.0.0.1:8080", true)]
        [InlineData("wss://127.0.0.1", true)]
        [InlineData("ws://www.contoso.com/base", true)]
        [InlineData("wss://contoso.com", true)]
        [InlineData("wss://contoso", true)]
        [InlineData("rtmp://contoso.com/base", false)]
        //[InlineData("wss://0.0.0.0", false)]
        [InlineData("wss:", false)]
        [InlineData("test_data", false)]
        [InlineData("wss", false)]
        public void IsValidWebsocketUriTest(string url, bool valid)
        {
            Assert.Equal(valid, WebSocketReverseComm.IsValidWebsocketUri(url));
        }
    }
}
