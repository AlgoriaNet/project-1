using Newtonsoft.Json.Linq;
using UnityEngine.Events;
using Object = System.Object;

public class BaseWebSocketApi
{
    public string Channel { get; private set; }

    public void SetChannel(string channel)
    {
        Channel = channel;
    }

    public void Subscribe()
    {
        WebSocketManager.Instance.Subscribe(Channel);
    }

    public void Action(string action, Object data)
    {
        WebSocketManager.Instance.Action(Channel, action, data);
    }
    
    public void Action(string action, Object data, UnityAction<JObject> callback)
    {
        WebSocketManager.Instance.Action(Channel, action, data, callback);
    }
    
    public void AddBroadcastAcceptor(string action, UnityAction<JObject> callback)
    {
        WebSocketManager.AddBroadcastAcceptor(Channel, action, callback);
    }
    
}