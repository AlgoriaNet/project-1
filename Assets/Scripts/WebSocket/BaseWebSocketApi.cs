using UnityEngine;
using Object = System.Object;

public class BaseWebSocketApi
{
    protected string channel;

    public void setChannel(string channel)
    {
        this.channel = channel;
    }
    public void Subscribe()
    {
        WebSocketManager.Instance.Subscribe(channel);
    }

    public void Action(string action, Object data)
    {
        WebSocketManager.Instance.Action(channel, action, data);
    }
}