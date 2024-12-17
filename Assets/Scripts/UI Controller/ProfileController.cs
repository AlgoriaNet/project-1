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
            WebSocketManager.Instance.ConnectWebSocket();
            _playerApi = PlayerWebSocketApi.Instance;
        }

        private void Start() => _playerApi.Action("profile", new { }, SetProfileFromServer);

        private void SetProfileFromServer(JObject obj) => PlayerProfile.Data.SetPlayer(obj["Player"].ToObject<Player>());
    }
}