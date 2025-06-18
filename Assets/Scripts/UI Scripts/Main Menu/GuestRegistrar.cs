using UnityEngine;
using UnityEngine.Networking;
using System.Net;
using System.Threading.Tasks;
using model;
using Newtonsoft.Json.Linq;
using WebSocket;

public class GuestRegistrar : MonoBehaviour
{
    private const string RegisterUrl = "/api/guest_login";
    private static PlayerWebSocketApi _playerApi;

    async void Awake()
    {
        string deviceId = SystemInfo.deviceUniqueIdentifier;
        await RegisterGuestAsync(deviceId);
    }

    private async Task RegisterGuestAsync(string deviceId)
    {
        // Bypass SSL certificate check (for testing only)
        ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true;

        string jsonBody = $"{{\"device_id\":\"{deviceId}\"}}";
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest request = new UnityWebRequest(Config.BaseUrl + RegisterUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            var operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Registration Failed: " + request.error);
            }
            else
            {
                Debug.Log("Registration Succeed: " + request.downloadHandler.text);
                try
                {
                    var json = JsonUtility.FromJson<RegisterResponseWrapper>(request.downloadHandler.text);
                    var playerId = json.id;
                    WebSocketManager.Instance.ConnectWebSocket(playerId);
                    Debug.Log("Saved player_id Successfully: " + playerId);
                }
                catch
                {
                    Debug.LogError("Failed to Save play_id");
                }
            }
        }
    }
}

[System.Serializable]
public class RegisterResponseWrapper
{
    public int id;
}
