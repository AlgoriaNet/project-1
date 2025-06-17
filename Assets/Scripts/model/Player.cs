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

        [JsonProperty("equipments")] public List<Equipment> Equipments;
        [JsonProperty("gemstones")] public List<Gemstone> Gemstones;

        [JsonProperty("created_at")] public string CreatedAt { get; set; }
        [JsonProperty("updated_at")] public string UpdatedAt { get; set; }

        [JsonProperty("items_json")]
        public Dictionary<string, int> ItemsJson { get; set; }
    }
}