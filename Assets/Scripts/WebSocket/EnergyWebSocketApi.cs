namespace WebSocket
{
    public class EnergyWebSocketApi : BaseWebSocketApi
    {
        private static EnergyWebSocketApi _instance;

        public static EnergyWebSocketApi Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new EnergyWebSocketApi();
                    _instance.SetChannel("EnergyChannel");
                }
                return _instance;
            }
        }   
    }
}
