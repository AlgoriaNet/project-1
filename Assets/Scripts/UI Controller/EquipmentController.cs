using WebSocket;

namespace UI_Controller
{
    public class EquipmentController
    {
        private EquipmentWebSocketApi _equipmentApi;
        
        private void Awake()
        {
            //临时的建立连接,  正常流程是在登录成功后建立连接
            _equipmentApi = EquipmentWebSocketApi.Instance;
        }
        
        
        private void Start()
        {
            
        }
    }
}