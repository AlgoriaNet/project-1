using System;
using System.Collections;
using System.Collections.Generic;
using model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PimDeWitte.UnityMainThreadDispatcher;
using UnityEngine;
using UnityEngine.Events;
using WebSocket;
using Random = UnityEngine.Random;


public class WebSocketManager : MonoBehaviour
{
    private static WebSocketManager _instance;

    //广播接收器
    private static readonly Dictionary<string, Dictionary<string, UnityAction<JObject>>> BroadcastAcceptors = new();

    //请求回调
    private static readonly Dictionary<string, UnityAction<JObject>> RequestCallbacks = new();

    //失败回调
    private static readonly Dictionary<string, UnityAction<JObject>> ErrorCallbacks = new();

    //当失败时是否全局显示错误信息
    private static readonly Dictionary<string, bool> showGlobalErrorMsg = new();

    private static int _playerID;

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


    public void ConnectWebSocket(int playerID)
    {
        _playerID = playerID;
        _ws = new WebSocketSharp.WebSocket(Config.websocket);

        // 添加事件处理
        _ws.OnOpen += (sender, e) =>
        {
            Debug.Log("WebSocket Connected!");
            Debug.Log("WebSocketApis  Subscribe");
            GamingSocketApi.Instance.Subscribe();
            PlayerWebSocketApi.Instance.Subscribe();
            PurchaseWebSocketApi.Instance.Subscribe();
            DrawWebSocketApi.Instance.Subscribe();
            EquipmentWebSocketApi.Instance.Subscribe();
            GemWebSocketApi.Instance.Subscribe();
            BattleWebSocketApi.Instance.Subscribe();
            PlayerWebSocketApi.Instance.Action("profile", new { }, SetProfileFromServer);
            PlayerWebSocketApi.Instance.AddBroadcastAcceptor("send_periodic_rewards",result =>
            {
                SetProfileFromServer(result);
                // todo 弹奖励框
            });
        };
        
        _ws.OnClose += (sender, e) =>
        {
            GamingSocketApi.Instance.Disconnect();
            PlayerWebSocketApi.Instance.Disconnect();
            PurchaseWebSocketApi.Instance.Disconnect();
            DrawWebSocketApi.Instance.Disconnect();
            EquipmentWebSocketApi.Instance.Disconnect();
            GemWebSocketApi.Instance.Disconnect();
            BattleWebSocketApi.Instance.Disconnect();
            _instance = null;
        };

        _ws.OnMessage += (sender, e) =>
        {
            if (e.Data != null)
            {
                JObject res = JObject.Parse(e.Data);
                //如果是心跳包
                if (res["type"] != null) return;
                Debug.Log("Socket response:" + e.Data);
                WsResponse data = JsonConvert.DeserializeObject<WsResponse>(e.Data);
                if (data.Channel != null && data.message is { action: not null })
                {
                    string channel = data.Channel;
                    string action = data.message.action;
                    if (BroadcastAcceptors.ContainsKey(channel) && BroadcastAcceptors[channel].ContainsKey(action))
                    {
                        UnityAction<JObject> callback = BroadcastAcceptors[channel][action];
                        UnityMainThreadDispatcher.Instance().Enqueue(() => callback?.Invoke(data.message.data));
                    }
                }

                //需要回调的请求
                if (data.Channel != null && data.message is { requestId: not null })
                {
                    string requestId = data.message.requestId;
                    //code != 200 出现错误
                    Debug.Log(data.message.code);
                    if (data.message.code != 200)
                    {
                        var msg = data.message.data["msg"]?.ToString();
                        //全局显示错误
                        if (showGlobalErrorMsg.ContainsKey(requestId))
                        {
                            UnityMainThreadDispatcher.Instance().Enqueue(() =>
                            {
                                // todo
                            });
                            showGlobalErrorMsg.Remove(requestId);
                        }

                        if (ErrorCallbacks.ContainsKey(requestId))
                        {
                            UnityAction<JObject> callback = ErrorCallbacks[requestId];
                            UnityMainThreadDispatcher.Instance().Enqueue(() => callback?.Invoke(data.message.data));
                            ErrorCallbacks.Remove(requestId);
                        }
                    }
                    //code == 200 请求成功
                    else if (RequestCallbacks.ContainsKey(requestId))
                    {
                        UnityAction<JObject> callback = RequestCallbacks[requestId];
                        UnityMainThreadDispatcher.Instance().Enqueue(() => callback?.Invoke(data.message.data));
                        RequestCallbacks.Remove(requestId);
                    }
                }
            }
        };

        _ws.OnError += (sender, e) => { Debug.LogError("WebSocket Error: " + e.Message); };
        

        // 连接到 WebSocket 服务
        _ws.Connect();
    }


    public void Subscribe(string channel)
    {
        var subscriptionMessage = new
        {
            command = "subscribe",
            identifier = JsonConvert.SerializeObject(new { channel, user_id = _playerID }),
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
            identifier = JsonConvert.SerializeObject(new { channel, user_id = _playerID }),
            data = JsonConvert.SerializeObject(new { requestId, action, json, sign })
        };
        Debug.Log("WebSocket Send: " + JsonConvert.SerializeObject(sendMessage));
        _ws.Send(JsonConvert.SerializeObject(sendMessage));
    }


    public void Action(string channel, string action, object data, UnityAction<JObject> successCallback,
        UnityAction<JObject> errorCallback = null, bool showGlobalError = true)
    {
        string requestId = GenerateRequestId();
        RequestCallbacks.Add(requestId, successCallback);
        ErrorCallbacks.Add(requestId, errorCallback);
        showGlobalErrorMsg.Add(requestId, showGlobalError);
        Action(channel, action, data, requestId);
    }

    private string GenerateRequestId()
    {
        return Guid.NewGuid() + Random.Range(1000, 9999).ToString();
    }

    public static void AddBroadcastAcceptor(string channel, string action, UnityAction<JObject> successCallback)
    {
        if (!BroadcastAcceptors.ContainsKey(channel))
            BroadcastAcceptors.Add(channel, new Dictionary<string, UnityAction<JObject>>());
        BroadcastAcceptors[channel][action] = successCallback;
    }

    public void Disconnect(String channel)
    {
        if (_ws != null && _ws.IsAlive)
        {
            var disconnectMessage = new
            {
                command = "unsubscribe",
                identifier = JsonConvert.SerializeObject(new { channel, user_id = _playerID }),
            };
            Debug.Log("WebSocket Disconnect: " + JsonConvert.SerializeObject(disconnectMessage));
            _ws.Send(JsonConvert.SerializeObject(disconnectMessage));
        }
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
                ConnectWebSocket(_playerID);
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
    
    private void SetProfileFromServer(JObject obj)
    {
        if (obj == null || !obj.ContainsKey("Player"))
        {
            Debug.LogError("Invalid response from server: Player data not found.");
        }
        else
        {
            PlayerProfile.Data.SetPlayer(obj["Player"].ToObject<Player>());
        }
    }

    private void OnDestroy()
    {
        BroadcastAcceptors.Clear();
        RequestCallbacks.Clear();
        ErrorCallbacks.Clear();
        showGlobalErrorMsg.Clear();
        if (_ws != null && _ws.IsAlive) _ws.Close();
    }
}