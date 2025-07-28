using Newtonsoft.Json;

namespace model
{
    public class Gemstone : ApplicationModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("effect_name")]
        public string EffectName { get; set; }

        [JsonProperty("effect_description")]
        public string EffectDescription { get; set; }

        [JsonProperty("part")]
        public string Part { get; set; }

        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("level_name")]
        public string LevelName { get; set; } // Level name corresponding to gem tier

        [JsonProperty("is_locked")]
        public bool IsLocked { get; set; }

        [JsonProperty("inlay_with_hero_id")]
        public int? InlayWithHeroId { get; set; } // 使用 int? 表示可能为 null

        [JsonProperty("inlay_with_sidekick_id")]
        public int? InlayWithSidekickId { get; set; } // 使用 int? 表示可能为 null

        [JsonProperty("entry_id")]
        public int EntryId { get; set; }

        [JsonProperty("entry_value")]
        public double EntryValue { get; set; }

        [JsonProperty("equipment_id")]
        public int? EquipmentId { get; set; }

        [JsonProperty("slot_number")]
        public int? SlotNumber { get; set; }

        [JsonProperty("is_in_inventory")]
        public bool IsInInventory { get; set; }

        [JsonProperty("is_embedded")]
        public bool IsEmbedded { get; set; }
    }
}