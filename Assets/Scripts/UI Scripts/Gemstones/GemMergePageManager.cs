using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using model;
using WebSocket;
using Newtonsoft.Json.Linq;

namespace PimDeWitte.UnityMainThreadDispatcher.UI_Scripts.PopUpBox
{
    public class GemMergePageManager : MonoBehaviour
    {
        public static GemMergePageManager Instance { get; private set; }

        [Header("Gem Merge Page")]
        public GameObject gemMergePage;
        public Button closeGemMergeButton;

        [Header("Content Management")]
        public Transform gemMergeContentPanel;
        public GridLayoutGroup gemMergeGridLayout;

        [Header("Auto Merge")]
        public Button autoMergeButton;
        public TextMeshProUGUI autoMergeStatusText;
        public TextMeshProUGUI mergeableGroupsText;

        private GemWebSocketApi _gemApi;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                _gemApi = GemWebSocketApi.Instance;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (gemMergePage != null)
            {
                gemMergePage.SetActive(false);
            }

            if (closeGemMergeButton != null)
            {
                closeGemMergeButton.onClick.AddListener(CloseGemMergePage);
            }

            if (autoMergeButton != null)
            {
                autoMergeButton.onClick.AddListener(OnAutoMergeClicked);
            }
        }

        public void OpenGemMergePage()
        {
            if (gemMergePage != null)
            {
                gemMergePage.SetActive(true);
                ReloadBlockItemsForMerge();
                UpdateMergeableGroupsDisplay();
            }
        }

        public void CloseGemMergePage()
        {
            if (gemMergePage != null)
            {
                gemMergePage.SetActive(false);
            }
        }

        private void ReloadBlockItemsForMerge()
        {
            if (gemMergeContentPanel == null)
            {
                return;
            }

            // Clear existing items in Gem Merge page - safer approach
            List<Transform> childrenToDestroy = new List<Transform>();
            foreach (Transform child in gemMergeContentPanel)
            {
                childrenToDestroy.Add(child);
            }
            
            foreach (Transform child in childrenToDestroy)
            {
                if (child != null)
                {
                    DestroyImmediate(child.gameObject);
                }
            }

            // Find the source content panel based on active menu
            Transform sourceContentPanel = GetSourceContentPanel();
            if (sourceContentPanel == null)
            {
                return;
            }

            // Copy over items from source content panel
            int newBlockCount = 0;
            foreach (Transform block in sourceContentPanel)
            {
                GameObject newBlock = Instantiate(block.gameObject, gemMergeContentPanel);
                newBlock.name = block.name;
                newBlockCount++;
            }

            // Adjust the GridLayoutGroup for the new blocks
            UpdateGridLayoutForGemMerge(sourceContentPanel);
            
            // Force immediate layout rebuild to ensure proper positioning
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(gemMergeContentPanel.GetComponent<RectTransform>());
        }

        private Transform GetSourceContentPanel()
        {
            MenuController menuController = FindObjectOfType<MenuController>();
            if (menuController == null)
            {
                return null;
            }

            if (menuController.IsMenuActive(0)) // Allies Menu
            {
                AlliesBlockSetup alliesBlockSetup = FindObjectOfType<AlliesBlockSetup>();
                return alliesBlockSetup?.contentPanel;
            }
            else if (menuController.IsMenuActive(1)) // Hero Menu
            {
                HeroBlockSetup heroBlockSetup = FindObjectOfType<HeroBlockSetup>();
                return heroBlockSetup?.contentPanel;
            }

            return null;
        }

