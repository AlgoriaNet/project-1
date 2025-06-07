using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Net;

public class GuestRegistrar : MonoBehaviour
{
    // private const string RegisterUrl = "http://82.156.77.48:3000/api/guest_login";
    private const string RegisterUrl = "https://server.algorianet.com/api/guest_login";

    private const string PlayerIdKey = "player_id";

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
                Debug.Log("Saved player_id Successfully: " + playerId);
            }
            catch
            {
                Debug.LogError("Failed to Save play_id");
            }
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

