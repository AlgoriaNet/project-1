using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using model;
using TMPro;
using System.Linq;
using WebSocket;
using Newtonsoft.Json.Linq;

namespace PimDeWitte.UnityMainThreadDispatcher.UI_Scripts.PopUpBox
{
    public class EquipmentDismantleManager : MonoBehaviour
    {
        [Header("Dismantle Page References")]
        public GameObject dismantlePage;
        public GameObject commonPage;
        public GameObject dismantleBlock;
        public Transform dismantlePackContent;
        public GridLayoutGroup dismantlePackGrid;
        
        [Header("Congrats Page References")]
        public GameObject congratsPage;
        public GameObject dismantlePack;
        
        [Header("Other Panel References")]
        public GameObject heroStep2Panel;
        public GameObject allyStep2Panel;
        public GameObject equipCompPanel;
        
        [Header("Menu References")]
        public GameObject heroMenu;
        public GameObject allyMenu;
        
        [Header("Pack Sources")]
        public Transform heroSourceContentPanel; // Reference to Hero menu contentPanel
        public Transform allySourceContentPanel; // Reference to Ally menu contentPanel
        
        [Header("Crystal Block UI")]
        public Image crystalImage; // Crystal icon in the block
        public TextMeshProUGUI crystalQuantityText; // Crystal quantity display
        
        [Header("Dismantle Action")]
        public Button dismantleButton; // Button to trigger dismantle
        
        public static EquipmentDismantleManager Instance;
        
        // Selection management
        private List<Equipment> selectedEquipments = new List<Equipment>();
        private List<GameObject> equipmentBlocks = new List<GameObject>();
        
        public enum DismantleContext
        {
            Hero,
            Ally
        }
        
        private DismantleContext currentContext;
        
        // Grid layout variables (copied from HeroBlockSetup)
        private float blockWidth;
        private float leftPadding;
        private float rightPadding;
        private float spacingX;
        private float spacingY;
        
        public void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void Start()
        {
            // Set up dismantle button
            if (dismantleButton != null)
            {
                dismantleButton.onClick.AddListener(DismantleSelectedEquipment);
            }
        }
        
        public void OpenDismantlePageFromHero()
        {
            OpenDismantlePage(DismantleContext.Hero);
        }
        
        public void OpenDismantlePageFromAlly()
        {
            OpenDismantlePage(DismantleContext.Ally);
        }
        
        public void OpenDismantlePage(DismantleContext context = DismantleContext.Hero)
        {
            // Determine context based on which menu is currently active
            if (heroMenu != null && heroMenu.activeInHierarchy)
            {
                currentContext = DismantleContext.Hero;
            }
            else if (allyMenu != null && allyMenu.activeInHierarchy)
            {
                currentContext = DismantleContext.Ally;
            }
            else
            {
                currentContext = context; // fallback to passed context
            }
            
            dismantlePage.SetActive(true);
            commonPage.SetActive(false);
            CloseEquipComp();
            heroStep2Panel.SetActive(false);
            allyStep2Panel.SetActive(false);
            
            // Ensure congrats page is hidden and dismantle pack is shown
            congratsPage.SetActive(false);
            dismantlePack.SetActive(true);
            MoveBlockUp(); // Reset block position
            
            LoadDismantlePagePack();

            // Load selected equipment data into dismantle block
            LoadDismantlePageData();
        }
        
        private void CloseEquipComp()
        {
            if (equipCompPanel != null)
            {
                equipCompPanel.SetActive(false);
            }
        }
        
        public void LoadDismantlePageData()
        {
        }

        
        public void LoadDismantlePagePack()
        {
            if (dismantlePackContent == null)
            {
                return;
            }

            // Clear existing selection and items
            selectedEquipments.Clear();
            equipmentBlocks.Clear();
            
            // Clear existing items in DismantlePage Pack
            foreach (Transform child in dismantlePackContent)
            {
                Destroy(child.gameObject);
            }

            // Get all equipment from pack (all are dismantleable)
            List<Equipment> packEquipments = PlayerProfile.Data?.GetEquipmentsInPack() ?? new List<Equipment>();
            
            // Create equipment blocks with selection functionality
            for (int i = 0; i < packEquipments.Count; i++)
            {
                Equipment equipment = packEquipments[i];
                GameObject newBlock = CreateEquipmentBlock(equipment, i);
                equipmentBlocks.Add(newBlock);
            }
            
            // Apply dynamic grid adjustments with delayed setup to ensure RectTransform is properly sized
            if (dismantlePackGrid != null)
            {
                StartCoroutine(DelayedSetupDismantlePackGrid());
            }
            
            // Update crystal display and button state
            UpdateCrystalDisplay();
            UpdateDismantleButton();
        }
        
        private System.Collections.IEnumerator DelayedSetupDismantlePackGrid()
        {
            // Wait for the next frame to ensure UI layout is updated
            yield return null;
            
            // Force canvas update to ensure rect sizes are calculated
            Canvas.ForceUpdateCanvases();
            
            // Wait one more frame for the force update to take effect
            yield return null;
            
            SetupDismantlePackGrid();
        }
        
