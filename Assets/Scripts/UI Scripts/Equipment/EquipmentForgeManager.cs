using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using model;
using TMPro;

namespace PimDeWitte.UnityMainThreadDispatcher.UI_Scripts.PopUpBox
{
    public class EquipmentForgeManager : MonoBehaviour
    {
        [Header("Forge Page References")]
        public GameObject forgePage;
        public GameObject commonPage;
        public GameObject forgeBlock;
        public Transform forgePackContent;
        public GridLayoutGroup forgePackGrid;
        
        [Header("Enhance/Upgrade Controls")]
        public Button enhanceButton;
        public Button upgradeButton;
        public GameObject enhancePanel;
        public GameObject upgradePanel;
        public GameObject enhanceOrangeImage;
        public GameObject enhanceGreyImage;
        public GameObject upgradeOrangeImage;
        public GameObject upgradeGreyImage;
        
        [Header("Other Panel References")]
        public GameObject heroStep2Panel;
        public GameObject allyStep2Panel;
        public GameObject equipCompPanel;
        // public GameObject currentEqupment;
        
        [Header("Menu References")]
        public GameObject heroMenu;
        public GameObject allyMenu;
        
        [Header("Pack Sources")]
        public Transform heroSourceContentPanel; // Reference to Hero menu contentPanel
        public Transform allySourceContentPanel; // Reference to Ally menu contentPanel
        
        [Header("Resource Display UI")]
        [Header("Enhance Panel Resources")]
        public TextMeshProUGUI enhanceCrystalText; // Block_1/Text (TMP) - Crystals
        public TextMeshProUGUI enhanceGoldText; // Block_2/Text (TMP) - Gold
        [Header("Upgrade Panel Resources")]
        public TextMeshProUGUI upgradeSkbText; // Block_1/Text (TMP) - Skillbook
        public TextMeshProUGUI upgradeCrystalText; // Block_2/Text (TMP) - Crystals
        
        public static EquipmentForgeManager Instance;
        
        public enum ForgeContext
        {
            Hero,
            Ally
        }
        
        private ForgeContext currentContext;
        private Equipment currentSelectedEquipment; // Store the equipment selected for forging
        
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
            // Listen for player data changes to update resource displays
            PlayerProfile.Data.AddListener(OnPlayerDataUpdated, "Player");
            // Listen for equipment changes to refresh forge page pack
            PlayerProfile.Data.AddListener(OnEquipmentDataUpdated, "Equipments");
        }
        
        private void OnDestroy()
        {
            // Clean up listeners
            if (PlayerProfile.Data != null)
            {
                PlayerProfile.Data.RemoveListener(OnPlayerDataUpdated, "Player");
                PlayerProfile.Data.RemoveListener(OnEquipmentDataUpdated, "Equipments");
            }
        }
        
        private void OnPlayerDataUpdated(ApplicationModel model)
        {
            // Update resource displays if forge page is open
            if (forgePage != null && forgePage.activeInHierarchy)
            {
                bool isEnhanceActive = enhancePanel != null && enhancePanel.activeInHierarchy;
                UpdateResourceDisplays(isEnhanceActive);
            }
        }
        
        private void OnEquipmentDataUpdated(ApplicationModel model)
        {
            // Refresh forge page pack if forge page is open
            if (forgePage != null && forgePage.activeInHierarchy)
            {
                LoadForgePagePack();
            }
        }
        
        public void OpenForgePageFromHero()
        {
            OpenForgePage(ForgeContext.Hero);
        }
        
        public void OpenForgePageFromAlly()
        {
            OpenForgePage(ForgeContext.Ally);
        }
        
        public void OpenForgePage(Equipment selectedEquipment, ForgeContext context = ForgeContext.Hero)
        {
            currentSelectedEquipment = selectedEquipment; // Store the selected equipment
            OpenForgePage(context);
        }
        
