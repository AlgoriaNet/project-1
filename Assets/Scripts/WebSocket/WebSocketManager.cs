using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using WebSocketSharp;

public class WebSocketManager : MonoBehaviour
{
    public class MessageData
    {
        public class Content
        {
            public string action { get; set; }
            public int code { get; set; }
            public string data { get; set; }
            public string sign { get; set; }

        }
        public string type { get; set; }
        public Content message { get; set; }
        public string identifier { get; set; }

        public string Channel
        {
            get
            {
                JObject data = JObject.Parse(identifier);
                return data["channel"]?.ToString();
            }
        }
    }

    private WebSocketSharp.WebSocket ws;

    private static WebSocketManager _instance;

    private static readonly Dictionary<string, Dictionary<string, Action<JObject>>> ActionCallbacks = new();

    public static WebSocketManager Instance
    {
        get
        {
            if (!_instance)
            {
                _instance = new GameObject("WebSocketManager").AddComponent<WebSocketManager>();
            }

            return _instance;
        }
    }


    public void ConnectWebSocket()
    {
        ws = new WebSocketSharp.WebSocket(Config.websocket);

        // 添加事件处理
        ws.OnOpen += (sender, e) =>
        {
            Debug.Log("WebSocket Connected!");
            GamingSocketApi.Instance.Subscribe();
            GamingSocketApi.Instance.Action("login", new { data =  "WebSocket Connected!"});
        };

        ws.OnMessage += (sender, e) =>
        {
            
            if (e.Data != null)
            {
                JObject res = JObject.Parse(e.Data);
                Debug.Log("Socket response" + e.Data);
                if (res["type"] != null) return;
                MessageData data = JsonConvert.DeserializeObject<MessageData>(e.Data);
                if (data.Channel != null && data.message is { action: not null })
                {
                    string channel = data.Channel;
                    string action = data.message.action;
                    if (ActionCallbacks.ContainsKey(channel) && ActionCallbacks[channel].ContainsKey(action))
                    {
                        Action<JObject> callback = ActionCallbacks[channel][action];
                        JObject result = JObject.Parse(data.message.data);
                        callback?.Invoke(result);
                    }
                }
            }
        };

        ws.OnError += (sender, e) => { Debug.LogError("WebSocket Error: " + e.Message); };

        ws.OnClose += (sender, e) => { Debug.Log("WebSocket Closed!"); };

        // 连接到 WebSocket 服务
        ws.Connect();
    }

    public void Subscribe(string channel)
    {
        var subscriptionMessage = new
        {
            command = "subscribe",
            identifier = JsonConvert.SerializeObject(new { channel, user_id = 1}),
        };
        Debug.Log(JsonConvert.SerializeObject(subscriptionMessage));
        ws.Send(JsonConvert.SerializeObject(subscriptionMessage));
    }

    public void Action(string channel, string action, object data)
    {
        string json = JsonConvert.SerializeObject(data);
        string sign = EncryptionUtil.Encrypt(json);
        
        var sendMessage = new
        {
            command = "message",
            identifier = JsonConvert.SerializeObject(new { channel, user_id = 1 }),
            data = JsonConvert.SerializeObject(new { action, json, sign })
        };
        ws.Send(JsonConvert.SerializeObject(sendMessage));
    }

    public void RegisterOnActionResponse(string channel, string action, Action<JObject> successCallback)
    {
        if (!ActionCallbacks.ContainsKey(channel))
            ActionCallbacks.Add(channel, new Dictionary<string, Action<JObject>>());
        ActionCallbacks[channel][action] = successCallback;
    }

    void OnDestroy()
    {
        if (ws != null && ws.IsAlive)
        {
            ws.Close();
        }
    }
}