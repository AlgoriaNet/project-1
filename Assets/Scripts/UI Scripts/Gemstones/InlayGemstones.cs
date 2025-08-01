using System.Collections.Generic;
using model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace PimDeWitte.UnityMainThreadDispatcher.UI_Scripts.Gemstones
{
    public class InlayGemstones : MonoBehaviour
    {
        private List<Gemstone> _gemstones;
        [SerializeField] public List<Image> images;
        [SerializeField] public List<TextMeshProUGUI> descriptions;
        
        // Selection tracking
        private int _selectedSlot = -1; // -1 means no selection, 1-5 for selected slots
        private Color _originalColor = Color.white;
        private Color _selectedColor = new Color(1f, 1f, 0f, 1f); // Yellow highlight
        
        
        public void Init(List<Gemstone> gemstones)
        {
            _gemstones = gemstones;
            for (var i = 0; i < images.Count; i++)
            {
                if (i < gemstones.Count)
                {
                    images[i].sprite = Resources.Load<Sprite>($"UILoading/Gem/Stone/Gem_{gemstones[i].Level:D2}");
                    descriptions[i].text = gemstones[i].EffectDescription;
                }
                else
                {
                    images[i].sprite = null;
                    descriptions[i].text = "";
                }
            }
        }
        
        /// <summary>
        /// Initialize using equipment data instead of UI parsing (NEW DATA-DRIVEN APPROACH)
        /// </summary>
        public void InitFromEquipmentData(Equipment equipment)
        {
            Debug.Log($"[InlayGemstones] InitFromEquipmentData called for equipment: {equipment?.Name}");
            
            // Store current selection to restore it after data refresh
            int previousSelection = _selectedSlot;
            
            // Clear selection temporarily for data refresh
            ClearSelection();
            
            if (equipment?.EmbeddedGems == null)
            {
                Debug.Log($"[InlayGemstones] No embedded gems data for equipment {equipment?.Name}");
                ClearAllSlots();
                return;
            }
            
            // Update UI from equipment embedded gems data
            for (int i = 0; i < 5; i++) // Always 5 slots
            {
                if (i < images.Count)
                {
                    if (i < equipment.EmbeddedGems.Count && !equipment.EmbeddedGems[i].is_empty && equipment.EmbeddedGems[i].gem != null)
                    {
                        // Slot has gem - display it
                        var gem = equipment.EmbeddedGems[i].gem;
                        images[i].sprite = Resources.Load<Sprite>($"UILoading/Gem/Stone/Gem_{gem.Level:D2}");
                        descriptions[i].text = gem.EffectDescription ?? $"Gem Level {gem.Level}";
                        Debug.Log($"[InlayGemstones] Loaded embedded gem: {gem.EffectName} for slot {i + 1}");
                    }
                    else
                    {
                        // Empty slot
                        images[i].sprite = null;
                        descriptions[i].text = "";
                        Debug.Log($"[InlayGemstones] Slot {i + 1} is empty");
                    }
                }
            }
            
            // Restore previous selection if it was valid and slot still has a gem
            if (previousSelection > 0 && HasGemInSlot(previousSelection))
            {
                Debug.Log($"[InlayGemstones] Restoring selection for slot {previousSelection}");
                SelectSlot(previousSelection);
            }
        }
        
        private void ClearAllSlots()
        {
            // Clear any existing selection
            ClearSelection();
            
            for (int i = 0; i < images.Count; i++)
            {
                images[i].sprite = null;
                descriptions[i].text = "";
            }
        }
        
        /// <summary>
        /// Direct update using GameObject hierarchy - bypasses serialized arrays
        /// </summary>
        public void UpdateSlotDirectly(int slotNumber, Gemstone gem)
        {
            Debug.Log($"[InlayGemstones] UpdateSlotDirectly called for slot {slotNumber}, gem: {gem?.EffectName}");
            
            // Find the InlayGems GameObject (parent of this component)
            Transform inlayGemsTransform = transform;
            
            // Find Group_{slotNumber} (1-based)
            Transform groupTransform = inlayGemsTransform.Find($"Group_{slotNumber}");
            if (groupTransform == null)
            {
                Debug.LogError($"[InlayGemstones] Could not find Group_{slotNumber} in InlayGems");
                return;
            }
            
            // Find Image under Group_{slotNumber}
            Transform imageTransform = groupTransform.Find("Image");
            if (imageTransform == null)
            {
                Debug.LogError($"[InlayGemstones] Could not find Image under Group_{slotNumber}");
                return;
            }
            
            Image slotImage = imageTransform.GetComponent<Image>();
            if (slotImage == null)
            {
                Debug.LogError($"[InlayGemstones] No Image component found on Group_{slotNumber}/Image");
                return;
            }
            
            if (gem != null)
            {
                // Load and set gem sprite
                Sprite gemSprite = Resources.Load<Sprite>($"UILoading/Gem/Stone/Gem_{gem.Level:D2}");
                slotImage.sprite = gemSprite;
                Debug.Log($"[InlayGemstones] ✅ Updated slot {slotNumber} with gem {gem.EffectName} (Level {gem.Level})");
                
                // Also update description if there's a text component
                Transform textTransform = groupTransform.Find("Text");
                if (textTransform != null)
                {
                    var textComponent = textTransform.GetComponent<TMPro.TextMeshProUGUI>();
                    if (textComponent != null)
                    {
                        textComponent.text = gem.EffectDescription ?? $"Gem Level {gem.Level}";
                    }
                }
            }
            else
            {
                // Clear slot
                slotImage.sprite = null;
                Debug.Log($"[InlayGemstones] ✅ Cleared slot {slotNumber}");
            }
        }
        
        /// <summary>
        /// Setup click detection for all gem slot groups
        /// </summary>
        private void Start()
        {
            SetupSlotClickDetection();
        }
        
        /// <summary>
        /// Setup click detection for Group_1 through Group_5 using EventTrigger instead of Button
        /// </summary>
        private void SetupSlotClickDetection()
        {
            for (int i = 1; i <= 5; i++)
            {
                Transform groupTransform = transform.Find($"Group_{i}");
                if (groupTransform != null)
                {
                    // Remove any existing Button component that causes orange appearance
                    Button existingButton = groupTransform.GetComponent<Button>();
                    if (existingButton != null)
                    {
                        DestroyImmediate(existingButton);
                    }
                    
                    // Add EventTrigger for click detection without visual changes
                    UnityEngine.EventSystems.EventTrigger trigger = groupTransform.GetComponent<UnityEngine.EventSystems.EventTrigger>();
                    if (trigger == null)
                    {
                        trigger = groupTransform.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
                    }
                    
                    // Clear existing triggers
                    trigger.triggers.Clear();
                    
                    // Add click trigger
                    UnityEngine.EventSystems.EventTrigger.Entry entry = new UnityEngine.EventSystems.EventTrigger.Entry();
                    entry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerClick;
                    
                    // Capture slot number for closure
                    int slotNumber = i;
                    entry.callback.AddListener((eventData) => OnSlotClicked(slotNumber));
                    
                    trigger.triggers.Add(entry);
                    
                    Debug.Log($"[InlayGemstones] Setup click detection for Group_{i}");
                }
                else
                {
                    Debug.LogWarning($"[InlayGemstones] Group_{i} not found in hierarchy");
                }
            }
        }
        
        /// <summary>
        /// Handle gem slot click for selection
        /// </summary>
        private void OnSlotClicked(int slotNumber)
        {
            Debug.Log($"[InlayGemstones] Slot {slotNumber} clicked");
            
            // Check if this slot has a gem (only allow selection of occupied slots)
            if (HasGemInSlot(slotNumber))
            {
                // Toggle selection
                if (_selectedSlot == slotNumber)
                {
                    // Deselect current slot
                    ClearSelection();
                }
                else
                {
                    // Select new slot
                    SelectSlot(slotNumber);
                }
            }
            else
            {
                Debug.Log($"[InlayGemstones] Cannot select empty slot {slotNumber}");
            }
        }
        
        /// <summary>
        /// Check if a slot has a gem
        /// </summary>
        private bool HasGemInSlot(int slotNumber)
        {
            int arrayIndex = slotNumber - 1;
            if (arrayIndex >= 0 && arrayIndex < images.Count)
            {
                return images[arrayIndex].sprite != null;
            }
            return false;
        }
        
        /// <summary>
        /// Select a gem slot and provide visual feedback
        /// </summary>
        private void SelectSlot(int slotNumber)
        {
            // Clear previous selection
            ClearSelection();
            
            _selectedSlot = slotNumber;
            
            // Apply visual highlight by adding background to the Group
            Transform groupTransform = transform.Find($"Group_{slotNumber}");
            if (groupTransform != null)
            {
                // Get or create background Image on the Group itself
                Image groupBackground = groupTransform.GetComponent<Image>();
                if (groupBackground == null)
                {
                    groupBackground = groupTransform.gameObject.AddComponent<Image>();
                }
                
                // Set semi-transparent yellow background
                groupBackground.color = new Color(1f, 1f, 0f, 0.4f); // Yellow background
                Debug.Log($"[InlayGemstones] Selected slot {slotNumber} - applied yellow background to group");
            }
        }
        
        /// <summary>
        /// Clear current selection and visual feedback
        /// </summary>
        public void ClearSelection()
        {
            // Clear ALL group backgrounds, not just the previously selected one
            for (int i = 1; i <= 5; i++)
            {
                Transform groupTransform = transform.Find($"Group_{i}");
                if (groupTransform != null)
                {
                    Image groupBackground = groupTransform.GetComponent<Image>();
                    if (groupBackground != null)
                    {
                        // Make background transparent
                        groupBackground.color = new Color(0, 0, 0, 0);
                    }
                }
            }
            
            if (_selectedSlot > 0)
            {
                Debug.Log($"[InlayGemstones] Cleared selection from slot {_selectedSlot}");
            }
            
            _selectedSlot = -1;
        }
        
        /// <summary>
        /// Get the currently selected slot number (1-5, or -1 if none selected)
        /// </summary>
        public int GetSelectedSlot()
        {
            return _selectedSlot;
        }
        
        /// <summary>
        /// Check if any slot is currently selected
        /// </summary>
        public bool HasSelection()
        {
            return _selectedSlot > 0;
        }
        
        /// <summary>
        /// Get the embedded gem from a specific slot using equipment data
        /// </summary>
        public Gemstone GetEmbeddedGemFromSlot(int slotNumber, Equipment equipment)
        {
            if (equipment?.EmbeddedGems == null || slotNumber < 1 || slotNumber > 5)
            {
                return null;
            }
            
            int arrayIndex = slotNumber - 1;
            if (arrayIndex < equipment.EmbeddedGems.Count && 
                !equipment.EmbeddedGems[arrayIndex].is_empty && 
                equipment.EmbeddedGems[arrayIndex].gem != null)
            {
                return equipment.EmbeddedGems[arrayIndex].gem;
            }
            
            return null;
        }
        
        /// <summary>
        /// Initialize using dot-reading method (copied from successful page4 logic)
        /// </summary>
        public void InitFromDots(string equipmentPart, int? sidekickId = null)
        {
            Debug.Log($"[InlayGemstones] InitFromDots called for part: {equipmentPart}, sidekickId: {sidekickId}");
            
            // Find the Step2Panel (equipment grid) - same path as in old page4 logic
            Transform gridTransform = null;
            
            if (sidekickId != null)
            {
                // For Allies - find AlliesBlockSetup and use its step2Panel (exactly like page4)
                AlliesBlockSetup alliesBlockSetup = FindObjectOfType<AlliesBlockSetup>();
                if (alliesBlockSetup != null && alliesBlockSetup.step2Panel != null)
                {
                    gridTransform = alliesBlockSetup.step2Panel.transform.Find("Upper Group/Right Panel/Grid");
                }
            }
            else
            {
                // For Hero - find HeroBlockSetup equivalent
                HeroBlockSetup heroBlockSetup = FindObjectOfType<HeroBlockSetup>();
                if (heroBlockSetup != null && heroBlockSetup.heroStep2Panel != null)
                {
                    Debug.Log($"[InlayGemstones] Found HeroBlockSetup, heroStep2Panel active: {heroBlockSetup.heroStep2Panel.activeInHierarchy}");
                    
                    // Try the same pattern as Allies but for Hero
                    gridTransform = heroBlockSetup.heroStep2Panel.transform.Find("Upper Group/Right Panel/Grid");
                    if (gridTransform == null)
                    {
                        // Try alternative Hero grid path
                        gridTransform = heroBlockSetup.heroStep2Panel.transform.Find("UpperGroup/RightPanel/Grid");
                        Debug.Log($"[InlayGemstones] Tried UpperGroup/RightPanel/Grid: {gridTransform != null}");
                    }
                    if (gridTransform == null)
                    {
                        // Try another alternative Hero grid path
                        gridTransform = heroBlockSetup.heroStep2Panel.transform.Find("Grid");
                        Debug.Log($"[InlayGemstones] Tried Grid: {gridTransform != null}");
                    }
                    if (gridTransform == null)
                    {
                        // Debug: List all child objects to find correct path
                        Debug.Log($"[InlayGemstones] Available children in heroStep2Panel:");
                        foreach (Transform child in heroBlockSetup.heroStep2Panel.transform)
                        {
                            Debug.Log($"  - {child.name}");
                            foreach (Transform grandchild in child)
                            {
                                Debug.Log($"    - {grandchild.name}");
                                foreach (Transform greatgrandchild in grandchild)
                                {
                                    Debug.Log($"      - {greatgrandchild.name}");
                                }
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogError($"[InlayGemstones] HeroBlockSetup not found or heroStep2Panel is null");
                }
                
                if (gridTransform == null)
                {
                    Debug.LogWarning("[InlayGemstones] Hero equipment grid not found - tried multiple paths");
                    return;
                }
            }
            
            if (gridTransform == null)
            {
                Debug.LogError("[InlayGemstones] Could not find equipment grid transform");
                return;
            }
            
            // Find the matching equipment block (same logic as page4)
            foreach (Transform block in gridTransform)
            {
                Image gridBlockImage = block.Find("Image")?.GetComponent<Image>();
                
                if (gridBlockImage != null && gridBlockImage.sprite != null && 
                    gridBlockImage.sprite.name.StartsWith(equipmentPart))
                {
                    Debug.Log($"[InlayGemstones] Found matching block: {gridBlockImage.sprite.name}");
                    
                    // Read the color dots (exactly like page4 logic)
                    Transform dotsTransform = block.Find("Dots");
                    if (dotsTransform != null)
                    {
                        Debug.Log($"[InlayGemstones] Found dots under block: {block.name}");
                        
                        for (int i = 1; i <= 5; i++)
                        {
                            string dotName = $"D{i}";
                            Transform dotTransform = dotsTransform.Find(dotName);
                            
                            if (dotTransform != null)
                            {
                                Image dotImage = dotTransform.GetComponent<Image>();
                                if (dotImage != null && dotImage.sprite != null)
                                {
                                    Debug.Log($"[InlayGemstones] Found {dotName} Image File: {dotImage.sprite.name}");
                                    
                                    // Check if we have enough image slots
                                    if (i-1 < images.Count)
                                    {
                                        if (dotImage.sprite.name == "Dot_00")
                                        {
                                            // Empty slot - clear image and description
                                            images[i-1].sprite = null;
                                            descriptions[i-1].text = "";
                                            Debug.Log($"[InlayGemstones] Slot {i} is empty (Dot_00)");
                                        }
                                        else
                                        {
                                            // Convert dot to gem (same logic as page4)
                                            string gemImageFileName = dotImage.sprite.name.Replace("Dot", "Gem");
                                            
                                            // Load gem sprite from correct path
                                            Sprite gemSprite = Resources.Load<Sprite>($"UILoading/Gem/Stone/{gemImageFileName}");
                                            if (gemSprite != null)
                                            {
                                                images[i-1].sprite = gemSprite;
                                                descriptions[i-1].text = $"Gem Level {gemImageFileName.Replace("Gem_", "")}";
                                                Debug.Log($"[InlayGemstones] Loaded gem: {gemImageFileName} for slot {i}");
                                            }
                                            else
                                            {
                                                // Try alternative path if first fails
                                                gemSprite = Resources.Load<Sprite>($"UILoading/Gem/{gemImageFileName}");
                                                if (gemSprite != null)
                                                {
                                                    images[i-1].sprite = gemSprite;
                                                    descriptions[i-1].text = $"Gem Level {gemImageFileName.Replace("Gem_", "")}";
                                                    Debug.Log($"[InlayGemstones] Loaded gem from alternative path: {gemImageFileName} for slot {i}");
                                                }
                                                else
                                                {
                                                    Debug.LogWarning($"[InlayGemstones] Could not load gem sprite: {gemImageFileName} from either path");
                                                    images[i-1].sprite = null;
                                                    descriptions[i-1].text = "";
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        return; // Found the matching block, exit
                    }
                    else
                    {
                        Debug.LogWarning($"[InlayGemstones] No Dots found under block: {block.name}");
                    }
                }
            }
            
            Debug.LogWarning($"[InlayGemstones] No matching equipment block found for part: {equipmentPart}");
        }
    }
}