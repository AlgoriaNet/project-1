using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json;

namespace model
{
    public class Player : ApplicationModel
    {
        [JsonProperty("id")]
        public BigInteger Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("level")]
        public int Level { get; set; }
        [JsonProperty("exp")]
        public int Exp { get; set; }
        [JsonProperty("gold_coin")]
        public int GoldCoin { get; set; }
        [JsonProperty("diamond")]
        public int Diamond { get; set; }
        [JsonProperty("stamina")]
        public int Stamina { get; set; }

        [JsonProperty("monthly_card_expiry")]
        public string MonthlyCardExpiry { get; set; }

        [JsonProperty("weekly_card_expiry")]
        public string WeeklyCardExpiry { get; set; }
    
        public List<Equipment> Equipments;
        public List<Gemstone> Gemstones; 
    }
}