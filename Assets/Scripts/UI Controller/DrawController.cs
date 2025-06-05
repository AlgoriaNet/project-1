using System;
using UnityEngine;
using WebSocket;

namespace UI_Controller
{
    public class DrawController:MonoBehaviour
    {
        public DrawWebSocketApi drawWebSocketApi;

        private void Start()
        {
            drawWebSocketApi = DrawWebSocketApi.Instance;
            
            
        }

        private void click1()
        {
            // drawWebSocketApi.Action("draw");
        }
    }
}