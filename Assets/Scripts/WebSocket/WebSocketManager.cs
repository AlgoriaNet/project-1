using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using WebSocketSharp;
using Object = UnityEngine.Object;

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

    private WebSocket ws;

    private static WebSocketManager _instance;

    private static Dictionary<string, Dictionary<string, Action<object>>> _actionCallbacks =
        new Dictionary<string, Dictionary<string, Action<object>>>();

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
        ws = new WebSocket(Config.websocket);

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
                if (res["type"] != null) return;
                MessageData data = JsonConvert.DeserializeObject<MessageData>(e.Data);
                Debug.Log(e.Data);
                if (e.Data.Contains("type")) return;
                Debug.Log(data);
                if (data.Channel != null && data.message is { action: not null })
                {
                    string channel = data.Channel;
                    string action = data.message.action;
                    if (_actionCallbacks.ContainsKey(channel) && _actionCallbacks[channel].ContainsKey(action))
                    {
                        Action<object> callback = _actionCallbacks[channel][action];
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
            identifier = JsonConvert.SerializeObject(new { channel, user_id = PlayerPrefs.GetInt("user_id") }),
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
            identifier = JsonConvert.SerializeObject(new { channel, user_id = PlayerPrefs.GetInt("user_id") }),
            data = JsonConvert.SerializeObject(new { action, json, sign })
        };
        ws.Send(JsonConvert.SerializeObject(sendMessage));
    }

    public void RegisterOnActionResponse(string channel, string action, Action<object> successCallback)
    {
        if (!_actionCallbacks.ContainsKey(channel))
            _actionCallbacks.Add(channel, new Dictionary<string, Action<object>>());
        _actionCallbacks[channel][action] = successCallback;
    }

    void OnDestroy()
    {
        if (ws != null && ws.IsAlive)
        {
            ws.Close();
        }
    }
}