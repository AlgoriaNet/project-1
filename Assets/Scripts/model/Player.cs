using System.Collections.Generic;
using System.Numerics;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace model
{
    public class Player : ApplicationModel
    {
        [JsonProperty("id")] public BigInteger Id { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("level")] public int Level { get; set; }
        [JsonProperty("exp")] public int Exp { get; set; }
        [JsonProperty("gold_coin")] public int GoldCoin { get; set; }
        [JsonProperty("diamond")] public int Diamond { get; set; }
        [JsonProperty("stamina")] public int Stamina { get; set; }

        [JsonProperty("device_id")] public string DeviceId { get; set; }

        [JsonProperty("monthly_card_expiry")]
        [CanBeNull]
        public string MonthlyCardExpiry { get; set; }

        [JsonProperty("weekly_card_expiry")]
        [CanBeNull]
        public string WeeklyCardExpiry { get; set; }

        [JsonProperty("equipments")] public List<Equipment> Equipments = new ();
        [JsonProperty("gemstones")] public List<Gemstone> Gemstones = new (); 
        [JsonProperty("sidekicks")] public List<Sidekick> Sidekicks = new ();
        
        // DEPRECATED: Use Sidekicks collection instead
        [JsonProperty("summoned_allies")] public List<string> SummonedAllies { get; set; } = new List<string>();

        [JsonProperty("created_at")] public string CreatedAt { get; set; }
        [JsonProperty("updated_at")] public string UpdatedAt { get; set; }

        //解决后端items_json为null时,  对象不会初始化的问题
        private Dictionary<string, int> _itemsJson = new ();
        
        [JsonProperty("items_json")]
        public Dictionary<string, int> ItemsJson {
            get => _itemsJson;
            set => _itemsJson = value ?? new Dictionary<string, int>();
        }

        // Server-controlled draw costs for gacha
        [JsonProperty("draw_costs")] 
        public Dictionary<string, Dictionary<string, int>> DrawCosts { get; set; } = new ();
    }
}