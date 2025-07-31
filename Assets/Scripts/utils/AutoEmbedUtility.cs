using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using model;
using WebSocket;
using Newtonsoft.Json.Linq;
using System.Collections;

namespace GemUtils
{
    /// <summary>
    /// Auto embed utility that automatically embeds the best available gems into equipment slots
    /// Works for both Hero and Allies contexts, following AutoEquipUtility pattern
    /// </summary>
    public static class AutoEmbedUtility
    {
        public enum EmbedContext
        {
            Hero,
            Ally
        }

        /// <summary>
        /// Auto embed the best gems for all equipped equipment in the specified context
        /// </summary>
        /// <param name="context">Hero or Ally context</param>
        /// <param name="contextId">0 for Hero, actual sidekick ID for Ally</param>
        /// <param name="onComplete">Callback when all operations complete</param>
        public static void AutoEmbedAll(EmbedContext context, int contextId, System.Action<AutoEmbedResult> onComplete = null)
        {
            // Start auto embed for context
            
            if (context == EmbedContext.Ally && contextId == 0)
            {
                Debug.LogWarning("[AutoEmbedUtility] Invalid sidekick ID 0 for Ally context");
                onComplete?.Invoke(new AutoEmbedResult { TotalEmbedded = 0, FailedEmbeds = 0 });
                return;
            }
            
            // Find all equipped equipment for this context
            string[] equipmentSlots = { "Helm", "Shoulder", "Chest", "Pants", "Gloves", "Boots" };
            List<(Equipment equipment, List<(Gemstone gem, int slot, bool isUpgrade)> embedActions)> embedPlans = new List<(Equipment, List<(Gemstone, int, bool)>)>();
            
            int totalAvailableSlots = 0;
            int totalGemsToEmbed = 0;
            int totalUpgrades = 0;
            
            foreach (string slot in equipmentSlots)
            {
                Equipment equippedEquipment = GetEquippedEquipmentForContext(slot, context, contextId);
                if (equippedEquipment == null)
                {
                    continue;
                }
                
                // Find available gem slots (both empty and upgradeable) in this equipment
                List<(int slotIndex, Gemstone currentGem, bool isUpgrade)> availableSlots = GetAvailableGemSlots(equippedEquipment, slot);
                if (availableSlots.Count == 0)
                {
                    continue;
                }
                
                // Find best gems for this equipment part
                List<Gemstone> availableGems = FindBestGemsForPart(slot, availableSlots.Count);
                if (availableGems.Count == 0)
                {
                    continue;
                }
                
                // Create embed plan for this equipment
                List<(Gemstone gem, int slot, bool isUpgrade)> equipmentEmbedActions = new List<(Gemstone, int, bool)>();
                for (int i = 0; i < System.Math.Min(availableSlots.Count, availableGems.Count); i++)
                {
                    var targetSlot = availableSlots[i];
                    var gemToEmbed = availableGems[i];
                    
                    // For upgrades, make sure this gem is actually better than current
                    if (targetSlot.isUpgrade && !IsGemBetter(gemToEmbed, targetSlot.currentGem))
                    {
                        continue; // Skip if not actually better
                    }
                    
                    equipmentEmbedActions.Add((gemToEmbed, targetSlot.slotIndex, targetSlot.isUpgrade));
                    totalGemsToEmbed++;
                    if (targetSlot.isUpgrade) totalUpgrades++;
                }
                
                if (equipmentEmbedActions.Count > 0)
                {
                    embedPlans.Add((equippedEquipment, equipmentEmbedActions));
                }
                
                totalAvailableSlots += availableSlots.Count;
            }
            
            if (embedPlans.Count == 0)
            {
                onComplete?.Invoke(new AutoEmbedResult { TotalEmbedded = 0, FailedEmbeds = 0 });
                return;
            }
            
            // Execute embedding operations
            ExecuteEmbedPlans(embedPlans, context, contextId, onComplete);
        }
        
        /// <summary>
        /// Execute the embed plans for all equipment
        /// </summary>
        private static void ExecuteEmbedPlans(List<(Equipment equipment, List<(Gemstone gem, int slot, bool isUpgrade)> embedActions)> embedPlans, 
                                             EmbedContext context, int contextId, System.Action<AutoEmbedResult> onComplete)
        {
            AutoEmbedResult result = new AutoEmbedResult();
            int totalOperations = embedPlans.Sum(plan => plan.embedActions.Count);
            int completedOperations = 0;
            
            foreach (var plan in embedPlans)
            {
                Equipment equipment = plan.equipment;
                var embedActions = plan.embedActions;
                
                foreach (var action in embedActions)
                {
                    Gemstone gem = action.gem;
                    int slot = action.slot;
                    bool isUpgrade = action.isUpgrade;
                    
                    AutoEmbedSingleGem(gem, equipment, slot, isUpgrade, context, contextId, (success) => {
                        completedOperations++;
                        
                        if (success)
                        {
                            result.TotalEmbedded++;
                        }
                        else
                        {
                            result.FailedEmbeds++;
                        }
                        
                        // Check if all operations completed
                        if (completedOperations >= totalOperations)
                        {
                            onComplete?.Invoke(result);
                        }
                    });
                }
            }
        }
        
