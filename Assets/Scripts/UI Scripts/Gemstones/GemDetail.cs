using System;
using System.Collections.Generic;
using System.Globalization;
using model;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WebSocket;

namespace PimDeWitte.UnityMainThreadDispatcher.UI_Scripts.Gemstones
{
    public class GemDetail : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI name;
        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private TextMeshProUGUI level;
        [SerializeField] private TextMeshProUGUI part;
        [SerializeField] Button inlayButton;
        private Gemstone _gemstone;
        private int? _sidekickId;
        private GemWebSocketApi _gemApi;

        public void Awake()
        {
            _gemApi = GemWebSocketApi.Instance;
            inlayButton.onClick.AddListener(OnInlay);
        }

        public void Init(Gemstone gemstone, int? sidekickId)
        {
            _gemstone = gemstone;
            _sidekickId = sidekickId;
            name.text = gemstone.LevelName ?? "Unknown Gem";
            icon.sprite = Resources.Load<Sprite>($"UILoading/Gem/Stone/Gem_{gemstone.Level:D2}");
            description.text = gemstone.EffectDescription ?? "No description available";
            level.text = gemstone.Level.ToString();
            // value field removed - no longer needed since description is descriptive
            part.text = gemstone.Part;
        }
        
        private void OnInlay()
        {
            Debug.Log($"[GemDetail] Embed button clicked for {_gemstone.EffectName} (Part: {_gemstone.Part})");
            
            // Find the equipment that matches this gem's part
            Equipment targetEquipment = FindEquipmentForPart(_gemstone.Part);
            if (targetEquipment == null)
            {
                Debug.LogWarning($"[GemDetail] No equipped {_gemstone.Part} found for embedding");
                return;
            }
            
            Debug.Log($"[GemDetail] Found target equipment: {targetEquipment.Name}");
            
            // Find empty dot slot in the equipment
            int emptySlot = FindEmptyDotSlot(targetEquipment);
            if (emptySlot == -1)
            {
                Debug.Log($"[GemDetail] All dot slots occupied in {targetEquipment.Name} - need replacement logic");
                // TODO: Implement replacement UI when all slots are occupied
                return;
            }
            
            Debug.Log($"[GemDetail] Found empty slot D{emptySlot} - embedding gem");
            
            // Embed the gem in the empty slot
            EmbedGemInSlot(targetEquipment, emptySlot);
        }

        private void SetProfileFromServer(JObject obj)
        {
            Debug.Log($"[GemDetail] SetProfileFromServer called with: {obj}");
            var gems = obj["gems"].ToObject<List<Gemstone>>();
            Debug.Log($"[GemDetail] Updating player profile with {gems?.Count ?? 0} gems");
            PlayerProfile.Data.SetGems(gems);
        }
        