        public void OpenForgePage(ForgeContext context = ForgeContext.Hero)
        {
            // Determine context based on which menu is currently active
            if (heroMenu != null && heroMenu.activeInHierarchy)
            {
                currentContext = ForgeContext.Hero;
            }
            else if (allyMenu != null && allyMenu.activeInHierarchy)
            {
                currentContext = ForgeContext.Ally;
            }
            else
            {
                currentContext = context; // fallback to passed context
            }
            
            forgePage.SetActive(true);
            commonPage.SetActive(false);
            CloseEquipComp();
            heroStep2Panel.SetActive(false);
            allyStep2Panel.SetActive(false);
            LoadForgePagePack();

            // Load selected equipment data into forge block
            LoadForgePageData();

            // Add button listeners when ForgePage opens
            enhanceButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.RemoveAllListeners();
            enhanceButton.onClick.AddListener(() => ToggleEnhanceUpgrade(true));
            upgradeButton.onClick.AddListener(() => ToggleEnhanceUpgrade(false));

            // Set default state (this will also update resource displays)
            ToggleEnhanceUpgrade(true);
        }
        
        private void CloseEquipComp()
        {
            if (equipCompPanel != null)
            {
                equipCompPanel.SetActive(false);
            }
        }
        
        public void LoadForgePageData()
        {
            if (forgeBlock == null)
            {
                return;
            }

            Equipment selectedEquipment = null;
            
            // First, check if we have a stored selected equipment
            if (currentSelectedEquipment != null)
            {
                selectedEquipment = currentSelectedEquipment;
            }
            // If no stored equipment, try to get from EquipmentComparisonManager
            else if (EquipmentComparisonManager.Instance != null && EquipmentComparisonManager.Instance.ComparedEquippedId > 0)
            {
                selectedEquipment = PlayerProfile.Data.Player.Equipments.Find(equipment => 
                    equipment.Id == EquipmentComparisonManager.Instance.ComparedEquippedId);
            }

            // If still no equipment is selected, use the equipped helm as default
            if (selectedEquipment == null)
            {
                var heroEquipments = PlayerProfile.Data.GetHeroEquipments();
                selectedEquipment = heroEquipments.Find(equipment => equipment.Part == "Helm");
                if (selectedEquipment == null)
                {
                    return;
                }
            }

            // Update forge block with selected equipment data
            LoadSelectedEquipmentToForgeBlock(selectedEquipment);
        }

        private void LoadSelectedEquipmentToForgeBlock(Equipment equipment)
        {
            // Find EquipImage and EquipNameText in the forge block
            Image equipImage = forgeBlock.transform.Find("EquipImage")?.GetComponent<Image>();
            TextMeshProUGUI equipNameText = forgeBlock.transform.Find("EquipNameText")?.GetComponent<TextMeshProUGUI>();
            
            // Get the background image component from the forge block itself (for quality color)
            Image backgroundImage = forgeBlock.GetComponent<Image>();

            if (equipImage == null)
            {
                return;
            }

            if (equipNameText == null)
            {
                return;
            }

            // Load equipment image
            string imagePath = $"UILoading/Equipment/{equipment.Name}";
            Sprite equipmentSprite = Resources.Load<Sprite>(imagePath);
            if (equipmentSprite != null)
            {
                equipImage.sprite = equipmentSprite;
                equipImage.color = Color.white; // Ensure visibility
            }
            else
            {
                equipImage.color = Color.clear; // Hide if no sprite found
            }

            // Set equipment display name (use descriptive name from backend)
            string equipmentDisplayName = !string.IsNullOrEmpty(equipment.DisplayName) 
                ? equipment.DisplayName 
                : equipment.Name; // Fallback to technical name if display name missing
            equipNameText.text = equipmentDisplayName;

            // Set background color based on equipment quality
            if (backgroundImage != null)
            {
                Color qualityColor = ItemLoader.quantityColor.GetValueOrDefault(equipment.Quality, Color.white);
                backgroundImage.color = qualityColor;
            }

            // Update level progression display
            UpdateLevelProgressionDisplay(equipment);
        }
        
