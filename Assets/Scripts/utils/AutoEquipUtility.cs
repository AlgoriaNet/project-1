using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using model;
using WebSocket;
using Newtonsoft.Json.Linq;
using System.Collections;
using PimDeWitte.UnityMainThreadDispatcher.UI_Scripts.Allies_Menu;

namespace EquipmentUtils
{
    /// <summary>
    /// Common auto equip utility that works for both Hero and Allies
    /// Automatically finds and equips the best available equipment from pack
    /// </summary>
    public static class AutoEquipUtility
    {
        public enum EquipContext
        {
            Hero,
            Ally
        }

        /// <summary>
        /// Auto equip the best equipment for the specified context (Hero or Ally)
        /// </summary>
        /// <param name="context">Hero or Ally context</param>
        /// <param name="contextId">0 for Hero, actual sidekick ID for Ally</param>
        /// <param name="onComplete">Callback when all operations complete</param>
        public static void AutoEquipAll(EquipContext context, int contextId, System.Action onComplete = null)
        {
            Debug.Log($"[AutoEquipUtility] ✅ Starting auto equip for {context} (ID: {contextId})");
            
            if (context == EquipContext.Ally && contextId == 0)
            {
                Debug.LogWarning("[AutoEquipUtility] Invalid sidekick ID 0 for Ally context");
                onComplete?.Invoke();
                return;
            }
            
            // Check all equipment slots for upgrades or empty slots
            string[] equipmentSlots = { "Helm", "Shoulder", "Chest", "Pants", "Gloves", "Boots" };
            List<(string slotType, Equipment currentEquipment, Equipment bestEquipment)> upgradeActions = new List<(string, Equipment, Equipment)>();
            
            foreach (string slot in equipmentSlots)
            {
                Equipment currentEquipment = GetCurrentlyEquippedForContext(slot, context, contextId);
                Equipment bestEquipment = FindBestEquipmentInPack(slot, currentEquipment);
                
                if (bestEquipment != null)
                {
                    if (currentEquipment == null)
                    {
                        Debug.Log($"[AutoEquipUtility] Found empty slot: {slot} - will equip {bestEquipment.Name}");
                        upgradeActions.Add((slot, null, bestEquipment));
                    }
                    else if (IsEquipmentBetter(bestEquipment, currentEquipment))
                    {
                        Debug.Log($"[AutoEquipUtility] Found upgrade for {slot}: {currentEquipment.Name} -> {bestEquipment.Name}");
                        upgradeActions.Add((slot, currentEquipment, bestEquipment));
                    }
                    else
                    {
                        Debug.Log($"[AutoEquipUtility] No upgrade available for {slot}: {currentEquipment.Name} is already the best");
                    }
                }
                else
                {
                    if (currentEquipment == null)
                    {
                        Debug.Log($"[AutoEquipUtility] No equipment available for empty slot: {slot}");
                    }
                    else
                    {
                        Debug.Log($"[AutoEquipUtility] No better equipment available for {slot}: {currentEquipment.Name}");
                    }
                }
            }
            
            if (upgradeActions.Count == 0)
            {
                Debug.Log("[AutoEquipUtility] No equipment upgrades or empty slots to fill");
                onComplete?.Invoke();
                return;
            }
            
            Debug.Log($"[AutoEquipUtility] Found {upgradeActions.Count} equipment actions to perform");
            
            // Track pending operations for completion callback
            int pendingOperations = upgradeActions.Count;
            
            // Perform all upgrade actions
            foreach (var action in upgradeActions)
            {
                if (action.currentEquipment != null)
                {
                    Debug.Log($"[AutoEquipUtility] Upgrading {action.slotType}: {action.currentEquipment.Name} -> {action.bestEquipment.Name}");
                }
                else
                {
                    Debug.Log($"[AutoEquipUtility] Equipping to empty {action.slotType}: {action.bestEquipment.Name}");
                }
                
                bool isUpgrade = action.currentEquipment != null;
                AutoEquipSingleItem(action.bestEquipment, context, contextId, isUpgrade, () => {
                    pendingOperations--;
                    if (pendingOperations <= 0)
                    {
                        // All operations completed
                        Debug.Log("[AutoEquipUtility] All auto equip operations completed");
                        onComplete?.Invoke();
                    }
                });
            }
        }