        private void SetupDismantlePackGrid()
        {
            // Get panel width from the RectTransform, ensuring it's properly updated
            RectTransform dismantleContentRect = dismantlePackContent.GetComponent<RectTransform>();
            float panelWidth = dismantleContentRect.rect.width;
            
            // If width is still 0 or very small, use a fallback calculation
            if (panelWidth <= 10f)
            {
                // Get width from parent or use a reasonable fallback
                RectTransform parentRect = dismantleContentRect.parent as RectTransform;
                if (parentRect != null && parentRect.rect.width > 10f)
                {
                    panelWidth = parentRect.rect.width;
                }
                else
                {
                    // Last resort fallback - use a standard screen proportion
                    panelWidth = Screen.width * 0.8f;
                }
            }
            
            int blocksPerRow = 5;
            blockWidth = panelWidth / (blocksPerRow + 1);
            leftPadding = blockWidth * 0.25f;
            rightPadding = blockWidth * 0.25f;
            spacingX = blockWidth * 0.125f;
            spacingY = blockWidth * 0.125f;
            
            dismantlePackGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            dismantlePackGrid.constraintCount = 5;
            dismantlePackGrid.cellSize = new Vector2(blockWidth, blockWidth);
            dismantlePackGrid.spacing = new Vector2(spacingX, spacingY);
            dismantlePackGrid.childAlignment = TextAnchor.UpperLeft; // Align items to the left instead of center
            dismantlePackGrid.padding.left = Mathf.RoundToInt(leftPadding);
            dismantlePackGrid.padding.right = Mathf.RoundToInt(rightPadding);
        }
        
        public void OpenCongratsPage()
        {
            congratsPage.SetActive(true);
            dismantlePack.SetActive(false);
            MoveBlockDown();
        }
        
        public void CloseDismantlePage()
        {
            dismantlePage.SetActive(false);
            commonPage.SetActive(true);

            // Clear selection when closing
            selectedEquipments.Clear();
            equipmentBlocks.Clear();

            // Return to the correct panel based on context
            if (currentContext == DismantleContext.Hero)
            {
                heroStep2Panel.SetActive(true);
            }
            else
            {
                allyStep2Panel.SetActive(true);
                heroStep2Panel.SetActive(true);
            }

            congratsPage.SetActive(false);
            dismantlePack.SetActive(true);
            MoveBlockUp();
        }
        
        public void MoveBlockDown()
        {
            if (dismantleBlock != null)
            {
                RectTransform dismantleBlockRect = dismantleBlock.GetComponent<RectTransform>();
                dismantleBlockRect.anchorMin = new Vector2(dismantleBlockRect.anchorMin.x, 0.5f);
                dismantleBlockRect.anchorMax = new Vector2(dismantleBlockRect.anchorMax.x, 0.5f);
            }
        }
        
        public void MoveBlockUp()
        {
            if (dismantleBlock != null)
            {
                RectTransform dismantleBlockRect = dismantleBlock.GetComponent<RectTransform>();
                dismantleBlockRect.anchorMin = new Vector2(dismantleBlockRect.anchorMin.x, 0.75f);
                dismantleBlockRect.anchorMax = new Vector2(dismantleBlockRect.anchorMax.x, 0.7f);
            }
        }
        
        
        /// <summary>
        /// Create an equipment block with selection functionality
        /// </summary>
        private GameObject CreateEquipmentBlock(Equipment equipment, int index)
        {
            // Find an existing block from source to clone from
            Transform sourcePanel = currentContext == DismantleContext.Hero ? heroSourceContentPanel : allySourceContentPanel;
            GameObject blockPrefab = null;
            
            if (sourcePanel != null && sourcePanel.childCount > 0)
            {
                blockPrefab = sourcePanel.GetChild(0).gameObject; // Use first child as template
            }
            
            if (blockPrefab == null)
            {
                return null;
            }
            
            GameObject newBlock = Instantiate(blockPrefab, dismantlePackContent);
            newBlock.name = $"EquipmentBlock_{equipment.Id}";
            
            // Set up the equipment data display (using existing ItemLoader logic)
            var itemLoader = FindObjectOfType<ItemLoader>();
            if (itemLoader != null)
            {
                itemLoader.LoadEquipmentItems(newBlock.transform, equipment);
            }
            
            // Add selection functionality
            Button blockButton = newBlock.GetComponent<Button>();
            if (blockButton != null)
            {
                blockButton.onClick.RemoveAllListeners();
                blockButton.onClick.AddListener(() => ToggleEquipmentSelection(equipment, newBlock));
            }
            
            // Add selection visual indicator (overlay)
            AddSelectionOverlay(newBlock);
            
            return newBlock;
        }
        
        /// <summary>
        /// Add a selection overlay to the equipment block
        /// </summary>
        private void AddSelectionOverlay(GameObject block)
        {
            // Create selection overlay
            GameObject overlay = new GameObject("SelectionOverlay");
            overlay.transform.SetParent(block.transform, false);
            
            // Set up RectTransform
            RectTransform overlayRect = overlay.AddComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;
            
            // Add Image component for visual feedback
            Image overlayImage = overlay.AddComponent<Image>();
            overlayImage.color = new Color(0f, 1f, 0f, 0.3f); // Green with transparency
            overlayImage.raycastTarget = false; // Don't block clicks
            
            // Initially hidden
            overlay.SetActive(false);
        }
        
