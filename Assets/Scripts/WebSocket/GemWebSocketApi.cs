namespace WebSocket
{
    public class GemWebSocketApi  : BaseWebSocketApi
    {
        private static GemWebSocketApi _instance;

        public static GemWebSocketApi Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GemWebSocketApi();
                    _instance.SetChannel("GemstoneChannel");
                }
                return _instance;
            }
        }   
    }
}