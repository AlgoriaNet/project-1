using System.Collections.Generic;
using model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PimDeWitte.UnityMainThreadDispatcher.UI_Scripts.Gemstones
{
    public class InlayGemstones : MonoBehaviour
    {
        private List<Gemstone> _gemstones;
        [SerializeField] private List<Image> images;
        [SerializeField] private List<TextMeshProUGUI> descriptions;
        
        
        public void Init(List<Gemstone> gemstones)
        {
            _gemstones = gemstones;
            for (var i = 0; i < images.Count; i++)
            {
                if (i < gemstones.Count)
                {
                    images[i].sprite = Resources.Load<Sprite>($"UILoading/Gem/Stone/Gem_{gemstones[i].Level:D2}");
                    descriptions[i].text = gemstones[i].Description;
                }
                else
                {
                    images[i].sprite = null;
                    descriptions[i].text = "";
                }
            }
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