        /// <summary>
        /// Auto embed a single gem into an equipment slot
        /// </summary>
        private static void AutoEmbedSingleGem(Gemstone gem, Equipment equipment, int slotNumber, bool isUpgrade,
                                              EmbedContext context, int contextId, System.Action<bool> onComplete = null)
        {
            if (gem == null || equipment == null)
            {
                Debug.LogError("[AutoEmbedUtility] Cannot auto embed - gem or equipment is null");
                onComplete?.Invoke(false);
                return;
            }

            GemWebSocketApi gemApi = GemWebSocketApi.Instance;
            if (gemApi == null)
            {
                Debug.LogError("[AutoEmbedUtility] GemWebSocketApi.Instance is null - cannot auto embed");
                onComplete?.Invoke(false);
                return;
            }

            // Create API parameters (same format as manual embedding)
            var apiParams = new
            {
                gemId = gem.Id,
                equipmentId = equipment.Id,
                slotNumber = slotNumber
            };

            // Choose the correct API action: inlay for empty slots, replace for occupied slots
            string apiAction = isUpgrade ? "replace" : "inlay";
            
            // Use the appropriate API action
            gemApi.Action(apiAction, apiParams, (response) => {

                // Check for success response (same as manual embedding)
                if (response["success"]?.Value<bool>() == true && response["updated_equipment"] != null)
                {
                    // Update PlayerProfile from server response (same as manual embedding)
                    UpdatePlayerProfileFromEmbedResponse(response);
                    
                    onComplete?.Invoke(true);
                }
                else
                {
                    string error = response["error"]?.Value<string>() ?? "Unknown error";
                    Debug.LogError($"[AutoEmbedUtility] ❌ Auto embed failed for {gem.EffectName}: {error}");
                    Debug.LogError($"[AutoEmbedUtility] Full response: {response}");
                    onComplete?.Invoke(false);
                }
            }, (errorResponse) => {
                Debug.LogError($"[AutoEmbedUtility] 💥 WebSocket error for {gem.EffectName}: {errorResponse}");
                onComplete?.Invoke(false);
            });
        }
        
        
        /// <summary>
        /// Get equipped equipment for a specific context and slot
        /// </summary>
        private static Equipment GetEquippedEquipmentForContext(string equipmentPart, EmbedContext context, int contextId)
        {
            if (PlayerProfile.Data?.Player?.Equipments == null)
            {
                return null;
            }

            Equipment equippedEquipment = null;
            
            if (context == EmbedContext.Hero)
            {
                // Find equipment that matches the part and is equipped to Hero
                equippedEquipment = PlayerProfile.Data.Player.Equipments.Find(equipment =>
                    equipment.Part == equipmentPart && equipment.EquipWithHeroId > 0);
            }
            else // Ally context
            {
                // Find equipment that matches the part and is equipped to this sidekick
                equippedEquipment = PlayerProfile.Data.Player.Equipments.Find(equipment =>
                    equipment.Part == equipmentPart && equipment.EquipWithSidekickId == contextId);
            }

            return equippedEquipment;
        }
        
        /// <summary>
        /// Get available gem slots for an equipment piece (both empty slots and upgradeable slots)
        /// Returns list of (slotIndex, currentGem, isUpgrade) tuples
        /// </summary>
        private static List<(int slotIndex, Gemstone currentGem, bool isUpgrade)> GetAvailableGemSlots(Equipment equipment, string equipmentPart)
        {
            List<(int, Gemstone, bool)> availableSlots = new List<(int, Gemstone, bool)>();
            List<Gemstone> availableGems = FindBestGemsForPart(equipmentPart, 10); // Get more gems for comparison
            
            if (availableGems.Count == 0)
            {
                return availableSlots;
            }
            
            if (equipment.EmbeddedGems == null || equipment.EmbeddedGems.Count == 0)
            {
                // If no embedded gems data, assume all 5 slots are empty
                for (int i = 1; i <= 5; i++)
                {
                    availableSlots.Add((i, null, false)); // Empty slot
                }
                return availableSlots;
            }
            
            // Check each slot for emptiness or upgrade opportunity
            for (int i = 0; i < equipment.EmbeddedGems.Count && i < 5; i++)
            {
                if (equipment.EmbeddedGems[i].is_empty || equipment.EmbeddedGems[i].gem == null)
                {
                    // Empty slot
                    availableSlots.Add((i + 1, null, false));
                }
                else
                {
                    // Occupied slot - check if we can upgrade
                    Gemstone currentGem = equipment.EmbeddedGems[i].gem;
                    Gemstone bestAvailableGem = availableGems.FirstOrDefault(gem => IsGemBetter(gem, currentGem));
                    
                    if (bestAvailableGem != null)
                    {
                        availableSlots.Add((i + 1, currentGem, true)); // Upgradeable slot
                    }
                }
            }
            
            // If we have fewer than 5 slots in data, assume remaining are empty
            for (int i = equipment.EmbeddedGems.Count; i < 5; i++)
            {
                availableSlots.Add((i + 1, null, false)); // Empty slot
            }
            
            return availableSlots;
        }
        
