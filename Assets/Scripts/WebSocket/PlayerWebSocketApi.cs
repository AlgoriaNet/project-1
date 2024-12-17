namespace WebSocket
{
    public class PlayerWebSocketApi : BaseWebSocketApi
    {
        private static PlayerWebSocketApi _instance;

        public static PlayerWebSocketApi Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PlayerWebSocketApi();
                    _instance.SetChannel("PlayerChannel");
                }
                _instance.Subscribe();
                return _instance;
            }
        }   
    }
}