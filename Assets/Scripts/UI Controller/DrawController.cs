// Grok Worked, but no heroKey deducted+++++++++++++++++++++++++++++++++++++++
using System;
using System.Collections.Generic;
using System.ComponentModel;
using model;
using Newtonsoft.Json.Linq;
using Unity.VisualScripting;
using UnityEngine;
using WebSocket;
using System.Linq;

namespace UI_Controller
{
    public class DrawController : MonoBehaviour
    {
        public DrawWebSocketApi drawWebSocketApi => DrawWebSocketApi.Instance;
        public static DrawController Instance { get; private set; }

        public List<Gemstone> LastDrawnGems { get; private set; } = new List<Gemstone>();
        public Dictionary<string, int> LastDrawnItems { get; private set; } = new Dictionary<string, int>();

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
            drawWebSocketApi.Action("draw", apiParams, (obj) => OnDrawSuccess(type, count, obj));
        }
      
        private void OnDrawSuccess(string type, int count, JObject obj)
        {
            if (obj == null)
            {
                Debug.LogError("Received null response from server in OnDrawSuccess");
                return;
            }

            if (obj["Player"] != null)
            {
                PlayerProfile.Data.SetPlayer(obj["Player"].ToObject<Player>());
            }
            else
            {
                Debug.LogWarning("No Player data in server response");
            }

            if (type != "hero")
            {
                var newGems = obj["gems"]?.ToObject<List<Gemstone>>();
                if (newGems != null && newGems.Count > 0)
                {
                    LastDrawnGems = newGems;
                    for (int i = 0; i < newGems.Count; i++)
                    {
                        var gem = newGems[i];
                        Debug.Log($"🆕 Newly Drawn Gem {i + 1}:");
                        Debug.Log($"  ID={gem.Id}");
                        Debug.Log($"  Level={gem.Level}");
                        Debug.Log($"  Part={gem.Part}");
                        Debug.Log($"  Name={gem.Name}");
                    }
                }

                if (obj["all_gems"] != null)
                {
                    PlayerProfile.Data.SetGems(obj["all_gems"].ToObject<List<Gemstone>>());
                }
                else
                {
                    var existingGems = PlayerProfile.Data.Player.Gemstones ?? new List<Gemstone>();
                    if (newGems != null)
                    {
                        existingGems.AddRange(newGems);
                        PlayerProfile.Data.SetGems(existingGems);
                    }
                }

                var gachaController = FindObjectOfType<GachaController>();
                if (gachaController != null)
                {
                    if (count == 1)
                    {
                        if (type == "rare gem")
                            gachaController.ExecuteRareGemDraw();
                        else if (type == "epic gem")
                            gachaController.ExecuteEpicGemDraw();
                    }
                    else if (count == 10)
                    {
                        if (type == "rare gem")
                            gachaController.ExecuteRareGemTenDraw();
                        else if (type == "epic gem")
                            gachaController.ExecuteEpicGemTenDraw();
                    }
                }
            }
            else
            {
                // HERO DRAW - Handle items array properly
                var itemsArray = obj["items"] as JArray;
                if (itemsArray != null && itemsArray.Count > 0)
                {
                    LastDrawnItems.Clear();
                    
                    // Store items in the exact order they appear in the array
                    // This preserves the server's intended display order
                    for (int i = 0; i < itemsArray.Count; i++)
                    {
                        var itemObj = itemsArray[i] as JObject;
                        if (itemObj != null)
                        {
                            foreach (var property in itemObj.Properties())
                            {
                                string itemKey = property.Name;
                                int quantity = property.Value.ToObject<int>();
                                
                                // Use index-based key to preserve order and handle duplicates
                                string orderedKey = $"{i:D2}_{itemKey}_{quantity}";
                                LastDrawnItems[orderedKey] = quantity;
                                
                                // Also update the player's actual inventory
                                if (PlayerProfile.Data.Player.ItemsJson.ContainsKey(itemKey))
                                {
                                    PlayerProfile.Data.Player.ItemsJson[itemKey] += quantity;
                                }
                                else
                                {
                                    PlayerProfile.Data.Player.ItemsJson[itemKey] = quantity;
                                }
                                
                                Debug.Log($"🆕 Newly Drawn Item [{i}]: {itemKey} x{quantity}");
                            }
                        }
                    }
                    PlayerProfile.Data.NotifyListeners("Bag");
                }
                else
                {
                    Debug.LogError("❌ No items array found in server response or array is empty");
                }

                var gachaController = FindObjectOfType<GachaController>();
                if (gachaController != null)
                {
                    if (count == 1)
                        gachaController.ExecuteShardDraw();
                    else if (count == 10)
                        gachaController.ExecuteShardTenDraw();
                }
            }
        }

        public Gemstone GetLastDrawnGem()
        {
            return LastDrawnGems != null && LastDrawnGems.Count > 0 ? LastDrawnGems[0] : null;
        }

        public List<Gemstone> GetLastDrawnGems()
        {
            return LastDrawnGems ?? new List<Gemstone>();
        }

        public string GetLastDrawnItemKey()
        {
            return LastDrawnItems != null && LastDrawnItems.Count > 0 
                ? LastDrawnItems.Keys.First() 
                : null;
        }

        public Dictionary<string, int> GetLastDrawnItems()
        {
            return LastDrawnItems ?? new Dictionary<string, int>();
        }
    }
}

