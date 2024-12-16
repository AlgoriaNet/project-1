using Newtonsoft.Json.Linq;

namespace WebSocket
{
    public class WsResponse
    {
        public class Content
        {
            public string action { get; set; }
            public string requestId { get; set; }
            public int code { get; set; }
            public JObject data { get; set; }
            public string sign { get; set; }

        }
        public string type { get; set; }
        public Content message { get; set; }
        public string identifier { get; set; }

        public string Channel
        {
            get
            {
                JObject data = JObject.Parse(identifier);
                return data["channel"]?.ToString();
            }
        }
    }
}