        /// <summary>
        /// Update the level progression display elements (Level X -> Level Y, Attack +X -> Attack +Y)
        /// </summary>
        private void UpdateLevelProgressionDisplay(Equipment equipment)
        {
            // Find level and attack text elements in the Board hierarchy
            // Based on Unity hierarchy: Board -> LevelText_1, LevelText_2, AttackText_1, AttackText_2
            
            // Find current and next level displays
            TextMeshProUGUI currentLevelText = FindTextComponent("LevelText_1");
            TextMeshProUGUI nextLevelText = FindTextComponent("LevelText_2");
            
            // Find current and next attack displays  
            TextMeshProUGUI currentAttackText = FindTextComponent("AttackText_1");
            TextMeshProUGUI nextAttackText = FindTextComponent("AttackText_2");
            
            // Update level displays
            if (currentLevelText != null)
            {
                int currentLevel = equipment.IntensifyLevel + 1; // Display as 1-based (0 -> Level 1)
                currentLevelText.text = $"Level {currentLevel}";
            }
            
            if (nextLevelText != null)
            {
                int nextLevel = equipment.IntensifyLevel + 2; // Next level
                nextLevelText.text = $"Level {nextLevel}";
            }
            
            if (currentAttackText != null)
            {
                int currentAttack = equipment.Attack; // Already calculated: BaseAtk + IntensifyLevel * GrowthAtk
                currentAttackText.text = $"Attack +{currentAttack}";
            }
            
            if (nextAttackText != null)
            {
                int nextAttack = equipment.BaseAtk + (equipment.IntensifyLevel + 1) * equipment.GrowthAtk;
                nextAttackText.text = $"Attack +{nextAttack}";
            }
        }
        
        /// <summary>
        /// Helper method to find text components in the correct Board hierarchy
        /// Structure: ForgePage -> EnhancePanel/UpgradePanel -> Board -> LevelText/AttackText -> LevelText_1/AttackText_1 etc.
        /// </summary>
        private TextMeshProUGUI FindTextComponent(string componentName)
        {
            // Need to find the ForgePage first, then the correct panel
            Transform forgePageTransform = forgePage.transform;
            
            // Try both EnhancePanel and UpgradePanel to see which one is active or contains the component
            string[] panelNames = { "EnhancePanel", "UpgradePanel" };
            
            foreach (string panelName in panelNames)
            {
                Transform panelTransform = forgePageTransform.Find(panelName);
                if (panelTransform == null) continue;
                
                // Find Board within the panel
                Transform boardTransform = panelTransform.Find("Board");
                if (boardTransform == null) continue;
                
                // Determine the container based on component name
                string containerName = "";
                if (componentName.StartsWith("Level"))
                {
                    containerName = "LevelText";
                }
                else if (componentName.StartsWith("Attack"))
                {
                    containerName = "AttackText";
                }
                
                if (string.IsNullOrEmpty(containerName)) continue;
                
                // Find the container within Board
                Transform containerTransform = boardTransform.Find(containerName);
                if (containerTransform == null) continue;
                
                // Finally, find the actual text component within the container
                TextMeshProUGUI textComponent = containerTransform.Find(componentName)?.GetComponent<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    return textComponent;
                }
            }
            
