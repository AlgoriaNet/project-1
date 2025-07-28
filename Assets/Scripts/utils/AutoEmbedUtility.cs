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
            Debug.Log($"[AutoEmbedUtility] ✅ Starting auto embed for {context} (ID: {contextId})");
            
            if (context == EmbedContext.Ally && contextId == 0)
            {
                Debug.LogWarning("[AutoEmbedUtility] Invalid sidekick ID 0 for Ally context");
                onComplete?.Invoke(new AutoEmbedResult { TotalEmbedded = 0, FailedEmbeds = 0 });
                return;
            }
            
            // Find all equipped equipment for this context
            string[] equipmentSlots = { "Helm", "Shoulder", "Chest", "Pants", "Gloves", "Boots" };
            List<(Equipment equipment, List<(Gemstone gem, int slot)> embedActions)> embedPlans = new List<(Equipment, List<(Gemstone, int)>)>();
            
            int totalEmptySlots = 0;
            int totalGemsToEmbed = 0;
            
            foreach (string slot in equipmentSlots)
            {
                Equipment equippedEquipment = GetEquippedEquipmentForContext(slot, context, contextId);
                if (equippedEquipment == null)
                {
                    Debug.Log($"[AutoEmbedUtility] No {slot} equipped for {context} {contextId}");
                    continue;
                }
                
                // Find empty gem slots in this equipment
                List<int> emptySlots = GetEmptyGemSlots(equippedEquipment);
                if (emptySlots.Count == 0)
                {
                    Debug.Log($"[AutoEmbedUtility] No empty gem slots in {equippedEquipment.Name}");
                    continue;
                }
                
                // Find best gems for this equipment part
                List<Gemstone> availableGems = FindBestGemsForPart(slot, emptySlots.Count);
                if (availableGems.Count == 0)
                {
                    Debug.Log($"[AutoEmbedUtility] No available gems for {slot}");
                    continue;
                }
                
                // Create embed plan for this equipment
                List<(Gemstone gem, int slot)> equipmentEmbedActions = new List<(Gemstone, int)>();
                for (int i = 0; i < System.Math.Min(emptySlots.Count, availableGems.Count); i++)
                {
                    equipmentEmbedActions.Add((availableGems[i], emptySlots[i]));
                    totalGemsToEmbed++;
                }
                
                if (equipmentEmbedActions.Count > 0)
                {
                    embedPlans.Add((equippedEquipment, equipmentEmbedActions));
                    Debug.Log($"[AutoEmbedUtility] Plan for {equippedEquipment.Name}: {equipmentEmbedActions.Count} gems to embed");
                }
                
                totalEmptySlots += emptySlots.Count;
            }
            
            if (embedPlans.Count == 0)
            {
                Debug.Log("[AutoEmbedUtility] No gems to embed - no equipped equipment with empty slots or no suitable gems available");
                onComplete?.Invoke(new AutoEmbedResult { TotalEmbedded = 0, FailedEmbeds = 0 });
                return;
            }
            
            Debug.Log($"[AutoEmbedUtility] Auto embed plan: {totalGemsToEmbed} gems across {embedPlans.Count} equipment pieces");
            
            // Execute embedding operations
            ExecuteEmbedPlans(embedPlans, context, contextId, onComplete);
        }
        
        /// <summary>
        /// Execute the embed plans for all equipment
        /// </summary>
        private static void ExecuteEmbedPlans(List<(Equipment equipment, List<(Gemstone gem, int slot)> embedActions)> embedPlans, 
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
                    
                    Debug.Log($"[AutoEmbedUtility] Embedding {gem.EffectName} (Level {gem.Level}) into {equipment.Name} slot {slot}");
                    
                    AutoEmbedSingleGem(gem, equipment, slot, context, contextId, (success) => {
                        completedOperations++;
                        
                        if (success)
                        {
                            result.TotalEmbedded++;
                            Debug.Log($"[AutoEmbedUtility] ✅ Successfully embedded {gem.EffectName} into {equipment.Name}");
                        }
                        else
                        {
                            result.FailedEmbeds++;
                            Debug.LogError($"[AutoEmbedUtility] ❌ Failed to embed {gem.EffectName} into {equipment.Name}");
                        }
                        
                        // Check if all operations completed
                        if (completedOperations >= totalOperations)
                        {
                            Debug.Log($"[AutoEmbedUtility] Auto embed completed: {result.TotalEmbedded} embedded, {result.FailedEmbeds} failed");
                            onComplete?.Invoke(result);
                        }
                    });
                }
            }
        }
        
        /// <summary>
        /// Auto embed a single gem into an equipment slot
        /// </summary>
        private static void AutoEmbedSingleGem(Gemstone gem, Equipment equipment, int slotNumber, 
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

            Debug.Log($"[AutoEmbedUtility] Auto embed API call - gemId: {gem.Id}, equipmentId: {equipment.Id}, slotNumber: {slotNumber}");
            
            // Use the same inlay API as manual embedding
            gemApi.Action("inlay", apiParams, (response) => {
                Debug.Log($"[AutoEmbedUtility] Auto embed response received for {gem.EffectName}");
                
                // Check for success response (same as manual embedding)
                if (response["success"]?.Value<bool>() == true && response["updated_equipment"] != null)
                {
                    Debug.Log($"[AutoEmbedUtility] ✅ Gem {gem.EffectName} auto embedded successfully");
                    
                    // Update PlayerProfile from server response (same as manual embedding)
                    UpdatePlayerProfileFromEmbedResponse(response);
                    
                    onComplete?.Invoke(true);
                }
                else
                {
                    string error = response["error"]?.Value<string>() ?? "Unknown error";
                    Debug.LogError($"[AutoEmbedUtility] ❌ Auto embed failed for {gem.EffectName}: {error}");
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
        /// Get empty gem slots for an equipment piece
        /// </summary>
        private static List<int> GetEmptyGemSlots(Equipment equipment)
        {
            List<int> emptySlots = new List<int>();
            
            if (equipment.EmbeddedGems == null || equipment.EmbeddedGems.Count == 0)
            {
                // If no embedded gems data, assume all 5 slots are empty
                for (int i = 1; i <= 5; i++)
                {
                    emptySlots.Add(i);
                }
                return emptySlots;
            }
            
            // Check each slot for emptiness
            for (int i = 0; i < equipment.EmbeddedGems.Count && i < 5; i++)
            {
                if (equipment.EmbeddedGems[i].is_empty || equipment.EmbeddedGems[i].gem == null)
                {
                    emptySlots.Add(i + 1); // Convert to 1-based slot number
                }
            }
            
            // If we have fewer than 5 slots in data, assume remaining are empty
            for (int i = equipment.EmbeddedGems.Count; i < 5; i++)
            {
                emptySlots.Add(i + 1);
            }
            
            return emptySlots;
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
                Debug.Log($"[AutoEmbedUtility] No suitable gems available for {equipmentPart}");
                return new List<Gemstone>();
            }

            // Sort by new two-step ranking: Level (highest first) > EntryValue (highest first) > ID
            List<Gemstone> bestGems = suitableGems
                .OrderByDescending(gem => gem.Level)             // Primary: Level (7 > 6 > 5 > 4 > 3 > 2 > 1)
                .ThenByDescending(gem => gem.EntryValue)         // Secondary: Entry value within same level
                .ThenByDescending(gem => gem.Id)                 // Final: ID for consistent ordering
                .Take(maxCount)
                .ToList();

            Debug.Log($"[AutoEmbedUtility] Found {bestGems.Count} suitable {equipmentPart} gems (max {maxCount} requested)");
            Debug.Log($"[AutoEmbedUtility] Gem priority ranking for {equipmentPart}:");
            for (int i = 0; i < bestGems.Count; i++)
            {
                var gem = bestGems[i];
                Debug.Log($"[AutoEmbedUtility] #{i+1}: {gem.EffectName} (Level: {gem.Level}, Power: {gem.EntryValue}, LevelName: {gem.LevelName ?? "N/A"}, ID: {gem.Id})");
            }

            return bestGems;
        }
        
        /// <summary>
        /// Update PlayerProfile from embed response (copied from GemDetail.cs)
        /// </summary>
        private static void UpdatePlayerProfileFromEmbedResponse(JObject response)
        {
            Debug.Log($"[AutoEmbedUtility] UpdatePlayerProfileFromEmbedResponse called with: {response}");
            
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
                            Debug.Log($"[AutoEmbedUtility] Updated equipment {updatedEquipment.Name} with embedded gems");
                            break;
                        }
                    }
                }
            }
            
            // Update inventory gems
            if (response["inventory_gems"] != null)
            {
                var inventoryGems = response["inventory_gems"].ToObject<List<Gemstone>>();
                Debug.Log($"[AutoEmbedUtility] Updating gem inventory with {inventoryGems?.Count ?? 0} gems");
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