using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Net;
using model;
using Newtonsoft.Json.Linq;
using WebSocket;

public class GuestRegistrar : MonoBehaviour
{
    private const string RegisterUrl = "https://server.algorianet.com/api/guest_login";
    private const string PlayerIdKey = "player_id";

    // Add this line:
    private PlayerWebSocketApi _playerApi;

    void Start()
    {
        if (!PlayerPrefs.HasKey(PlayerIdKey))
        {
            string deviceId = SystemInfo.deviceUniqueIdentifier;
            StartCoroutine(RegisterGuest(deviceId));
        }
        else
        {
            Debug.Log("Already have player_id: " + PlayerPrefs.GetString(PlayerIdKey));
        }
    }

    private IEnumerator RegisterGuest(string deviceId)
    {
        // Bypass SSL certificate check (for testing only)
        ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true;

        string jsonBody = $"{{\"device_id\":\"{deviceId}\"}}";
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);

        UnityWebRequest request = new UnityWebRequest(RegisterUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Registeration Failed: " + request.error);
        }
        else
        {
            Debug.Log("Registeration Succeed: " + request.downloadHandler.text);

            try
            {
                var json = JsonUtility.FromJson<RegisterResponseWrapper>(request.downloadHandler.text);
                string playerId = json.player.id.ToString();
                PlayerPrefs.SetString(PlayerIdKey, playerId);
                PlayerPrefs.Save();
                WebSocketManager.Instance.ConnectWebSocket();  // Fixed - no parameter
                _playerApi = PlayerWebSocketApi.Instance;
                _playerApi.Action("profile", data: new { }, SetProfileFromServer);
                Debug.Log("Saved player_id Successfully: " + playerId);
            }
            catch
            {
                Debug.LogError("Failed to Save play_id");
            }
        }
    }

    private void SetProfileFromServer(JObject obj)
    {
        if (obj == null || !obj.ContainsKey("Player"))
        {
            Debug.LogError("Invalid response from server: Player data not found.");
        }
    }
}

[System.Serializable]
public class RegisterResponseWrapper
{
    public PlayerData player;
}

[System.Serializable]
public class PlayerData
{
    public string id;
}