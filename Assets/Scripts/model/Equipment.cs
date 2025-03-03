using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace model
{
    public class Equipment: ApplicationModel
    {
        // {:id=>1,
        //         :intensify_level=>0,
        //         :nearby_attributes=>nil,
        //         :additional_attributes=>nil,
        //         :equip_with_hero_id=>nil,
        //         :equip_with_sidekick_id=>nil,
        //     "description"=>nil,
        //     "name"=>"Pants_02",
        //     "quality"=>2,
        //     "part"=>nil,
        //     "base_atk"=>10,
        //     "growth_atk"=>2} 
        public int Id;
        [JsonProperty("intensify_level")]
        public int IntensifyLevel = 0;
        [JsonProperty("nearby_attributes")]
        public Dictionary<string, int> NearbyAttributes;
        [JsonProperty("additional_attributes")]
        public string AdditionalAttributes;
        [JsonProperty("equip_with_hero_id")]
        public int? EquipWithHeroId = 0;
        [JsonProperty("equip_with_sidekick_id")]
        public int? EquipWithSidekickId = 0;
        public string Description;
        public string Name;
        public int Quality;
        public string Part;
        [JsonProperty("base_atk")]
        public int BaseAtk;
        [JsonProperty("growth_atk")]
        public int GrowthAtk;

        public bool IsEquipped()
        {
            return EquipWithHeroId != 0 || EquipWithSidekickId != 0;
        }
        
        public int Attack => BaseAtk + IntensifyLevel * GrowthAtk;
    }
}