        /// <summary>
        /// Update player profile using new backend embed response format
        /// </summary>
        private void UpdatePlayerProfileFromEmbedResponse(JObject response)
        {
            Debug.Log($"[GemDetail] UpdatePlayerProfileFromEmbedResponse called with: {response}");
            
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
                            Debug.Log($"[GemDetail] Updated equipment {updatedEquipment.Name} with embedded gems");
                            break;
                        }
                    }
                }
            }
            
            // Update inventory gems
            if (response["inventory_gems"] != null)
            {
                var inventoryGems = response["inventory_gems"].ToObject<List<Gemstone>>();
                Debug.Log($"[GemDetail] Updating gem inventory with {inventoryGems?.Count ?? 0} gems");
                PlayerProfile.Data.SetGems(inventoryGems);
            }
            
            // Notify listeners of data changes
            PlayerProfile.Data.NotifyListeners("Equipments");
            PlayerProfile.Data.NotifyListeners("Gemstones");
        }
        
        /// <summary>
        /// Find the equipped equipment that matches the gem's part
        /// </summary>
        private Equipment FindEquipmentForPart(string gemPart)
        {
            if (PlayerProfile.Data?.Player?.Equipments == null)
            {
                return null;
            }
            
            // Find equipment that matches the part and is equipped to the current context
            Equipment targetEquipment = null;
            
            if (_sidekickId != null)
            {
                // For Allies - find equipment equipped to this sidekick
                targetEquipment = PlayerProfile.Data.Player.Equipments.Find(equipment =>
                    equipment.Part == gemPart && equipment.EquipWithSidekickId == _sidekickId);
            }
            else
            {
                // For Hero - find equipment equipped to hero
                targetEquipment = PlayerProfile.Data.Player.Equipments.Find(equipment =>
                    equipment.Part == gemPart && equipment.EquipWithHeroId > 0);
            }
            
            return targetEquipment;
        }
        
        /// <summary>
        /// Find empty dot slot in equipment (returns 1-5, or -1 if all occupied)
        /// </summary>
        private int FindEmptyDotSlot(Equipment equipment)
        {
            // Use the new data-driven approach instead of UI manipulation
            int emptySlot = equipment.GetFirstEmptyGemSlot();
            if (emptySlot > 0)
            {
                Debug.Log($"[GemDetail] Found empty slot {emptySlot} in {equipment.Part} using equipment data");
                return emptySlot;
            }
            
            Debug.Log($"[GemDetail] No empty slots found in {equipment.Part} - all 5 slots occupied");
            return -1; // No empty slots found
        }
        
        /// <summary>
        /// Find the equipment transform in the UI hierarchy
        /// </summary>
        private Transform FindEquipmentTransform(string equipmentPart)
        {
            if (_sidekickId != null)
            {
                // For Allies - find in AlliesBlockSetup
                AlliesBlockSetup alliesBlockSetup = FindObjectOfType<AlliesBlockSetup>();
                if (alliesBlockSetup?.step2Panel != null)
                {
                    Transform gridTransform = alliesBlockSetup.step2Panel.transform.Find("Upper Group/Right Panel/Grid");
                    return FindEquipmentInGrid(gridTransform, equipmentPart);
                }
            }
            else
            {
                // For Hero - find in HeroBlockSetup
                HeroBlockSetup heroBlockSetup = FindObjectOfType<HeroBlockSetup>();
                if (heroBlockSetup?.heroStep2Panel != null)
                {
                    Transform gridTransform = heroBlockSetup.heroStep2Panel.transform.Find("Upper Group/Right Panel/Grid");
                    if (gridTransform == null)
                    {
                        // Try alternative paths
                        gridTransform = heroBlockSetup.heroStep2Panel.transform.Find("UpperGroup/RightPanel/Grid");
                        if (gridTransform == null)
                        {
                            gridTransform = heroBlockSetup.heroStep2Panel.transform.Find("Grid");
                        }
                    }
                    return FindEquipmentInGrid(gridTransform, equipmentPart);
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Find specific equipment in the grid by part name
        /// </summary>
        private Transform FindEquipmentInGrid(Transform gridTransform, string equipmentPart)
        {
            if (gridTransform == null) return null;
            
            foreach (Transform child in gridTransform)
            {
                if (child.name.Contains(equipmentPart))
                {
                    return child;
                }
                
                // Also check by image sprite name
                Transform imageTransform = child.Find("Image");
                if (imageTransform != null)
                {
                    Image image = imageTransform.GetComponent<Image>();
                    if (image?.sprite != null && image.sprite.name.StartsWith(equipmentPart))
                    {
                        return child;
                    }
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Embed the gem in the specified slot
        /// </summary>
        private void EmbedGemInSlot(Equipment equipment, int dotSlot)
        {
            Debug.Log($"[GemDetail] Embedding {_gemstone.EffectName} (ID: {_gemstone.Id}) into equipment ID {equipment.Id} slot {dotSlot}");
            
            // Gem should always be embeddable since pack filtering now excludes embedded gems
            Debug.Log($"[GemDetail] 🔍 Embedding gem {_gemstone.Id} - should be in_inventory=true, is_embedded=false");
            
            // Update the UI immediately (optimistic update)
            UpdateDotSlotUI(equipment.Part, dotSlot);
            
            // Send server request using new backend API format
            var apiParams = new
            {
                gemId = _gemstone.Id,
                equipmentId = equipment.Id,  // ✅ Use equipment ID instead of part
                slotNumber = dotSlot         // ✅ Use slotNumber instead of dotSlot
            };
            
            Debug.Log($"[GemDetail] 📤 Sending WebSocket request: {Newtonsoft.Json.JsonConvert.SerializeObject(apiParams)}");
            
            _gemApi.Action("inlay", apiParams, (response) =>
            {
                Debug.Log($"[GemDetail] 📥 Embed response received: {response}");
                
                // Check for success response with new backend format
                if (response["success"]?.Value<bool>() == true && response["updated_equipment"] != null)
                {
                    Debug.Log("[GemDetail] ✅ Gem embedded successfully using new backend API");
                    
                    // Log gem count BEFORE updating profile
                    int gemCountBefore = PlayerProfile.Data.GetGemstonesInPack()?.Count ?? 0;
                    Debug.Log($"[GemDetail] 📊 Gem count BEFORE update: {gemCountBefore}");
                    
                    // Update player profile with complete response data
                    UpdatePlayerProfileFromEmbedResponse(response);
                    
                    // Log gem count AFTER updating profile
                    int gemCountAfter = PlayerProfile.Data.GetGemstonesInPack()?.Count ?? 0;
                    Debug.Log($"[GemDetail] 📊 Gem count AFTER update: {gemCountAfter} (difference: {gemCountAfter - gemCountBefore})");
                    
                    // DIRECT UPDATE: Immediately update the InlayGems slot with the embedded gem
                    UpdateInlayGemSlotDirectly(dotSlot, _gemstone);
                    
                    // VERIFY: Check if the slot actually got updated
                    VerifySlotUpdate(dotSlot, _gemstone);
                    
                    // REFRESH: Update the InlayGemstones display to show the correct equipment
                    RefreshInlayGemstonesUI(equipment.Part);
                    
                    // Use a small delay to ensure data updates are processed before UI refresh
                    if (gameObject.activeInHierarchy)
                    {
                        StartCoroutine(DelayedInventoryRefresh());
                    }
                    
                    Debug.Log("[GemDetail] ✅ Embedding complete - slot updated directly, inlay refreshed, inventory refreshing");
                }
                else
                {
                    string error = response["error"]?.Value<string>() ?? "Unknown error";
                    Debug.LogError($"[GemDetail] ❌ Embed failed: {error}");
                    
                    // Revert UI changes on failure
                    RevertDotSlotUI(equipment.Part, dotSlot);
                }
            }, (errorResponse) =>
            {
                Debug.LogError($"[GemDetail] 💥 WebSocket error callback: {errorResponse}");
                
                // Revert UI changes on error
                RevertDotSlotUI(equipment.Part, dotSlot);
            });
        }
        
        /// <summary>
        /// Update the dot slot UI with gem image
        /// </summary>
        private void UpdateDotSlotUI(string equipmentPart, int dotSlot)
        {
            Transform equipmentTransform = FindEquipmentTransform(equipmentPart);
            if (equipmentTransform == null) return;
            
            Transform dotsTransform = equipmentTransform.Find("Dots");
            if (dotsTransform == null) return;
            
            Transform dotTransform = dotsTransform.Find($"D{dotSlot}");
            if (dotTransform == null) return;
            
            Image dotImage = dotTransform.GetComponent<Image>();
            if (dotImage != null)
            {
                // Load dot sprite (Gem_01 -> Dot_01, etc.)
                string dotSpriteName = $"Dot_{_gemstone.Level:D2}";
                Sprite dotSprite = Resources.Load<Sprite>($"UILoading/Gem/Dots/{dotSpriteName}");
                
                if (dotSprite != null)
                {
                    dotImage.sprite = dotSprite;
                    dotImage.color = Color.white;
                    Debug.Log($"[GemDetail] Updated UI: {equipmentPart} D{dotSlot} with {dotSpriteName}");
                }
                else
                {
                    Debug.LogWarning($"[GemDetail] Could not load dot sprite: {dotSpriteName}");
                }
            }
        }
        
        /// <summary>
        /// Revert dot slot UI changes
        /// </summary>
        private void RevertDotSlotUI(string equipmentPart, int dotSlot)
        {
            Transform equipmentTransform = FindEquipmentTransform(equipmentPart);
            if (equipmentTransform == null) return;
            
            Transform dotsTransform = equipmentTransform.Find("Dots");
            if (dotsTransform == null) return;
            
            Transform dotTransform = dotsTransform.Find($"D{dotSlot}");
            if (dotTransform == null) return;
            
            Image dotImage = dotTransform.GetComponent<Image>();
            if (dotImage != null)
            {
                dotImage.sprite = null;
                dotImage.color = Color.clear;
                Debug.Log($"[GemDetail] Reverted UI: {equipmentPart} D{dotSlot}");
            }
        }
        
        /// <summary>
        /// Delayed refresh to ensure UI updates after dot changes
        /// </summary>
        private System.Collections.IEnumerator DelayedRefreshInlayGemstones(string equipmentPart)
        {
            // Wait one frame for dot UI to update
            yield return null;
            RefreshInlayGemstonesUI(equipmentPart);
        }
        
        /// <summary>
        /// Delayed close popup and refresh pack to allow InlayGemstones to update first
        /// </summary>
        private System.Collections.IEnumerator DelayedInventoryRefresh()
        {
            // Small delay to ensure data updates are processed
            yield return new WaitForSeconds(0.1f);
            
            // Refresh the pack UI to remove the embedded gem from inventory
            RefreshPackUI();
            
            Debug.Log("[GemDetail] ✅ Inventory refreshed - embedded gem removed from pack");
        }
        
        private System.Collections.IEnumerator DelayedClosePopupAndRefresh()
        {
            // Wait a moment for InlayGemstones UI to refresh
            yield return new WaitForSeconds(0.1f);
            
            // Close the gem detail popup
            if (GemDetailWithInlaid.Instance != null)
            {
                GemDetailWithInlaid.Instance.gameObject.SetActive(false);
            }
            
            // Delay pack UI refresh to avoid immediate recreation of gem elements (which makes them unclickable)
            yield return new WaitForSeconds(0.5f);
            
            // Refresh the pack UI to remove the embedded gem
            RefreshPackUI();
        }
        
        /// <summary>
        /// DIRECT UPDATE: Use InlayGemstones component arrays to update slot immediately
        /// </summary>
        private void UpdateInlayGemSlotDirectly(int slotNumber, Gemstone gem)
        {
            Debug.Log($"[GemDetail] UpdateInlayGemSlotDirectly called for slot {slotNumber}, gem: {gem?.EffectName}");
            
            if (GemDetailWithInlaid.Instance == null)
            {
                Debug.LogError("[GemDetail] GemDetailWithInlaid.Instance is null");
                return;
            }
            
            // Access InlayGemstones directly from GemDetailWithInlaid serialized field
            // Need to access the private inlayGemstones field through reflection or make it public
            var inlayField = typeof(GemDetailWithInlaid).GetField("inlayGemstones", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (inlayField == null)
            {
                Debug.LogError("[GemDetail] inlayGemstones field not found in GemDetailWithInlaid");
                return;
            }
            
            InlayGemstones inlayComponent = (InlayGemstones)inlayField.GetValue(GemDetailWithInlaid.Instance);
            if (inlayComponent == null)
            {
                Debug.LogError("[GemDetail] inlayGemstones field is null");
                return;
            }
            
            // Use the connected arrays directly (slotNumber is 1-based, array is 0-based)
            int arrayIndex = slotNumber - 1;
            if (arrayIndex < 0 || arrayIndex >= inlayComponent.images.Count)
            {
                Debug.LogError($"[GemDetail] Invalid slot number {slotNumber}, array size: {inlayComponent.images.Count}");
                return;
            }
            
            // Load gem sprite directly
            string gemSpriteName = $"Gem_{gem.Level:D2}";
            Sprite gemSprite = Resources.Load<Sprite>($"UILoading/Gem/Stone/{gemSpriteName}");
            
            if (gemSprite != null)
            {
                // Update the connected Image component directly via the array
                inlayComponent.images[arrayIndex].sprite = gemSprite;
                
                // Update description if available
                if (arrayIndex < inlayComponent.descriptions.Count)
                {
                    inlayComponent.descriptions[arrayIndex].text = gem.EffectDescription ?? $"Gem Level {gem.Level}";
                }
                
                Debug.Log($"[GemDetail] ✅ UPDATED slot {slotNumber} with {gemSpriteName} using component arrays");
            }
            else
            {
                Debug.LogError($"[GemDetail] Could not load sprite: UILoading/Gem/Stone/{gemSpriteName}");
            }
        }
        
        /// <summary>
        /// Verify if the slot UI actually got updated with the gem image
        /// </summary>
        private void VerifySlotUpdate(int slotNumber, Gemstone gem)
        {
            Debug.Log($"[GemDetail] 🔍 Verifying slot {slotNumber} update for gem: {gem?.EffectName}");
            
            try
            {
                var inlayField = typeof(GemDetailWithInlaid).GetField("inlayGemstones", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (inlayField != null)
                {
                    InlayGemstones inlayComponent = (InlayGemstones)inlayField.GetValue(GemDetailWithInlaid.Instance);
                    if (inlayComponent != null)
                    {
                        int arrayIndex = slotNumber - 1;
                        if (arrayIndex >= 0 && arrayIndex < inlayComponent.images.Count)
                        {
                            bool hasSprite = inlayComponent.images[arrayIndex].sprite != null;
                            string spriteName = hasSprite ? inlayComponent.images[arrayIndex].sprite.name : "null";
                            Debug.Log($"[GemDetail] 🔍 Slot {slotNumber} sprite status: {(hasSprite ? "HAS SPRITE" : "NO SPRITE")} - {spriteName}");
                            
                            if (hasSprite)
                            {
                                Debug.Log($"[GemDetail] ✅ VERIFICATION SUCCESS: Slot {slotNumber} has sprite {spriteName}");
                            }
                            else
                            {
                                Debug.LogError($"[GemDetail] ❌ VERIFICATION FAILED: Slot {slotNumber} has no sprite!");
                            }
                        }
                        else
                        {
                            Debug.LogError($"[GemDetail] ❌ VERIFICATION FAILED: Invalid array index {arrayIndex}");
                        }
                    }
                    else
                    {
                        Debug.LogError("[GemDetail] ❌ VERIFICATION FAILED: inlayComponent is null");
                    }
                }
                else
                {
                    Debug.LogError("[GemDetail] ❌ VERIFICATION FAILED: inlayGemstones field not found");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[GemDetail] ❌ VERIFICATION ERROR: {e.Message}");
            }
        }
        
        /// <summary>
        /// Refresh the InlayGemstones UI immediately after embedding
        /// </summary>
        private void RefreshInlayGemstonesUI(string equipmentPart)
        {
            Debug.Log($"[GemDetail] RefreshInlayGemstonesUI called for {equipmentPart}");
            
            if (GemDetailWithInlaid.Instance != null)
            {
                Debug.Log($"[GemDetail] GemDetailWithInlaid.Instance found, active: {GemDetailWithInlaid.Instance.gameObject.activeInHierarchy}");
                
                if (GemDetailWithInlaid.Instance.gameObject.activeInHierarchy)
                {
                    // Use the new public method to refresh InlayGemstones
                    Debug.Log($"[GemDetail] Calling RefreshInlayGemstones on GemDetailWithInlaid.Instance");
                    GemDetailWithInlaid.Instance.RefreshInlayGemstones(equipmentPart, _sidekickId);
                }
                else
                {
                    Debug.LogWarning($"[GemDetail] GemDetailWithInlaid is not active, cannot refresh InlayGemstones");
                }
            }
            else
            {
                Debug.LogWarning($"[GemDetail] GemDetailWithInlaid.Instance is null, cannot refresh InlayGemstones");
            }
        }
        
        /// <summary>
        /// Refresh the pack UI after embedding
        /// </summary>
        private void RefreshPackUI()
        {
            Debug.Log($"[GemDetail] RefreshPackUI called for sidekickId: {_sidekickId}");
            
            // Find and refresh the appropriate block setup
            if (_sidekickId != null)
            {
                AlliesBlockSetup alliesBlockSetup = FindObjectOfType<AlliesBlockSetup>();
                if (alliesBlockSetup != null)
                {
                    Debug.Log($"[GemDetail] Refreshing AlliesBlockSetup pack UI");
                    alliesBlockSetup.UpdateTotalBlocks();
                }
                else
                {
                    Debug.LogWarning($"[GemDetail] AlliesBlockSetup not found");
                }
            }
            else
            {
                HeroBlockSetup heroBlockSetup = FindObjectOfType<HeroBlockSetup>();
                if (heroBlockSetup != null)
                {
                    Debug.Log($"[GemDetail] Refreshing HeroBlockSetup pack UI");
                    heroBlockSetup.UpdateTotalBlocks();
                }
                else
                {
                    Debug.LogWarning($"[GemDetail] HeroBlockSetup not found");
                }
            }
        }
    }
}