        /// <summary>
        /// Auto equip a single equipment item
        /// </summary>
        private static void AutoEquipSingleItem(Equipment equipment, EquipContext context, int contextId, bool isUpgrade, System.Action onComplete = null)
        {
            if (equipment == null)
            {
                Debug.LogError("[AutoEquipUtility] Cannot auto equip - equipment is null");
                onComplete?.Invoke();
                return;
            }

            EquipmentWebSocketApi equipmentApi = EquipmentWebSocketApi.Instance;
            if (equipmentApi == null)
            {
                Debug.LogError("[AutoEquipUtility] EquipmentWebSocketApi.Instance is null - cannot auto equip");
                onComplete?.Invoke();
                return;
            }

            // Create API parameters based on context
            var apiParams = new
            {
                type = context == EquipContext.Hero ? "hero" : "sidekick",
                sidekickId = contextId, // 0 for hero, actual ID for sidekick
                equipmentId = equipment.Id
            };

            // Choose the correct API action: equip for empty slots, replace for occupied slots
            string apiAction = isUpgrade ? "replace" : "equip";
            string debugRequestId = System.Guid.NewGuid().ToString()[..8];
            Debug.Log($"[AutoEquipUtility] 🚀 [{debugRequestId}] Auto equip API call - {apiAction} - type: {apiParams.type}, sidekickId: {contextId}, equipmentId: {equipment.Id}");
            
            // Use the appropriate API action
            equipmentApi.Action(apiAction, apiParams, (response) => {
                Debug.Log($"[AutoEquipUtility] 🔄 [{debugRequestId}] SUCCESS RESPONSE received for {equipment.Name}");
                Debug.Log($"[AutoEquipUtility] 📄 [{debugRequestId}] Response content: {response}");
                
                // Update PlayerProfile from server response
                UpdatePlayerProfileFromResponse(response, equipment.Name);
                
                // Call completion callback
                onComplete?.Invoke();
            }, (errorResponse) => {
                Debug.LogError($"[AutoEquipUtility] 💥 [{debugRequestId}] WebSocket ERROR for {equipment.Name}: {errorResponse}");
                onComplete?.Invoke();
            });
        }

        /// <summary>
        /// Get currently equipped equipment for a specific context and slot
        /// </summary>
        private static Equipment GetCurrentlyEquippedForContext(string equipmentPart, EquipContext context, int contextId)
        {
            if (PlayerProfile.Data?.Player?.Equipments == null)
            {
                return null;
            }

            Equipment currentEquipment = null;
            
            if (context == EquipContext.Hero)
            {
                // Find equipment that matches the part and is equipped to Hero
                currentEquipment = PlayerProfile.Data.Player.Equipments.Find(equipment =>
                    equipment.Part == equipmentPart && equipment.EquipWithHeroId > 0);
            }
            else // Ally context
            {
                // Find equipment that matches the part and is equipped to this sidekick
                currentEquipment = PlayerProfile.Data.Player.Equipments.Find(equipment =>
                    equipment.Part == equipmentPart && equipment.EquipWithSidekickId == contextId);
            }

            return currentEquipment;
        }