        /// <summary>
        /// Toggle selection of an equipment
        /// </summary>
        private void ToggleEquipmentSelection(Equipment equipment, GameObject block)
        {
            GameObject overlay = block.transform.Find("SelectionOverlay")?.gameObject;
            
            if (selectedEquipments.Contains(equipment))
            {
                // Deselect
                selectedEquipments.Remove(equipment);
                if (overlay != null) overlay.SetActive(false);
            }
            else
            {
                // Select
                selectedEquipments.Add(equipment);
                if (overlay != null) overlay.SetActive(true);
            }
            
            // Update UI
            UpdateCrystalDisplay();
            UpdateDismantleButton();
        }
        
        /// <summary>
        /// Update crystal display based on selected equipment
        /// </summary>
        private void UpdateCrystalDisplay()
        {
            int totalCrystals = CalculateExpectedCrystals();
            
            if (crystalQuantityText != null)
            {
                crystalQuantityText.text = totalCrystals.ToString();
            }
            
            // Load crystal sprite if not already set
            if (crystalImage != null && crystalImage.sprite == null)
            {
                Sprite crystalSprite = Resources.Load<Sprite>("UILoading/CharacterImages/Shard/crystal");
                if (crystalSprite != null)
                {
                    crystalImage.sprite = crystalSprite;
                    crystalImage.color = Color.white;
                }
            }
        }
        
        /// <summary>
        /// Calculate expected crystals from selected equipment
        /// </summary>
        private int CalculateExpectedCrystals()
        {
            int totalCrystals = 0;
            
            foreach (Equipment equipment in selectedEquipments)
            {
                // Base reward: 10 crystals
                int baseCrystals = 10;
                
                // Forge refund: 80% of crystals spent
                int refundCrystals = Mathf.RoundToInt((equipment.TotalCrystalsSpent ?? 0) * 0.8f);
                
                totalCrystals += baseCrystals + refundCrystals;
            }
            
            return totalCrystals;
        }
        
        /// <summary>
        /// Update dismantle button state
        /// </summary>
        private void UpdateDismantleButton()
        {
            if (dismantleButton == null) return;
            
            bool hasSelection = selectedEquipments.Count > 0;
            dismantleButton.interactable = hasSelection;
        }
        
        /// <summary>
        /// Dismantle selected equipment
        /// </summary>
        public void DismantleSelectedEquipment()
        {
            if (selectedEquipments.Count == 0)
            {
                return;
            }
            
            StartCoroutine(DismantleEquipmentCoroutine());
        }
        
        /// <summary>
        /// Coroutine to handle dismantling multiple equipment
        /// </summary>
        private System.Collections.IEnumerator DismantleEquipmentCoroutine()
        {
            List<Equipment> equipmentToDismantle = new List<Equipment>(selectedEquipments);
            
            foreach (Equipment equipment in equipmentToDismantle)
            {
                yield return StartCoroutine(DismantleSingleEquipment(equipment));
                
                // Refresh the dismantle page after each successful dismantle
                // to immediately remove the equipment from the UI
                LoadDismantlePagePack();
                
                yield return new WaitForSeconds(0.1f); // Small delay between requests
            }
            
            // Navigate to congrats page after all equipment is dismantled
            OpenCongratsPage();
        }
        
        /// <summary>
        /// Dismantle a single equipment via WebSocket
        /// </summary>
        private System.Collections.IEnumerator DismantleSingleEquipment(Equipment equipment)
        {
            // Create dismantle request data
            var requestData = new
            {
                equipmentId = equipment.Id
            };
            
            bool requestCompleted = false;
            bool requestSucceeded = false;
            
            // Send WebSocket request using proper API
            EquipmentWebSocketApi.Instance.Action("dismantle", requestData, 
                (response) => {
                    // Success callback
                    requestCompleted = true;
                    requestSucceeded = true;
                    
                    // Update player profile from response if available
                    // Backend response structure: { "success": true, "player_profile": {...}, "crystals_rewarded": 10 }
                    if (response.ContainsKey("player_profile"))
                    {
                        // Update PlayerProfile with new data from server
                        var playerProfileJson = response["player_profile"].ToString();
                        var updatedPlayer = Newtonsoft.Json.JsonConvert.DeserializeObject<Player>(playerProfileJson);
                        PlayerProfile.Data.SetPlayer(updatedPlayer);
                    }
                    
                    if (response.ContainsKey("crystals_rewarded"))
                    {
                        int crystalsRewarded = response["crystals_rewarded"].Value<int>();
                    }
                },
                (error) => {
                    // Error callback
                    requestCompleted = true;
                    requestSucceeded = false;
                }
            );
            
            // Wait for request to complete
            while (!requestCompleted)
            {
                yield return null;
            }
            
            // Remove from selected list if successful
            if (requestSucceeded && selectedEquipments.Contains(equipment))
            {
                selectedEquipments.Remove(equipment);
            }
        }
    }
}