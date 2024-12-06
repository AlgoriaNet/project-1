using UnityEngine;
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
}