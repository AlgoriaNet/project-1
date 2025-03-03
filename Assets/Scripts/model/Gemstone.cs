using Newtonsoft.Json;

namespace model
{
    public class Gemstone : ApplicationModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("part")]
        public string Part { get; set; }

        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("quality")]
        public int? Quality { get; set; } // 使用 int? 表示可能为 null

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
    }
}