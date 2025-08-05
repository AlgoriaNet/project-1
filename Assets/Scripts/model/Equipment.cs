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
        [JsonProperty("display_name")]
        public string DisplayName;
        public int Quality;
        public string Part;
        [JsonProperty("base_atk")]
        public int BaseAtk;
        [JsonProperty("growth_atk")]
        public int GrowthAtk;
        [JsonProperty("embedded_gems")]
        public List<EmbeddedGemSlot> EmbeddedGems = new List<EmbeddedGemSlot>();
        [JsonProperty("total_crystals_spent")]
        public int? TotalCrystalsSpent = 0;
        
        // New rank system fields
        [JsonProperty("upgrade_rank")]
        public int UpgradeRank = 1;
        [JsonProperty("rank_color")]
        public string RankColor = "#FFFFFF";
        [JsonProperty("rank_bonus_percentage")]
        public float RankBonusPercentage = 0f;
        [JsonProperty("total_attack_with_rank")]
        public float TotalAttackWithRank = 0f;
        [JsonProperty("can_upgrade_rank")]
        public bool CanUpgradeRank = false;

        public bool IsEquipped()
        {
            return EquipWithHeroId != 0 || EquipWithSidekickId != 0;
        }
        
        public int Attack => BaseAtk + IntensifyLevel * GrowthAtk;
        
        public int GetFirstEmptyGemSlot()
        {
            if (EmbeddedGems == null || EmbeddedGems.Count == 0)
                return 1; // First slot if no data
                
            for (int i = 0; i < EmbeddedGems.Count; i++)
            {
                if (EmbeddedGems[i].is_empty || EmbeddedGems[i].gem == null)
                {
                    return i + 1; // Return 1-based slot number
                }
            }
            return -1; // No empty slots
        }
    }
    
    [System.Serializable]
    public class EmbeddedGemSlot
    {
        public int slot;
        [CanBeNull] public Gemstone gem;
        public bool is_empty;
    }
}