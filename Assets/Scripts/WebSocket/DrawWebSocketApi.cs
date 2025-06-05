namespace WebSocket
{
    public class DrawWebSocketApi  : BaseWebSocketApi
    {
        private static DrawWebSocketApi _instance;

        public static DrawWebSocketApi Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DrawWebSocketApi();
                    _instance.SetChannel("DrawChannel");
                    _instance.Subscribe();
                }
                return _instance;
            }
        }   
    }
}