        private void UpdateGridLayoutForGemMerge(Transform sourceContentPanel)
        {
            if (gemMergeGridLayout == null)
            {
                return;
            }

            // Copy the layout settings from the source
            GridLayoutGroup sourceGridLayout = sourceContentPanel.GetComponent<GridLayoutGroup>();
            if (sourceGridLayout != null)
            {
                gemMergeGridLayout.cellSize = sourceGridLayout.cellSize;
                gemMergeGridLayout.spacing = sourceGridLayout.spacing;
                gemMergeGridLayout.padding = sourceGridLayout.padding;
                gemMergeGridLayout.constraint = sourceGridLayout.constraint;
                gemMergeGridLayout.constraintCount = sourceGridLayout.constraintCount;
            }

            // Rebuild the layout to ensure proper spacing and alignment
            LayoutRebuilder.ForceRebuildLayoutImmediate(gemMergeContentPanel.GetComponent<RectTransform>());
        }

        /// <summary>
        /// Refresh the source inventory display (Hero/Allies) to reflect updated gem data
        /// This forces a complete rebuild of the source inventory
        /// </summary>
        private void RefreshSourceInventoryDisplay()
        {
            MenuController menuController = FindObjectOfType<MenuController>();
            if (menuController == null)
            {
                return;
            }

            if (menuController.IsMenuActive(0)) // Allies Menu
            {
                AlliesBlockSetup alliesBlockSetup = FindObjectOfType<AlliesBlockSetup>();
                if (alliesBlockSetup != null && alliesBlockSetup.contentPanel != null)
                {
                    // Clear the source inventory first, then rebuild
                    Transform contentPanel = alliesBlockSetup.contentPanel;
                    List<Transform> oldBlocks = new List<Transform>();
                    foreach (Transform child in contentPanel)
                    {
                        oldBlocks.Add(child);
                    }
                    
                    foreach (Transform oldBlock in oldBlocks)
                    {
                        if (oldBlock != null)
                        {
                            DestroyImmediate(oldBlock.gameObject);
                        }
                    }
                    
                    // Now rebuild with fresh data
                    alliesBlockSetup.UpdateTotalBlocks();
                }
            }
            else if (menuController.IsMenuActive(1)) // Hero Menu
            {
                HeroBlockSetup heroBlockSetup = FindObjectOfType<HeroBlockSetup>();
                if (heroBlockSetup != null && heroBlockSetup.contentPanel != null)
                {
                    // Clear the source inventory first, then rebuild
                    Transform contentPanel = heroBlockSetup.contentPanel;
                    List<Transform> oldBlocks = new List<Transform>();
                    foreach (Transform child in contentPanel)
                    {
                        oldBlocks.Add(child);
                    }
                    
                    foreach (Transform oldBlock in oldBlocks)
                    {
                        if (oldBlock != null)
                        {
                            DestroyImmediate(oldBlock.gameObject);
                        }
                    }
                    
                    // Now rebuild with fresh data
                    heroBlockSetup.UpdateTotalBlocks();
                }
            }
        }

        /// <summary>
        /// Calculate and display how many gem groups can be merged
        /// </summary>
        private void UpdateMergeableGroupsDisplay()
        {
            if (mergeableGroupsText == null) return;

            var mergeableGroups = CalculateMergeableGroups();
            int totalMergeOperations = mergeableGroups.Sum(g => g.PossibleMerges);

            if (totalMergeOperations > 0)
            {
                mergeableGroupsText.text = $"{totalMergeOperations} groups can be auto-merged";
                if (autoMergeButton != null)
                {
                    autoMergeButton.interactable = true;
                }
            }
            else
            {
                mergeableGroupsText.text = "No gems available for merging";
                if (autoMergeButton != null)
                {
                    autoMergeButton.interactable = false;
                }
            }
        }

