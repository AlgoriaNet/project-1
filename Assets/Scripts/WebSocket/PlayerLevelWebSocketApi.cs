namespace WebSocket
{
    public class PlayerLevelWebSocketApi : BaseWebSocketApi
    {
        private static PlayerLevelWebSocketApi _instance;

        public static PlayerLevelWebSocketApi Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PlayerLevelWebSocketApi();
                    _instance.SetChannel("PlayerLevelChannel");
                }
                return _instance;
            }
        }   
    }
}