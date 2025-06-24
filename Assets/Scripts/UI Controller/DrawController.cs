using System;
using System.Collections.Generic;
using System.ComponentModel;
using model;
using Newtonsoft.Json.Linq;
using Unity.VisualScripting;
using UnityEngine;
using WebSocket;

namespace UI_Controller
{
    public class DrawController : MonoBehaviour
    {
        public DrawWebSocketApi drawWebSocketApi => DrawWebSocketApi.Instance;
        public static DrawController Instance { get; private set; }

        public enum eDrawType
        {
            [Description("hero")]
            Hero,

            [Description("rare gem")]
            RareGem,

            [Description("epic gem")]
            EpicGem
        }

        public enum eConsumeItem
        {
            [Description("ad")]
            Ad,

            [Description("key")]
            Key,

            [Description("diamond")]
            Diamond
        }

        private void Start()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogError("DrawController instance already exists!");
                return;
            }
        }

        public void Draw(string type, string consumeItem, int count = 1)
        {
            var apiParams = new
            {
                card_pool_type = type,
                consume_item = consumeItem,
                count
            };
            drawWebSocketApi.Action("draw", apiParams, OnDrawSuccess);
        }

        private void OnDrawSuccess(JObject obj)
        {
            Debug.Log("draw:" + obj);
            PlayerProfile.Data.SetGems(obj["all_gems"].ToObject<List<Gemstone>>());
            PlayerProfile.Data.SetPlayer(obj["Player"].ToObject<Player>());
        }
    }
}