        /// <summary>
        /// Calculate which gem groups can be merged (groups of 5+)
        /// </summary>
        private List<MergeableGroup> CalculateMergeableGroups()
        {
            var mergeableGroups = new List<MergeableGroup>();
            
            // Get all unembedded gems from inventory
            var inventoryGems = PlayerProfile.Data?.GetGemstonesInPack();
            if (inventoryGems == null || inventoryGems.Count == 0)
            {
                return mergeableGroups;
            }

            // Group gems by part and level
            var gemGroups = inventoryGems
                .Where(gem => !gem.IsEmbedded) // Only unembedded gems
                .GroupBy(gem => new { gem.Part, gem.Level })
                .Where(group => group.Count() >= 5) // Only groups with 5+ gems
                .ToList();

            foreach (var group in gemGroups)
            {
                int gemCount = group.Count();
                int possibleMerges = gemCount / 5; // Integer division
                int remainingGems = gemCount % 5;

                mergeableGroups.Add(new MergeableGroup
                {
                    Part = group.Key.Part,
                    Level = group.Key.Level,
                    TotalGems = gemCount,
                    PossibleMerges = possibleMerges,
                    RemainingGems = remainingGems
                });
            }

            return mergeableGroups;
        }

        /// <summary>
        /// Handle auto merge button click
        /// </summary>
        public void OnAutoMergeClicked()
        {
            // Disable button during processing
            if (autoMergeButton != null)
            {
                autoMergeButton.interactable = false;
            }

            // Update status
            if (autoMergeStatusText != null)
            {
                autoMergeStatusText.text = "Processing auto merge...";
            }

            // Send auto merge request to backend
            _gemApi.Action("auto_merge", new { }, (response) =>
            {
                HandleAutoMergeResponse(response);
            }, (errorResponse) =>
            {
                string errorMessage = errorResponse?.ToString() ?? "Unknown error";
                HandleAutoMergeError(errorMessage);
            });
        }

        /// <summary>
        /// Handle successful auto merge response
        /// </summary>
        private void HandleAutoMergeResponse(JObject response)
        {
            bool success = response["success"]?.Value<bool>() ?? false;
            
            if (success)
            {
                // Update player profile with new gem inventory
                if (response["inventory_gems"] != null)
                {
                    var updatedGems = response["inventory_gems"].ToObject<List<Gemstone>>();
                    PlayerProfile.Data.SetGems(updatedGems);
                }

                // Show merge results
                var mergedGroups = response["merged_groups"]?.ToObject<List<MergedGroupResult>>();
                int totalOperations = response["total_operations"]?.Value<int>() ?? 0;
                int totalGemsConsumed = response["total_gems_consumed"]?.Value<int>() ?? 0;
                int totalGemsCreated = response["total_gems_created"]?.Value<int>() ?? 0;

                // Update UI
                if (autoMergeStatusText != null)
                {
                    autoMergeStatusText.text = $"✅ Auto merge complete!\n{totalOperations} operations: {totalGemsConsumed} gems → {totalGemsCreated} higher-level gems";
                }

                // Refresh the source inventory first, then the merge page display
                RefreshSourceInventoryDisplay();
                ReloadBlockItemsForMerge();
                UpdateMergeableGroupsDisplay();
            }
            else
            {
                string error = response["error"]?.Value<string>() ?? "Unknown error";
                HandleAutoMergeError(error);
            }

            // Re-enable button
            if (autoMergeButton != null)
            {
                autoMergeButton.interactable = true;
            }
        }

        /// <summary>
        /// Handle auto merge error
        /// </summary>
        private void HandleAutoMergeError(string error)
        {
            if (autoMergeStatusText != null)
            {
                autoMergeStatusText.text = $"❌ Auto merge failed: {error}";
            }

            // Re-enable button
            if (autoMergeButton != null)
            {
                autoMergeButton.interactable = true;
            }
        }

        /// <summary>
        /// Data structure for mergeable gem groups
        /// </summary>
        private class MergeableGroup
        {
            public string Part { get; set; }
            public int Level { get; set; }
            public int TotalGems { get; set; }
            public int PossibleMerges { get; set; }
            public int RemainingGems { get; set; }
        }

        /// <summary>
        /// Data structure for merged group results from backend
        /// </summary>
        private class MergedGroupResult
        {
            public string part { get; set; }
            public int from_level { get; set; }
            public int to_level { get; set; }
            public int gems_consumed { get; set; }
            public int gems_created { get; set; }
            public Gemstone new_gem { get; set; }
        }
    }
}