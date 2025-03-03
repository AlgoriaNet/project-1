using System.Collections.Generic;
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
            Player = player;
            Debug.Log("player profile:" + Player.Equipments.Count);
            NotifyListeners("Player");
            NotifyListeners("Bag");
            NotifyListeners("Equipments");
            NotifyListeners("Gemstones");
        }
        
        public void SetGems(List<Gemstone> gems)
        {
            Debug.Log("gems:" + gems.Count);
            Player.Gemstones = gems;
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
    }
}