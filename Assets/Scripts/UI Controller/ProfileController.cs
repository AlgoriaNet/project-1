using model;
using Newtonsoft.Json.Linq;
using UnityEngine;
using WebSocket;

namespace UI_Controller
{
    public class ProfileController : MonoBehaviour
    {
        PlayerWebSocketApi _playerApi;
        private void Awake()
        {
            //临时的建立连接,  正常流程是在登录成功后建立连接
            WebSocketManager.Instance.ConnectWebSocket();
            _playerApi = PlayerWebSocketApi.Instance;
        }

        //临时的获取玩家信息,  正常流程是在登录成功后获取
        private void Start() => _playerApi.Action("profile", new { }, SetProfileFromServer);

        private void SetProfileFromServer(JObject obj) => PlayerProfile.Data.SetPlayer(obj["Player"].ToObject<Player>());
    }
}