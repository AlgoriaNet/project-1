using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using WebSocket;
using Random = UnityEngine.Random;


public class WebSocketManager : MonoBehaviour
{
    private static WebSocketManager _instance;
    //广播接收器
    private static readonly Dictionary<string, Dictionary<string, Action<JObject>>> BroadcastAcceptors = new();
    //请求回调
    private static readonly Dictionary<string, Action<JObject>> RequestCallbacks = new();

    private WebSocketSharp.WebSocket _ws;
    public static WebSocketManager Instance
    {
        get
        {
            if (!_instance) _instance = new GameObject("WebSocketManager").AddComponent<WebSocketManager>();
            return _instance;
        }
    }
    
    // 重连连接
    private const float ReconnectDelay = 0.5f;
    


    public void ConnectWebSocket()
    {
        _ws = new WebSocketSharp.WebSocket(Config.websocket);

        // 添加事件处理
        _ws.OnOpen += (sender, e) =>
        {
            Debug.Log("WebSocket Connected!");
            GamingSocketApi.Instance.Subscribe();
            GamingSocketApi.Instance.Action("login", new { data =  "WebSocket Connected!"});
        };

        _ws.OnMessage += (sender, e) =>
        {
            
            if (e.Data != null)
            {
                Debug.Log("Socket response:" + e.Data);
                JObject res = JObject.Parse(e.Data);
                if (res["type"] != null) return;
                WsResponse data = JsonConvert.DeserializeObject<WsResponse>(e.Data);
                if (data.Channel != null && data.message is { action: not null })
                {
                    string channel = data.Channel;
                    string action = data.message.action;
                    if (BroadcastAcceptors.ContainsKey(channel) && BroadcastAcceptors[channel].ContainsKey(action))
                    {
                        Action<JObject> callback = BroadcastAcceptors[channel][action];
                        callback?.Invoke(data.message.data);
                    }
                }
                if(data.Channel != null && data.message is { requestId: not null })
                {
                    string requestId = data.message.requestId;
                    if (RequestCallbacks.ContainsKey(requestId))
                    {
                        Action<JObject> callback = RequestCallbacks[requestId];
                        callback?.Invoke(data.message.data);
                        RequestCallbacks.Remove(requestId);
                    }
                }
            }
        };

        _ws.OnError += (sender, e) => { Debug.LogError("WebSocket Error: " + e.Message); };

        _ws.OnClose += (sender, e) =>
        {
            StartCoroutine(Reconnect());
        };

        // 连接到 WebSocket 服务
        _ws.Connect();
    }
    

    public void Subscribe(string channel)
    {
        var subscriptionMessage = new
        {
            command = "subscribe",
            identifier = JsonConvert.SerializeObject(new { channel, user_id = 1}),
        };
        Debug.Log(JsonConvert.SerializeObject(subscriptionMessage));
        _ws.Send(JsonConvert.SerializeObject(subscriptionMessage));
    }

    
    public void Action(string channel, string action, object data, string requestId = null)
    {
        string json = JsonConvert.SerializeObject(data);
        string sign = EncryptionUtil.Encrypt(json, requestId);
        
        var sendMessage = new
        {
            command = "message",
            identifier = JsonConvert.SerializeObject(new { channel, user_id = 1 }),
            data = JsonConvert.SerializeObject(new { requestId, action, json, sign })
        };
        _ws.Send(JsonConvert.SerializeObject(sendMessage));
    }
    
    
    public void Action(string channel, string action, object data, Action<JObject> successCallback)
    {
        string requestId = GenerateRequestId();
        RequestCallbacks.Add(requestId, successCallback);
        Action(channel, action, data, requestId);
    }
    
    private string GenerateRequestId()
    {
        return Guid.NewGuid() + Random.Range(1000, 9999).ToString();
    }

    public static void AddBroadcastAcceptor(string channel, string action, Action<JObject> successCallback)
    {
        if (!BroadcastAcceptors.ContainsKey(channel))
            BroadcastAcceptors.Add(channel, new Dictionary<string, Action<JObject>>());
        BroadcastAcceptors[channel][action] = successCallback;
    }
    
    void OnDestroy()
    {
        if (_ws != null && _ws.IsAlive) _ws.Close();
    }
    
    private IEnumerator Reconnect()
    {
        BeforeReconnect();
        while (true)
        {
            Debug.Log("Waiting to reconnect...");
            yield return new WaitForSeconds(ReconnectDelay);
            try
            {
                ConnectWebSocket();
                break;
            }
            catch (Exception ex)
            {
                Debug.LogError("Reconnect attempt failed: " + ex.Message);
            }
        }
        AfterReconnect();
    }

    private void AfterReconnect()
    {
        Time.timeScale = 1;
    }

    private void BeforeReconnect()
    {
        Time.timeScale = 0;
    }
}