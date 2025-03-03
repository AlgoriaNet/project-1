namespace WebSocket
{
    public class EquipmentWebSocketApi  : BaseWebSocketApi
    {
        private static EquipmentWebSocketApi _instance;

        public static EquipmentWebSocketApi Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new EquipmentWebSocketApi();
                    _instance.SetChannel("EquipmentChannel");
                    _instance.Subscribe();
                }
                return _instance;
            }
        }   
    }
}