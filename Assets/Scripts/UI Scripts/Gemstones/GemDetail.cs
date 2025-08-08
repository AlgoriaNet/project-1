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
        [SerializeField] private Image partImage;
        [SerializeField] private TextMeshProUGUI gemName;
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
            gemName.text = gemstone.LevelName ?? "Unknown Gem";
            icon.sprite = Resources.Load<Sprite>($"UILoading/Gem/Stone/Gem_{gemstone.Level:D2}");
            description.text = gemstone.EffectDescription ?? "No description available";
            level.text = gemstone.Level.ToString();
            part.text = gemstone.Part;
            
            // Load and display the part image (like in pack display)
            if (partImage != null)
            {
                Sprite partSprite = Resources.Load<Sprite>($"UILoading/Gem/Part/{gemstone.Part}");
                if (partSprite != null)
                {
                    partImage.sprite = partSprite;
                    partImage.color = Color.white;
                }
                else
                {
                    partImage.sprite = null;
                    partImage.color = Color.clear;
                }
            }
            
            // Check if equipment exists for this gem's part and enable/disable embed button accordingly
            UpdateEmbedButtonState();
        }
        
        /// <summary>
        /// Update the embed button state based on whether equipment exists for this gem's part
        /// </summary>
        private void UpdateEmbedButtonState()
        {
            if (inlayButton == null || _gemstone == null)
                return;
                
            Equipment targetEquipment = FindEquipmentForPart(_gemstone.Part);
            bool hasEquipment = targetEquipment != null;
            
            // Enable button only if equipment exists for this part
            inlayButton.interactable = hasEquipment;
        }
        
        private void OnInlay()
        {
            // Find the equipment that matches this gem's part
            Equipment targetEquipment = FindEquipmentForPart(_gemstone.Part);
            if (targetEquipment == null)
            {
                return;
            }

            // Find empty dot slot in the equipment
            int emptySlot = FindEmptyDotSlot(targetEquipment);
            if (emptySlot == -1)
            {
                // Check if user has selected a slot for replacement
                InlayGemstones inlayComponent = GetInlayGemstonesComponent();
                if (inlayComponent != null && inlayComponent.HasSelection())
                {
                    int selectedSlot = inlayComponent.GetSelectedSlot();
                    // Replace the gem in the selected slot
                    ReplaceGemInSlot(targetEquipment, selectedSlot);
                }
                else
                {
                    // You could show a message to user here if needed
                    return;
                }
                return;
            }

            // Embed the gem in the empty slot
            EmbedGemInSlot(targetEquipment, emptySlot);
        }

        private void SetProfileFromServer(JObject obj)
        {
            var gems = obj["gems"].ToObject<List<Gemstone>>();
            PlayerProfile.Data.SetGems(gems);
        }
        
        /// <summary>
        /// Update player profile using new backend embed response format
        /// </summary>
        private void UpdatePlayerProfileFromEmbedResponse(JObject response)
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
                return emptySlot;
            }

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
        /// Get the InlayGemstones component for selection checking
        /// </summary>
        private InlayGemstones GetInlayGemstonesComponent()
        {
            if (GemDetailWithInlaid.Instance == null)
            {
                return null;
            }
            
            // Access InlayGemstones component through reflection
            var inlayField = typeof(GemDetailWithInlaid).GetField("inlayGemstones", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (inlayField != null)
            {
                return (InlayGemstones)inlayField.GetValue(GemDetailWithInlaid.Instance);
            }
            
            return null;
        }
        
        /// <summary>
        /// Replace the gem in the specified slot using the replace API directly
        /// </summary>
        private void ReplaceGemInSlot(Equipment equipment, int dotSlot)
        {
            // Get the currently embedded gem from selected slot for UI update later
            InlayGemstones inlayComponent = GetInlayGemstonesComponent();
            Gemstone currentGem = inlayComponent?.GetEmbeddedGemFromSlot(dotSlot, equipment);

            if (currentGem == null)
            {
                return;
            }

            // Update the UI immediately (optimistic update) 
            UpdateDotSlotUI(equipment.Part, dotSlot);

            // Send server request using 'replace' API action
            var apiParams = new
            {
                gemId = _gemstone.Id,
                equipmentId = equipment.Id,
                slotNumber = dotSlot
            };

            _gemApi.Action("replace", apiParams, (response) =>
            {
                // Check for success response with new backend format
                if (response["success"]?.Value<bool>() == true && response["updated_equipment"] != null)
                {
                    // Update player profile with complete response data
                    UpdatePlayerProfileFromEmbedResponse(response);

                    // Update GemDetail display with the gem that was replaced (now in inventory)
                    UpdateGemDetailDisplay(currentGem);

                    // DIRECT UPDATE: Immediately update the InlayGems slot with the new gem
                    UpdateInlayGemSlotDirectly(dotSlot, _gemstone);

                    // REFRESH: Update the InlayGemstones display to show the correct equipment
                    RefreshInlayGemstonesUI(equipment.Part);

                    // Clear selection after successful replacement
                    if (inlayComponent != null)
                    {
                        inlayComponent.ClearSelection();
                    }

                    // Use a small delay to ensure data updates are processed before UI refresh
                    if (gameObject.activeInHierarchy)
                    {
                        StartCoroutine(DelayedInventoryRefresh());
                    }
                }
                else
                {
                    string error = response["error"]?.Value<string>() ?? "Unknown error";
                    
                    // Revert UI changes on failure
                    RevertDotSlotUI(equipment.Part, dotSlot);
                }
            }, (errorResponse) =>
            {
                // Revert UI changes on error
                RevertDotSlotUI(equipment.Part, dotSlot);
            });
        }
        
        /// <summary>
        /// Update the GemDetail display to show the newly acquired gem (the one that was replaced)
        /// </summary>
        private void UpdateGemDetailDisplay(Gemstone newGem)
        {
            // Update the gem status to reflect it's now in inventory (not embedded)
            newGem.IsEmbedded = false;
            newGem.IsInInventory = true;
            newGem.EquipmentId = null;
            newGem.SlotNumber = null;

            // Update the current GemDetail instance to show the gem that was just removed/replaced
            // This makes it appear in the left panel as if the user now has this gem
            Init(newGem, _sidekickId);
        }
        
        /// <summary>
        /// Embed the gem in the specified slot
        /// </summary>
        private void EmbedGemInSlot(Equipment equipment, int dotSlot)
        {
            // Update the UI immediately (optimistic update)
            UpdateDotSlotUI(equipment.Part, dotSlot);

            // Send server request using new backend API format
            var apiParams = new
            {
                gemId = _gemstone.Id,
                equipmentId = equipment.Id,  // ✅ Use equipment ID instead of part
                slotNumber = dotSlot         // ✅ Use slotNumber instead of dotSlot
            };

            _gemApi.Action("inlay", apiParams, (response) =>
            {
                // Check for success response with new backend format
                if (response["success"]?.Value<bool>() == true && response["updated_equipment"] != null)
                {
                    // Update player profile with complete response data
                    UpdatePlayerProfileFromEmbedResponse(response);

                    // DIRECT UPDATE: Immediately update the InlayGems slot with the embedded gem
                    UpdateInlayGemSlotDirectly(dotSlot, _gemstone);

                    // REFRESH: Update the InlayGemstones display to show the correct equipment
                    RefreshInlayGemstonesUI(equipment.Part);

                    // Use a small delay to ensure data updates are processed before UI refresh
                    if (gameObject.activeInHierarchy)
                    {
                        StartCoroutine(DelayedInventoryRefresh());
                    }
                }
                else
                {
                    string error = response["error"]?.Value<string>() ?? "Unknown error";
                    
                    // Revert UI changes on failure
                    RevertDotSlotUI(equipment.Part, dotSlot);
                }
            }, (errorResponse) =>
            {
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
            if (GemDetailWithInlaid.Instance == null)
            {
                return;
            }

            // Access InlayGemstones directly from GemDetailWithInlaid serialized field
            var inlayField = typeof(GemDetailWithInlaid).GetField("inlayGemstones", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (inlayField == null)
            {
                return;
            }

            InlayGemstones inlayComponent = (InlayGemstones)inlayField.GetValue(GemDetailWithInlaid.Instance);
            if (inlayComponent == null)
            {
                return;
            }

            // Use the connected arrays directly (slotNumber is 1-based, array is 0-based)
            int arrayIndex = slotNumber - 1;
            if (arrayIndex < 0 || arrayIndex >= inlayComponent.images.Count)
            {
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
            }
        }
        
        /// <summary>
        /// Refresh the InlayGemstones UI immediately after embedding
        /// </summary>
        private void RefreshInlayGemstonesUI(string equipmentPart)
        {
            if (GemDetailWithInlaid.Instance != null)
            {
                if (GemDetailWithInlaid.Instance.gameObject.activeInHierarchy)
                {
                    // Use the new public method to refresh InlayGemstones
                    GemDetailWithInlaid.Instance.RefreshInlayGemstones(equipmentPart, _sidekickId);
                }
            }
        }
        
        /// <summary>
        /// Refresh the pack UI after embedding
        /// </summary>
        private void RefreshPackUI()
        {
            // Find and refresh the appropriate block setup
            if (_sidekickId != null)
            {
                AlliesBlockSetup alliesBlockSetup = FindObjectOfType<AlliesBlockSetup>();
                if (alliesBlockSetup != null)
                {
                    alliesBlockSetup.UpdateTotalBlocks();
                }
            }
            else
            {
                HeroBlockSetup heroBlockSetup = FindObjectOfType<HeroBlockSetup>();
                if (heroBlockSetup != null)
                {
                    heroBlockSetup.UpdateTotalBlocks();
                }
            }
        }
    }
}