        /// <summary>
        /// Find the best available equipment in pack for a specific slot type
        /// Priority: Higher type number (Helm_05 > Helm_03 > Helm_01)
        /// Secondary: Equipment ID for same type (placeholder for power values)
        /// </summary>
        private static Equipment FindBestEquipmentInPack(string slotType, Equipment currentEquipment = null)
        {
            List<Equipment> packEquipments = PlayerProfile.Data.GetEquipmentsInPack();
            if (packEquipments == null || packEquipments.Count == 0)
            {
                return null;
            }

            // Filter equipment by slot type
            List<Equipment> suitableEquipments = packEquipments.Where(eq => eq.Part == slotType).ToList();
            
            if (suitableEquipments.Count == 0)
            {
                return null;
            }

            Debug.Log($"[AutoEquipUtility] Found {suitableEquipments.Count} {slotType} equipment(s) in pack");

            // Sort by equipment ranking: Primary = type number, Secondary = equipment ID
            Equipment bestEquipment = suitableEquipments.OrderByDescending(eq => GetEquipmentTypeNumber(eq.Name))
                                                       .ThenByDescending(eq => eq.Id) // Placeholder for power value ranking
                                                       .First();

            // If there's no current equipment, return the best from pack
            if (currentEquipment == null)
            {
                Debug.Log($"[AutoEquipUtility] Selected best {slotType} for empty slot: {bestEquipment.Name} (ID: {bestEquipment.Id}, Type: {GetEquipmentTypeNumber(bestEquipment.Name)})");
                return bestEquipment;
            }

            // If there's current equipment, only return if pack equipment is better
            if (IsEquipmentBetter(bestEquipment, currentEquipment))
            {
                Debug.Log($"[AutoEquipUtility] Found better {slotType}: {bestEquipment.Name} (Type: {GetEquipmentTypeNumber(bestEquipment.Name)}) > {currentEquipment.Name} (Type: {GetEquipmentTypeNumber(currentEquipment.Name)})");
                return bestEquipment;
            }

            Debug.Log($"[AutoEquipUtility] No better {slotType} found in pack than currently equipped {currentEquipment.Name}");
            return null;
        }

        /// <summary>
        /// Extract the type number from equipment name (e.g., "Helm_05" returns 5)
        /// </summary>
        private static int GetEquipmentTypeNumber(string equipmentName)
        {
            if (string.IsNullOrEmpty(equipmentName))
                return 0;

            string[] parts = equipmentName.Split('_');
            if (parts.Length >= 2 && int.TryParse(parts[1], out int typeNumber))
            {
                return typeNumber;
            }

            return 0;
        }

        /// <summary>
        /// Compare two equipment items to determine if one is better than the other
        /// Priority: Higher type number (Helm_05 > Helm_03 > Helm_01)
        /// For same type equipment, they are considered equal (not better)
        /// </summary>
        private static bool IsEquipmentBetter(Equipment candidate, Equipment current)
        {
            if (candidate == null || current == null)
                return false;

            int candidateType = GetEquipmentTypeNumber(candidate.Name);
            int currentType = GetEquipmentTypeNumber(current.Name);

            // Primary comparison: type number - only higher type is considered better
            if (candidateType > currentType)
                return true;

            // For same or lower type, candidate is not better
            return false;
        }

        /// <summary>
        /// Update PlayerProfile from server response
        /// </summary>
        private static void UpdatePlayerProfileFromResponse(JObject response, string equipmentName)
        {
            if (response == null)
            {
                Debug.LogError($"[AutoEquipUtility] ❌ Server response is null for {equipmentName}!");
                return;
            }
            
            // Check for the correct response structure: response["player_profile"]["Player"]
            if (response["player_profile"] == null)
            {
                Debug.LogError($"[AutoEquipUtility] ❌ Server response missing 'player_profile' field for {equipmentName}. Response keys: {string.Join(", ", response.Properties().Select(p => p.Name))}");
                return;
            }
            
            if (response["player_profile"]["Player"] == null)
            {
                Debug.LogError($"[AutoEquipUtility] ❌ Server response missing 'Player' field in player_profile for {equipmentName}. player_profile keys: {string.Join(", ", response["player_profile"].Cast<JProperty>().Select(p => p.Name))}");
                return;
            }
            
            try
            {
                PlayerProfile.Data.SetPlayer(response["player_profile"]["Player"].ToObject<Player>());
                Debug.Log($"[AutoEquipUtility] ✅ Player profile updated successfully from server response for {equipmentName}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AutoEquipUtility] ❌ Error updating player profile for {equipmentName}: {ex.Message}");
            }
        }
    }
}