        /// <summary>
        /// Find the best available gems for a specific equipment part
        /// Priority: Level (highest first) > EntryValue (highest first) > Quality > ID
        /// Only returns gems that are in inventory and match the part
        /// </summary>
        private static List<Gemstone> FindBestGemsForPart(string equipmentPart, int maxCount)
        {
            List<Gemstone> packGems = PlayerProfile.Data.GetGemstonesInPack();
            if (packGems == null || packGems.Count == 0)
            {
                return new List<Gemstone>();
            }

            // Filter gems by part and availability
            List<Gemstone> suitableGems = packGems.Where(gem => 
                gem.Part == equipmentPart && 
                gem.IsInInventory && 
                !gem.IsEmbedded
            ).ToList();
            
            if (suitableGems.Count == 0)
            {
                return new List<Gemstone>();
            }

            // Sort by new two-step ranking: Level (highest first) > EntryValue (highest first) > ID
            List<Gemstone> bestGems = suitableGems
                .OrderByDescending(gem => gem.Level)             // Primary: Level (7 > 6 > 5 > 4 > 3 > 2 > 1)
                .ThenByDescending(gem => gem.EntryValue)         // Secondary: Entry value within same level
                .ThenByDescending(gem => gem.Id)                 // Final: ID for consistent ordering
                .Take(maxCount)
                .ToList();

            return bestGems;
        }
        
        /// <summary>
        /// Compare two gems to determine if candidate is better than current
        /// Priority: Gem Level (Gem_07 > Gem_06 > ... > Gem_01) FIRST, then EntryValue/Power SECOND
        /// </summary>
        private static bool IsGemBetter(Gemstone candidate, Gemstone current)
        {
            if (candidate == null || current == null)
                return false;

            // PRIMARY: Gem Level (Gem_07 > Gem_06 > ... > Gem_01)
            if (candidate.Level > current.Level)
            {
                return true;
            }
            if (candidate.Level < current.Level)
                return false;

            // SECONDARY: Same gem level, compare by power
            if (candidate.EntryValue > current.EntryValue)
                return true;
            if (candidate.EntryValue < current.EntryValue)
                return false;

            // TERTIARY: Same level and power, compare by ID
            return candidate.Id > current.Id;
        }
        
        /// <summary>
        /// Update PlayerProfile from embed response (copied from GemDetail.cs)
        /// </summary>
        private static void UpdatePlayerProfileFromEmbedResponse(JObject response)
        {
            // Update equipment with embedded gems data
            if (response["updated_equipment"] != null)
            {
                var updatedEquipment = response["updated_equipment"].ToObject<Equipment>();
                if (updatedEquipment != null)
                {
                    // Find and update the equipment in player profile
                    var equipments = PlayerProfile.Data.Player.Equipments;
                    for (int i = 0; i < equipments.Count; i++)
                    {
                        if (equipments[i].Id == updatedEquipment.Id)
                        {
                            equipments[i] = updatedEquipment;
                            break;
                        }
                    }
                }
            }
            
            // Update inventory gems
            if (response["inventory_gems"] != null)
            {
                var inventoryGems = response["inventory_gems"].ToObject<List<Gemstone>>();
                PlayerProfile.Data.SetGems(inventoryGems);
            }
            
            // Notify listeners of data changes
            PlayerProfile.Data.NotifyListeners("Equipments");
            PlayerProfile.Data.NotifyListeners("Gemstones");
        }
    }

    /// <summary>
    /// Result of auto embed operation
    /// </summary>
    public class AutoEmbedResult
    {
        public int TotalEmbedded { get; set; } = 0;
        public int FailedEmbeds { get; set; } = 0;
        
        public bool HasEmbedded => TotalEmbedded > 0;
        public bool HasFailures => FailedEmbeds > 0;
        public int TotalAttempted => TotalEmbedded + FailedEmbeds;
    }
}