using System.Collections.Generic;
using System.Linq;
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
            Debug.Log($"[PlayerProfile.SetPlayer] Setting new player data:");
            Debug.Log($"[PlayerProfile.SetPlayer] - Old Items count: {this.Player?.ItemsJson?.Count ?? 0}");
            Debug.Log($"[PlayerProfile.SetPlayer] - New Items count: {player?.ItemsJson?.Count ?? 0}");
            Debug.Log($"[PlayerProfile.SetPlayer] - Old Equipment count: {this.Player?.Equipments?.Count ?? 0}");
            Debug.Log($"[PlayerProfile.SetPlayer] - New Equipment count: {player?.Equipments?.Count ?? 0}");
            Debug.Log($"[PlayerProfile.SetPlayer] - Old Gemstone count: {this.Player?.Gemstones?.Count ?? 0}");
            Debug.Log($"[PlayerProfile.SetPlayer] - New Gemstone count: {player?.Gemstones?.Count ?? 0}");
            Debug.Log($"[PlayerProfile.SetPlayer] - Old Sidekicks count: {this.Player?.Sidekicks?.Count ?? 0}");
            Debug.Log($"[PlayerProfile.SetPlayer] - New Sidekicks count: {player?.Sidekicks?.Count ?? 0}");
            
            var oldPlayer = this.Player;
            
            // Preserve equipment and gemstone collections if backend doesn't provide them
            // This prevents data loss during shard/gem draws where backend only updates specific data
            if (oldPlayer != null)
            {
                if (player.Equipments == null || player.Equipments.Count == 0)
                {
                    Debug.Log("[PlayerProfile.SetPlayer] Preserving existing equipment collection");
                    player.Equipments = oldPlayer.Equipments;
                }
                
                if (player.Gemstones == null || player.Gemstones.Count == 0)
                {
                    Debug.Log("[PlayerProfile.SetPlayer] Preserving existing gemstone collection");
                    player.Gemstones = oldPlayer.Gemstones;
                }
            }
            
            // Set the player data (with preserved collections if needed)
            this.Player = player;
            
            // Update PlayerProfile.Sidekick collection to match Player.Sidekicks
            this.Sidekick = this.Player?.Sidekicks ?? new List<Sidekick>();
            
            Debug.Log("Player Profile Set: " + Player?.Name);
            Debug.Log($"[PlayerProfile.SetPlayer] Final Items count: {this.Player?.ItemsJson?.Count ?? 0}");
            Debug.Log($"[PlayerProfile.SetPlayer] Final Equipment count: {this.Player?.Equipments?.Count ?? 0}");
            Debug.Log($"[PlayerProfile.SetPlayer] Final Gemstone count: {this.Player?.Gemstones?.Count ?? 0}");
            Debug.Log($"[PlayerProfile.SetPlayer] Final Sidekicks count: {this.Player?.Sidekicks?.Count ?? 0}");
            
            NotifyListeners("Player");
            NotifyListeners("Bag");
            NotifyListeners("Equipments");
            NotifyListeners("Gemstones");
            NotifyListeners("Sidekicks");
        }
        
        public void SetGems(List<Gemstone> gems)
        {
            if (this.Player == null)
                return;
                
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
            List<Equipment> equipments = Player.Equipments?.FindAll(equipment => equipment.EquipWithHeroId == null && equipment.EquipWithSidekickId == null);
            equipments?.Sort((a, b) => b.Quality.CompareTo(a.Quality));
            return equipments ?? new List<Equipment>();
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

        // copilot agent 2025-06-29: Sidekick management methods
        public void SetSidekicks(List<Sidekick> sidekicks)
        {
            if (this.Player == null)
                return;
                
            this.Player.Sidekicks = sidekicks ?? new List<Sidekick>();
            this.Sidekick = this.Player.Sidekicks;
            NotifyListeners("Sidekicks");
        }

        public void AddSidekick(Sidekick sidekick)
        {
            if (this.Player == null)
                return;
                
            if (this.Player.Sidekicks == null)
                this.Player.Sidekicks = new List<Sidekick>();
            
            this.Player.Sidekicks.Add(sidekick);
            this.Sidekick = this.Player.Sidekicks;
            NotifyListeners("Sidekicks");
        }

        public List<Sidekick> GetPlayerSidekicks()
        {
            return this.Player?.Sidekicks ?? new List<Sidekick>();
        }

        public bool HasSidekick(string sidekickName)
        {
            return this.Player?.Sidekicks?.Any(s => s.Name?.Equals(sidekickName, System.StringComparison.OrdinalIgnoreCase) == true) ?? false;
        }

        public Sidekick GetSidekick(string sidekickName)
        {
            return this.Player?.Sidekicks?.FirstOrDefault(s => s.Name?.Equals(sidekickName, System.StringComparison.OrdinalIgnoreCase) == true);
        }

        public Sidekick GetSidekickById(int sidekickId)
        {
            return this.Player?.Sidekicks?.FirstOrDefault(s => s.Id == sidekickId);
        }

        // Backward compatibility: Check if an ally is unlocked (for existing UI logic)
        public bool IsAllyUnlocked(string allyName)
        {
            // Return false if Player is null (not loaded yet)
            if (this.Player == null)
            {
                Debug.Log($"[PlayerProfile.IsAllyUnlocked] Player is null, returning false for {allyName}");
                return false;
            }
                
            // First check the new sidekick system
            bool hasSidekick = HasSidekick(allyName);
            if (hasSidekick)
            {
                Debug.Log($"[PlayerProfile.IsAllyUnlocked] Found {allyName} in sidekicks, returning true");
                return true;
            }
                
            // Fall back to legacy SummonedAllies if needed
            bool inSummonedAllies = Player?.SummonedAllies?.Contains(allyName.ToLower()) ?? false;
            
            // **NEW**: Also check if it's in summoned allies with different casing patterns
            if (!inSummonedAllies && Player?.SummonedAllies != null)
            {
                // Try case-insensitive comparison and different formats
                inSummonedAllies = Player.SummonedAllies.Any(ally => 
                    string.Equals(ally, allyName, System.StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(ally, allyName.ToLower(), System.StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(ally.ToLower(), allyName.ToLower(), System.StringComparison.OrdinalIgnoreCase)
                );
            }
            
            Debug.Log($"[PlayerProfile.IsAllyUnlocked] {allyName} - Sidekick: {hasSidekick}, SummonedAllies: {inSummonedAllies}");
            Debug.Log($"[PlayerProfile.IsAllyUnlocked] Current sidekicks count: {Player?.Sidekicks?.Count ?? 0}");
            Debug.Log($"[PlayerProfile.IsAllyUnlocked] Current summoned allies: [{string.Join(", ", Player?.SummonedAllies ?? new List<string>())}]");
            
            return hasSidekick || inSummonedAllies;
        }

        /// <summary>
        /// Updates only the player's stamina value without affecting other player data.
        /// This is the proper way to handle energy claim responses.
        /// </summary>
        /// <param name="newStamina">The new stamina value from backend</param>
        public void UpdateStamina(int newStamina)
        {
            if (this.Player == null)
            {
                Debug.LogError("[PlayerProfile.UpdateStamina] Cannot update stamina - Player is null");
                return;
            }
            
            var oldStamina = this.Player.Stamina;
            this.Player.Stamina = newStamina;
            
            Debug.Log($"[PlayerProfile.UpdateStamina] Stamina updated: {oldStamina} → {newStamina}");
            
            // Notify UI that player data has changed
            NotifyListeners("Player");
        }
    }
}