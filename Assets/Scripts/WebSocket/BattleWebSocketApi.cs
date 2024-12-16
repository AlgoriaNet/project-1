namespace WebSocket
{
    public class BattleWebSocketApi : BaseWebSocketApi
    {
        private static BattleWebSocketApi _instance;

        public static BattleWebSocketApi Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BattleWebSocketApi();
                    _instance.SetChannel("BattleChannel");
                }
                _instance.Subscribe();
                return _instance;
            }
        }   
    }
}