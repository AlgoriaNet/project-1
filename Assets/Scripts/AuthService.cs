using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System;
using Newtonsoft.Json;
public class AuthService
{
    private string loginUrl = $"{Config.BaseUrl}/api/login";

    public IEnumerator Login(string email, string password, Action<LoginResponse> onSuccess, Action<string> onError)
    {
        // 准备登录数据
    
        var jsonData =  JsonConvert.SerializeObject(new {email, password});
        Debug.Log(jsonData);
        var request = new UnityWebRequest(loginUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Login successful: " + request.downloadHandler.text);

            // 解析返回的数据并调用成功回调
            var response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
            onSuccess?.Invoke(response);
        }
        else
        {
            Debug.LogError("Login failed: " + request.error);
            onError?.Invoke("Invalid email or password.");
        }
    }

    [Serializable]
    public class LoginResponse
    {
        public string token; // JWT Token
        public User user;    // 用户信息
    }

    [Serializable]
    public class User
    {
        public int id;       // 用户ID
        public string email; // 用户邮箱
    }
}