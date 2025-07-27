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
        [SerializeField] private TextMeshProUGUI value;
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
            name.text = gemstone.Name;
            icon.sprite = Resources.Load<Sprite>($"UILoading/Gem/Stone/Gem_{gemstone.Level:D2}");
            description.text = gemstone.Description;
            level.text = gemstone.Level.ToString();
            value.text = gemstone.EntryValue.ToString(CultureInfo.InvariantCulture);
            part.text = gemstone.Part;
        }
        
        private void OnInlay()
        {
            Debug.Log($"[GemDetail] Embed button clicked for {_gemstone.Name} (Part: {_gemstone.Part})");
            
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
            // Find the equipment UI element to check dots
            Transform equipmentTransform = FindEquipmentTransform(equipment.Part);
            if (equipmentTransform == null)
            {
                Debug.LogWarning($"[GemDetail] Could not find equipment transform for {equipment.Part}");
                return -1;
            }
            
            Transform dotsTransform = equipmentTransform.Find("Dots");
            if (dotsTransform == null)
            {
                Debug.LogWarning($"[GemDetail] No dots found in {equipment.Part}");
                return -1;
            }
            
            // Check each dot slot (D1 through D5) - empty slots have Dot_00
            for (int i = 1; i <= 5; i++)
            {
                Transform dotTransform = dotsTransform.Find($"D{i}");
                if (dotTransform != null)
                {
                    Image dotImage = dotTransform.GetComponent<Image>();
                    if (dotImage != null && dotImage.sprite != null)
                    {
                        // Check if it's the default empty slot (Dot_00)
                        if (dotImage.sprite.name == "Dot_00")
                        {
                            Debug.Log($"[GemDetail] Found empty slot D{i} (Dot_00) in {equipment.Part}");
                            return i;
                        }
                    }
                }
            }
            
            return -1; // All slots occupied
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
            Debug.Log($"[GemDetail] Embedding {_gemstone.Name} into {equipment.Part} slot D{dotSlot}");
            
            // Update the UI immediately (optimistic update)
            UpdateDotSlotUI(equipment.Part, dotSlot);
            
            // Send server request
            var apiParams = new
            {
                gemId = _gemstone.Id,
                equipmentPart = equipment.Part,
                dotSlot = dotSlot,
                sidekickId = _sidekickId,
            };
            
            _gemApi.Action("inlay", apiParams, (response) =>
            {
                Debug.Log($"[GemDetail] Embed response: {response}");
                
                // Check if response contains gem data (success case)
                if (response.Type == Newtonsoft.Json.Linq.JTokenType.Array || response["gems"] != null)
                {
                    Debug.Log("[GemDetail] ✅ Gem embedded successfully");
                    
                    // Update player profile using existing method
                    SetProfileFromServer(response);
                    
                    // Refresh the InlayGemstones UI first, then close popup with delay
                    RefreshInlayGemstonesUI(equipment.Part);
                    
                    // Use coroutine to delay popup close and pack refresh
                    StartCoroutine(DelayedClosePopupAndRefresh());
                }
                else
                {
                    string error = response["error"]?.Value<string>() ?? "Unknown error";
                    Debug.LogError($"[GemDetail] ❌ Embed failed: {error}");
                    
                    // Revert UI changes on failure
                    RevertDotSlotUI(equipment.Part, dotSlot);
                }
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