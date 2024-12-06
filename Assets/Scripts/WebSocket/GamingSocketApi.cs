using UnityEngine;

public class GamingSocketApi : BaseWebSocketApi
{
    private static GamingSocketApi _instance;

    public static GamingSocketApi Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GamingSocketApi();
                _instance.SetChannel("GamingChannel");
            }

            return _instance;
        }
    }
}