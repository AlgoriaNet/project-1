namespace WebSocket
{
    public class PurchaseWebSocketApi  : BaseWebSocketApi
    {
        private static PurchaseWebSocketApi _instance;

        public static PurchaseWebSocketApi Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PurchaseWebSocketApi();
                    _instance.SetChannel("PurchaseChannel");
                }
                return _instance;
            }
        }   
    }
}