            return null;
        }
        
        public void LoadForgePagePack()
        {
            if (forgePackContent == null)
            {
                return;
            }

            // Clear existing items in ForgePage Pack
            foreach (Transform child in forgePackContent)
            {
                Destroy(child.gameObject);
            }

            // Get the correct source based on current context
            Transform sourcePanel = currentContext == ForgeContext.Hero ? heroSourceContentPanel : allySourceContentPanel;
            
            // Clone each item from the appropriate source Pack and add forge-specific click handlers
            if (sourcePanel != null)
            {
                // Get equipment data to associate with cloned items
                List<Equipment> packEquipments = PlayerProfile.Data.GetEquipmentsInPack();
                int itemIndex = 0;
                
                foreach (Transform item in sourcePanel)
                {
                    GameObject newItem = Instantiate(item.gameObject, forgePackContent);
                    newItem.name = item.name; // Keep the same name
                    
                    // Add forge-specific click handler if this is an equipment item
                    if (itemIndex < packEquipments.Count)
                    {
                        Equipment equipment = packEquipments[itemIndex];
                        AddForgePackClickHandler(newItem, equipment);
                        itemIndex++;
                    }
                }
                // ...
            }

            // Apply dynamic grid adjustments with delayed setup to ensure RectTransform is properly sized
            if (forgePackGrid != null)
            {
                StartCoroutine(DelayedSetupForgePackGrid());
            }
        }
        
        /// <summary>
        /// Add click handler to a forge pack item that updates the selected equipment and refreshes the top block
        /// </summary>
        private void AddForgePackClickHandler(GameObject packItem, Equipment equipment)
        {
            // Get or add Button component
            Button button = packItem.GetComponent<Button>();
            if (button == null)
            {
                button = packItem.AddComponent<Button>();
            }
            
            // Remove existing listeners to avoid conflicts
            button.onClick.RemoveAllListeners();
            
            // Add new click handler for forge page
            button.onClick.AddListener(() => {
                // Perform equipment swap: move clicked equipment up, previous equipment down
                SwapEquipmentInForge(equipment, packItem);
            });
        }
        
        /// <summary>
        /// Swap equipment: move clicked pack equipment to top block, move previous top equipment to pack
        /// </summary>
        private void SwapEquipmentInForge(Equipment newEquipment, GameObject clickedPackItem)
        {
            // Store the previously selected equipment for swapping down
            Equipment previousEquipment = currentSelectedEquipment;
            
            // Update the top block with new equipment
            currentSelectedEquipment = newEquipment;
            LoadSelectedEquipmentToForgeBlock(newEquipment);
            
            // If there was a previous equipment, update the clicked pack item to show it
            if (previousEquipment != null)
            {
                UpdatePackItemDisplay(clickedPackItem, previousEquipment);
                // Update the click handler of the pack item to use the previous equipment
                UpdatePackItemClickHandler(clickedPackItem, previousEquipment);
            }
        }
        
        /// <summary>
        /// Update a pack item's visual display to show different equipment
        /// </summary>
        private void UpdatePackItemDisplay(GameObject packItem, Equipment equipment)
        {
            // Update the equipment image
            Image itemImage = packItem.transform.Find("Image")?.GetComponent<Image>();
            if (itemImage != null)
            {
                string imagePath = $"UILoading/Equipment/{equipment.Name}";
                Sprite equipmentSprite = Resources.Load<Sprite>(imagePath);
                if (equipmentSprite != null)
                {
                    itemImage.sprite = equipmentSprite;
                    itemImage.color = Color.white;
                }
            }
            
            // Update the background color based on quality
            Image backgroundImage = packItem.GetComponent<Image>();
            if (backgroundImage != null)
            {
                Color qualityColor = ItemLoader.quantityColor.GetValueOrDefault(equipment.Quality, Color.white);
                backgroundImage.color = qualityColor;
            }
            
            // Update equipment part icon if it exists
            Image partImage = packItem.transform.Find("Part")?.GetComponent<Image>();
            if (partImage != null)
            {
                Sprite partSprite = Resources.Load<Sprite>($"UILoading/Equipment/Part/{equipment.Part}");
                if (partSprite != null)
                {
                    partImage.sprite = partSprite;
                }
            }
            
            // ...
        }
        
        /// <summary>
        /// Update a pack item's click handler to use different equipment data
        /// </summary>
        private void UpdatePackItemClickHandler(GameObject packItem, Equipment equipment)
        {
            Button button = packItem.GetComponent<Button>();
            if (button != null)
            {
                // Remove old click handler and add new one with updated equipment
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => {
                    SwapEquipmentInForge(equipment, packItem);
                });
            }
        }
        
        private System.Collections.IEnumerator DelayedSetupForgePackGrid()
        {
            // Wait for the next frame to ensure UI layout is updated
            yield return null;
            
            // Force canvas update to ensure rect sizes are calculated
            Canvas.ForceUpdateCanvases();
            
            // Wait one more frame for the force update to take effect
            yield return null;
            
            SetupForgePackGrid();
        }
        
        private void SetupForgePackGrid()
        {
            // Get panel width from the RectTransform, ensuring it's properly updated
            RectTransform forgeContentRect = forgePackContent.GetComponent<RectTransform>();
            float panelWidth = forgeContentRect.rect.width;
            
            // If width is still 0 or very small, use a fallback calculation
            if (panelWidth <= 10f)
            {
                // Get width from parent or use a reasonable fallback
                RectTransform parentRect = forgeContentRect.parent as RectTransform;
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
            
            // ...
            
            forgePackGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            forgePackGrid.constraintCount = 5;
            forgePackGrid.cellSize = new Vector2(blockWidth, blockWidth);
            forgePackGrid.spacing = new Vector2(spacingX, spacingY);
            forgePackGrid.childAlignment = TextAnchor.UpperLeft; // Align items to the left instead of center
            forgePackGrid.padding.left = Mathf.RoundToInt(leftPadding);
            forgePackGrid.padding.right = Mathf.RoundToInt(rightPadding);
        }
        
        private void ToggleEnhanceUpgrade(bool isEnhance)
        {
            // Enable Enhance Panel, Disable Upgrade Panel
            enhancePanel.SetActive(isEnhance);
            upgradePanel.SetActive(!isEnhance);

            // Handle Enhance Button visuals
            enhanceOrangeImage.SetActive(isEnhance);
            enhanceGreyImage.SetActive(!isEnhance);

            // Handle Upgrade Button visuals
            upgradeOrangeImage.SetActive(!isEnhance);
            upgradeGreyImage.SetActive(isEnhance);
            
            // Update resource displays for the active panel
            UpdateResourceDisplays(isEnhance);
        }
        
        public void CloseForgePage()
        {
            forgePage.SetActive(false);
            commonPage.SetActive(true);

            // Return to the correct panel based on context
            if (currentContext == ForgeContext.Hero)
            {
                heroStep2Panel.SetActive(true);
            }
            else
            {
                allyStep2Panel.SetActive(true);
                heroStep2Panel.SetActive(true);
            }

            // Clear ForgeBlock EquipImage, EquipNameText, and background color
            Image equipImage = forgeBlock.transform.Find("EquipImage")?.GetComponent<Image>();
            TextMeshProUGUI equipNameText = forgeBlock.transform.Find("EquipNameText")?.GetComponent<TextMeshProUGUI>();
            Image backgroundImage = forgeBlock.GetComponent<Image>();

            if (equipImage != null)
            {
                equipImage.sprite = null;
                equipImage.color = new Color(0, 0, 0, 0); // Fully transparent
            }

            if (equipNameText != null)
            {
                equipNameText.text = "";
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = Color.white; // Reset to default white background
            }
        }
        
        /// <summary>
        /// Update resource displays for the current panel (Enhance/Upgrade)
        /// </summary>
        private void UpdateResourceDisplays(bool isEnhance)
        {
            // Get player data
            Player player = PlayerProfile.Data?.Player;
            if (player == null)
            {
                return;
            }
            
            // Get crystal count from ItemsJson
            int crystalCount = GetCrystalCount(player);
            
            if (isEnhance)
            {
                // EnhancePanel: Block_1 = crystals, Block_2 = gold coins
                if (enhanceCrystalText != null)
                {
                    enhanceCrystalText.text = $"0/{FormatNumber(crystalCount)}";
                }
                
                if (enhanceGoldText != null)
                {
                    enhanceGoldText.text = $"0/{FormatNumber(player.GoldCoin)}";
                }
            }
            else
            {
                // UpgradePanel: Block_1 = skillbook, Block_2 = crystals
                // upgradeSkbText (skillbook) - declared but not implemented yet
                
                if (upgradeCrystalText != null)
                {
                    upgradeCrystalText.text = $"0/{FormatNumber(crystalCount)}";
                }
            }
        }
        
        
        /// <summary>
        /// Get crystal count from player's ItemsJson
        /// </summary>
        private int GetCrystalCount(Player player)
        {
            if (player?.ItemsJson != null && player.ItemsJson.ContainsKey("crystal"))
            {
                return player.ItemsJson["crystal"];
            }
            return 0;
        }
        
        /// <summary>
        /// Format number with K suffix for thousands
        /// </summary>
        private string FormatNumber(int number)
        {
            if (number >= 1000)
            {
                float thousands = number / 1000f;
                return $"{thousands:0.0}K";
            }
            return number.ToString();
        }
    }
}