using System.Collections.Generic;
using Spine;
using UnityEngine;

namespace model
{
    public class PlayerProfile : ApplicationModel
    {
        public Player Player { get; private set; }
        public List<Sidekick> Sidekick { get; private set; } = new();

        private static PlayerProfile _data;

        public static PlayerProfile Data
        {
            get { return _data ??= new PlayerProfile(); }
        }

        private PlayerProfile(){}
        
        public void SetPlayer(Player player)
        {
            // Player = player;

            var existingGemstones = this.Player?.Gemstones;
            var existingEquipments = this.Player?.Equipments; // Adjust property name if different
            this.Player = player;
            if (existingGemstones != null)
            {
                this.Player.Gemstones = existingGemstones;
            }
            if (existingEquipments != null)
            {
                this.Player.Equipments = existingEquipments; // Adjust property name if different
            }
            Debug.Log("Player Profile Set: " + Player.Name);
            NotifyListeners("Player");
            NotifyListeners("Bag");
            NotifyListeners("Equipments");
            NotifyListeners("Gemstones");
        }
        
        public void SetGems(List<Gemstone> gems)
        {
            Debug.Log("gems:" + gems.Count);
            // Player.Gemstones = gems;
            if (this.Player.Gemstones == null)
            {
                this.Player.Gemstones = new List<Gemstone>();
            }
            if (gems != null)
            {
                this.Player.Gemstones.AddRange(gems);
            }
            Debug.Log("gems in bag:" + GetGemstonesInPack().Count);
            NotifyListeners("Gemstones");
            NotifyListeners("Bag");
        }
        
        public List<Equipment> GetHeroEquipments()
        {
            return Player.Equipments?.FindAll(equipment => equipment.EquipWithHeroId != null );
        }
        
        public List<Equipment> GetSidekickEquipments(int sidekickId)
        {
            return Player.Equipments?.FindAll(equipment => equipment.EquipWithSidekickId == sidekickId);
        }
        
        public List<Equipment> GetEquipmentsInPack()
        {
            List<Equipment> equipments =  Player.Equipments?.FindAll(equipment => equipment.EquipWithHeroId == null && equipment.EquipWithSidekickId == null);
            equipments?.Sort((a, b) => b.Quality.CompareTo(a.Quality));
            return equipments;
        }
        
        public List<Gemstone> GetHeroGemstones()
        {
            return Player.Gemstones?.FindAll(gemstone => gemstone.InlayWithHeroId != null);
        }
        
        public List<Gemstone> GetSidekickGemstones(int sidekickId)
        {
            return Player.Gemstones?.FindAll(gemstone => gemstone.InlayWithSidekickId == sidekickId);
        }
        
        public List<Gemstone> GetHeroGemstones(string part)
        {
            return Player.Gemstones?.FindAll(gemstone => gemstone.InlayWithHeroId != null && gemstone.Part == part);
        }
        
        public List<Gemstone> GetSidekickGemstones(int sidekickId, string part)
        {
            return Player.Gemstones?.FindAll(gemstone => gemstone.InlayWithSidekickId == sidekickId && gemstone.Part == part);
        }
        
        public List<Gemstone> GetGemstonesInPack()
        {
            List<Gemstone> gemstones = Player.Gemstones?.FindAll(gemstone => gemstone.InlayWithHeroId == null && gemstone.InlayWithSidekickId == null);
            gemstones?.Sort((a, b) => b.Level.CompareTo(a.Level));
            return gemstones;
        }

        // copilot agent 2025-06-29: Add or update an item in ItemsJson (for shards, skillbooks, etc.)
        public void AddOrUpdateItem(string itemKey, int amount = 1)
        {
            if (Player.ItemsJson.ContainsKey(itemKey))
                Player.ItemsJson[itemKey] += amount;
            else
                Player.ItemsJson[itemKey] = amount;
            NotifyListeners("Bag"); // Notify UI to update Others tab
        }

        // copilot agent 2025-06-29: Get all 'other' items (shards, skillbooks, etc.)
        public Dictionary<string, int> GetOtherItemsInPack()
        {
            return Player.ItemsJson;
        }
    }
}