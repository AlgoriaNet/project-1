using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using model;
using TMPro;
using WebSocket;
using Newtonsoft.Json.Linq;

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
        
        [Header("Enhancement Action Buttons")]
        public Button singleEnhanceButton; // "Enhance" button
        public Button autoEnhanceButton; // "Auto Enhance" button
        public Button washButton; // "Wash" button (Button_2 in UpgradePanel)
        
        public static EquipmentForgeManager Instance;
        
        public enum ForgeContext
        {
            Hero,
            Ally
        }
        
        private ForgeContext currentContext;
        private Equipment currentSelectedEquipment; // Store the equipment selected for forging
        
        // Enhancement cost tracking
        private int currentEnhanceCrystalCost = 0;
        private int currentEnhanceGoldCost = 0;
        private bool canAffordEnhancement = false;
        
        // Current/Next level data from API
        private int currentLevel = 0;
        private int nextLevel = 0;
        private int currentAttack = 0;
        private int nextAttack = 0;
        private int attackIncrease = 0;
        
        // Prevent duplicate API calls
        private int lastFetchedEquipmentId = -1;
        
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
            
            // Setup button listeners
            if (singleEnhanceButton != null)
                singleEnhanceButton.onClick.AddListener(PerformSingleEnhancement);
            if (autoEnhanceButton != null)
                autoEnhanceButton.onClick.AddListener(PerformAutoEnhancement);
            if (washButton != null)
                washButton.onClick.AddListener(PerformWash);
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

            // Setup enhancement action button listeners
            if (singleEnhanceButton != null)
            {
                singleEnhanceButton.onClick.RemoveAllListeners();
                singleEnhanceButton.onClick.AddListener(PerformSingleEnhancement);
            }
            
            if (autoEnhanceButton != null)
            {
                autoEnhanceButton.onClick.RemoveAllListeners();
                autoEnhanceButton.onClick.AddListener(PerformAutoEnhancement);
            }

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

            // Fetch enhancement cost for this equipment (this will also update level progression)
            FetchEnhancementCost(equipment);
            
            // Fallback: Also update level progression from equipment data directly
            UpdateLevelProgressionDisplayFromEquipment(equipment);
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
            
            // Update level displays (backend uses 1-based intensify_level)
            if (currentLevelText != null)
            {
                int currentLevel = equipment.IntensifyLevel; // Backend already provides 1-based level
                currentLevelText.text = $"Level {currentLevel}";
            }
            
            if (nextLevelText != null)
            {
                int nextLevel = equipment.IntensifyLevel + 1; // Next level
                nextLevelText.text = $"Level {nextLevel}";
            }
            
            if (currentAttackText != null)
            {
                int currentAttack = equipment.Attack; // Use backend's calculated total attack
                currentAttackText.text = $"Attack +{currentAttack}";
            }
            
            if (nextAttackText != null)
            {
                // Calculate next level attack based on backend formula
                // Quality-based attack bonus: 1-3: +5, 4-5: +10, 6: +15
                int attackBonusPerLevel = equipment.Quality >= 6 ? 15 : 
                                         equipment.Quality >= 4 ? 10 : 5;
                int nextAttack = equipment.Attack + attackBonusPerLevel;
                nextAttackText.text = $"Attack +{nextAttack}";
            }
        }
        
        /// <summary>
        /// Update level progression display using data from enhancement cost API
        /// </summary>
        private void UpdateLevelProgressionDisplayFromAPI()
        {
            // Find level and attack text elements
            TextMeshProUGUI currentLevelText = FindTextComponent("LevelText_1");
            TextMeshProUGUI nextLevelText = FindTextComponent("LevelText_2");
            TextMeshProUGUI currentAttackText = FindTextComponent("AttackText_1");
            TextMeshProUGUI nextAttackText = FindTextComponent("AttackText_2");
            
            // Update level displays using API data
            if (currentLevelText != null)
            {
                currentLevelText.text = $"Level {currentLevel}";
            }
            
            if (nextLevelText != null)
            {
                nextLevelText.text = $"Level {nextLevel}";
            }
            
            // Update attack displays using API data
            if (currentAttackText != null)
            {
                currentAttackText.text = $"Attack +{currentAttack}";
            }
            
            if (nextAttackText != null)
            {
                nextAttackText.text = $"Attack +{nextAttack}";
            }
        }
        
        /// <summary>
        /// Fallback: Update level progression display using equipment data (when API fails)
        /// </summary>
        private void UpdateLevelProgressionDisplayFromEquipment(Equipment equipment)
        {
            // Find level and attack text elements
            TextMeshProUGUI currentLevelText = FindTextComponent("LevelText_1");
            TextMeshProUGUI nextLevelText = FindTextComponent("LevelText_2");
            TextMeshProUGUI currentAttackText = FindTextComponent("AttackText_1");
            TextMeshProUGUI nextAttackText = FindTextComponent("AttackText_2");
            
            // Use equipment data directly
            int currentLevel = equipment.IntensifyLevel > 0 ? equipment.IntensifyLevel : 1;
            int nextLevel = currentLevel + 1;
            int currentAttack = equipment.Attack;
            
            // Calculate next attack based on quality
            int attackBonusPerLevel = equipment.Quality >= 6 ? 15 : 
                                     equipment.Quality >= 4 ? 10 : 5;
            int nextAttack = currentAttack + attackBonusPerLevel;
            
            // Update level displays
            if (currentLevelText != null)
            {
                currentLevelText.text = $"Level {currentLevel}";
            }
            
            if (nextLevelText != null)
            {
                nextLevelText.text = $"Level {nextLevel}";
            }
            
            // Update attack displays
            if (currentAttackText != null)
            {
                currentAttackText.text = $"Attack +{currentAttack}";
            }
            
            if (nextAttackText != null)
            {
                nextAttackText.text = $"Attack +{nextAttack}";
            }
            
            // Calculate fallback costs using the F2P progression formula from backend docs
            // Crystals: 100 + (level-1) * 30 + (level-1)² * 5
            // Gold: 1500 * (1.4 ^ (level-1))
            int crystalCost = 100 + (currentLevel - 1) * 30 + (currentLevel - 1) * (currentLevel - 1) * 5;
            int goldCost = (int)(1500 * System.Math.Pow(1.4, currentLevel - 1));
            
            // Update cost variables for display
            currentEnhanceCrystalCost = crystalCost;
            currentEnhanceGoldCost = goldCost;
            
            // Check affordability using player resources
            Player player = PlayerProfile.Data?.Player;
            int playerCrystals = GetCrystalCount(player);
            int playerGold = player?.GoldCoin ?? 0;
            canAffordEnhancement = playerCrystals >= crystalCost && playerGold >= goldCost;
            
            // Ensure UI is updated with fallback costs
            if (enhancePanel != null && enhancePanel.activeInHierarchy)
            {
                UpdateResourceDisplays(true);
            }
        }
        
        /// <summary>
        /// Helper method to find text components in the correct Board hierarchy
        /// Structure: ForgePage -> EnhancePanel/UpgradePanel -> Board -> LevelText/AttackText -> LevelText_1/AttackText_1 etc.
        /// </summary>
        private TextMeshProUGUI FindTextComponent(string componentName)
        {
            // Need to find the ForgePage first, then the ACTIVE panel
            Transform forgePageTransform = forgePage.transform;
            
            // Determine which panel is currently active with detailed logging
            bool isEnhanceActive = enhancePanel != null && enhancePanel.activeInHierarchy;
            bool isUpgradeActive = upgradePanel != null && upgradePanel.activeInHierarchy;
            
            // If no panels are active, return null gracefully (happens during initial loading)
            if (!isEnhanceActive && !isUpgradeActive)
            {
                return null;
            }
            
            string activePanelName = isEnhanceActive ? "EnhancePanel" : "UpgradePanel";
            
            Transform panelTransform = forgePageTransform.Find(activePanelName);
            if (panelTransform == null) 
            {
                return null;
            }
            
            // Double-check that the chosen panel is actually active
            if (!panelTransform.gameObject.activeInHierarchy)
            {
                return null;
            }
            
            // Find Board within the active panel
            Transform boardTransform = panelTransform.Find("Board");
            if (boardTransform == null) 
            {
                // Debug: List all children under the panel
                for (int i = 0; i < panelTransform.childCount; i++)
                {
                    Transform child = panelTransform.GetChild(i);
                }
                
                return null;
            }
            
            // Determine the container based on component name and panel type
            string containerName = "";
            if (componentName.StartsWith("Level"))
            {
                containerName = "LevelText";
            }
            else if (componentName.StartsWith("Attack"))
            {
                // Different panels may have different attack text hierarchy
                containerName = "AttackText";
            }
            
            if (string.IsNullOrEmpty(containerName)) 
            {
                return null;
            }
            
            // Find the container within Board
            Transform containerTransform = boardTransform.Find(containerName);
            if (containerTransform == null && containerName == "AttackText") 
            {
                // Handle typo in UpgradePanel: "AttacklText" instead of "AttackText"
                containerTransform = boardTransform.Find("AttachlText");
                if (containerTransform != null)
                {
                }
            }
            
            if (containerTransform == null) 
            {
                return null;
            }
            
            // Finally, find the actual text component within the container
            TextMeshProUGUI textComponent = containerTransform.Find(componentName)?.GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                return textComponent;
            }
            else
            {
                return null;
            }
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
            
            // Update level/attack displays for the newly active panel
            UpdateLevelProgressionDisplayFromAPI();
            
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
                // Show actual enhancement costs from API or fallback calculation
                
                if (enhanceCrystalText != null)
                {
                    string crystalDisplay = $"{NumberFormatter.FormatNumber(currentEnhanceCrystalCost)}/{NumberFormatter.FormatNumber(crystalCount)}";
                    enhanceCrystalText.text = crystalDisplay;
                }
                
                if (enhanceGoldText != null)
                {
                    string goldDisplay = $"{NumberFormatter.FormatNumber(currentEnhanceGoldCost)}/{NumberFormatter.FormatNumber(player.GoldCoin)}";
                    enhanceGoldText.text = goldDisplay;
                }
                
                // Update button interactability based on affordability
                if (singleEnhanceButton != null)
                {
                    singleEnhanceButton.interactable = canAffordEnhancement;
                }
                if (autoEnhanceButton != null)
                {
                    autoEnhanceButton.interactable = canAffordEnhancement;
                }
            }
            else
            {
                // UpgradePanel: Block_1 = skillbook, Block_2 = crystals (for wash function)
                // Show wash cost: 200 crystals fixed cost
                const int WASH_COST = 200;
                
                if (upgradeSkbText != null)
                {
                    // Block_1 shows wash cost info (could be used for skillbook later)
                    upgradeSkbText.text = "Wash";
                }
                
                if (upgradeCrystalText != null)
                {
                    // Block_2 shows wash crystal cost
                    upgradeCrystalText.text = $"{WASH_COST}/{NumberFormatter.FormatNumber(crystalCount)}";
                }
                
                // Update wash button interactability
                if (washButton != null)
                {
                    bool canAffordWash = crystalCount >= WASH_COST;
                    bool hasEquipmentSelected = currentSelectedEquipment != null;
                    
                    // Backend confirmed: equipped equipment CAN be washed (no inventory restriction)
                    bool shouldEnable = canAffordWash && hasEquipmentSelected;
                    
                    washButton.interactable = shouldEnable;
                }
                else
                {
                    Debug.LogWarning("[EquipmentForgeManager] Wash button is null - not assigned in Inspector");
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
        /// Fetch enhancement cost from backend API
        /// </summary>
        private void FetchEnhancementCost(Equipment equipment)
        {
            if (equipment == null)
            {
                return;
            }
            
            // Prevent duplicate calls for the same equipment
            if (lastFetchedEquipmentId == equipment.Id)
            {
                return;
            }
            lastFetchedEquipmentId = equipment.Id;
            
            EquipmentWebSocketApi equipmentApi = EquipmentWebSocketApi.Instance;
            if (equipmentApi == null)
            {
                return;
            }

            var apiParams = new
            {
                equipmentId = equipment.Id
            };

            
            equipmentApi.Action("enhancement_cost", apiParams, (response) =>
            {
                HandleEnhancementCostResponse(response);
            }, (errorResponse) =>
            {
                // Set default values on error
                ResetEnhancementData();
                UpdateResourceDisplays(true); // Refresh display
            });
        }
        
        /// <summary>
        /// Handle enhancement cost response from backend (updated for new API format)
        /// </summary>
        private void HandleEnhancementCostResponse(JObject response)
        {
            try
            {
                // Check for success wrapper first (backend might send wrapped responses)
                bool hasSuccessWrapper = response["success"] != null;
                JObject dataObj = hasSuccessWrapper ? response["data"] as JObject : response;
                
                if (dataObj != null && dataObj["current"] != null && dataObj["next"] != null && dataObj["cost"] != null)
                {
                    // Parse current level/attack data
                    var current = dataObj["current"];
                    currentLevel = current["level"]?.Value<int>() ?? 0;
                    currentAttack = current["attack"]?.Value<int>() ?? 0;
                    
                    // Parse next level/attack data
                    var next = dataObj["next"];
                    nextLevel = next["level"]?.Value<int>() ?? 0;
                    nextAttack = next["attack"]?.Value<int>() ?? 0;
                    
                    // Parse cost data
                    var cost = dataObj["cost"];
                    currentEnhanceCrystalCost = cost["crystals"]?.Value<int>() ?? 0;
                    currentEnhanceGoldCost = cost["gold"]?.Value<int>() ?? 0;
                    
                    // Parse other data
                    attackIncrease = dataObj["attack_increase"]?.Value<int>() ?? 0;
                    canAffordEnhancement = dataObj["can_afford"]?.Value<bool>() ?? false;
                    
                }
                else
                {
                    // Don't reset - let fallback calculation handle it
                }
                
                // Update displays with new data (whether from API or fallback)
                UpdateLevelProgressionDisplayFromAPI();
                if (enhancePanel != null && enhancePanel.activeInHierarchy)
                {
                    UpdateResourceDisplays(true);
                }
            }
            catch
            {
                // Don't reset - let fallback calculation handle it
                UpdateResourceDisplays(true);
            }
        }
        
        /// <summary>
        /// Reset enhancement data to default values
        /// </summary>
        private void ResetEnhancementData()
        {
            currentEnhanceCrystalCost = 0;
            currentEnhanceGoldCost = 0;
            canAffordEnhancement = false;
            currentLevel = 0;
            nextLevel = 0;
            currentAttack = 0;
            nextAttack = 0;
            attackIncrease = 0;
            lastFetchedEquipmentId = -1; // Reset to allow new equipment fetches
        }
        
        /// <summary>
        /// Perform single equipment enhancement
        /// </summary>
        private void PerformSingleEnhancement()
        {
            if (currentSelectedEquipment == null)
            {
                return;
            }
            
            if (!canAffordEnhancement)
            {
                // Could show user feedback here
                return;
            }
            
            EquipmentWebSocketApi equipmentApi = EquipmentWebSocketApi.Instance;
            if (equipmentApi == null)
            {
                return;
            }

            var apiParams = new
            {
                equipmentId = currentSelectedEquipment.Id
            };
            
            // Disable button to prevent double-clicking
            if (singleEnhanceButton != null)
            {
                singleEnhanceButton.interactable = false;
            }

            equipmentApi.Action("enhance", apiParams, (response) =>
            {
                HandleEnhancementResponse(response, false);
            }, (errorResponse) =>
            {
                // Re-enable button on error
                if (singleEnhanceButton != null)
                {
                    singleEnhanceButton.interactable = true;
                }
                // Could show user error message here
            });
        }
        
        /// <summary>
        /// Perform auto equipment enhancement using backend auto_enhance API
        /// </summary>
        private void PerformAutoEnhancement()
        {
            if (currentSelectedEquipment == null)
            {
                return;
            }
            
            if (!canAffordEnhancement)
            {
                return;
            }
            
            EquipmentWebSocketApi equipmentApi = EquipmentWebSocketApi.Instance;
            if (equipmentApi == null)
            {
                return;
            }
            
            // Use backend auto_enhance API with optional target level (defaults to max level 12)
            var apiParams = new
            {
                equipmentId = currentSelectedEquipment.Id,
                targetLevel = 12 // Let backend auto-enhance to maximum level
            };
            
            // Disable both buttons to prevent interference during auto enhancement
            if (singleEnhanceButton != null)
                singleEnhanceButton.interactable = false;
            if (autoEnhanceButton != null)
                autoEnhanceButton.interactable = false;
            
            equipmentApi.Action("auto_enhance", apiParams, (response) => {
                HandleEnhancementResponse(response, true); // isAutoEnhance = true
            }, (errorResponse) => {
                // Re-enable buttons on error
                if (singleEnhanceButton != null)
                    singleEnhanceButton.interactable = true;
                if (autoEnhanceButton != null)
                    autoEnhanceButton.interactable = true;
            });
        }
        
        /// <summary>
        /// Handle enhancement response from backend (updated for current API format)
        /// </summary>
        private void HandleEnhancementResponse(JObject response, bool isAutoEnhance)
        {
            try
            {
                // Check for new backend response format: {"code": 200, "data": {"success": true, ...}}
                bool hasCodeWrapper = response["code"] != null;
                int responseCode = hasCodeWrapper ? response["code"]?.Value<int>() ?? 400 : 200;
                JObject dataObj = hasCodeWrapper ? response["data"] as JObject : response;
                bool isSuccess = responseCode == 200 && (dataObj?["success"]?.Value<bool>() ?? true);
                
                if (isSuccess && dataObj != null)
                {
                    // Update equipment data from response
                    if (dataObj["updated_equipment"] != null)
                    {
                        var updatedEquipment = dataObj["updated_equipment"].ToObject<Equipment>();
                        if (updatedEquipment != null)
                        {
                            // Update the equipment in player profile
                            UpdateEquipmentInProfile(updatedEquipment);
                            
                            // Update current selected equipment reference
                            currentSelectedEquipment = updatedEquipment;
                            
                            // Refresh the forge block display (this will fetch new cost data)
                            LoadSelectedEquipmentToForgeBlock(updatedEquipment);
                        }
                    }
                    
                    // Update player profile if included in response (for resource costs)
                    if (dataObj["player_profile"] != null)
                    {
                        var updatedPlayer = dataObj["player_profile"].ToObject<Player>();
                        if (updatedPlayer != null)
                        {
                            PlayerProfile.Data.SetPlayer(updatedPlayer);
                        }
                    }
                    
                    // Log success feedback using new backend format
                    if (isAutoEnhance)
                    {
                        int enhancementsPerformed = dataObj["enhancements_performed"]?.Value<int>() ?? 0;
                        int finalLevel = dataObj["final_level"]?.Value<int>() ?? 0;
                    }
                    else
                    {
                        // Parse before/after data from new backend format
                        var before = dataObj["before"];
                        var after = dataObj["after"];
                        var costPaid = dataObj["cost_paid"];
                        int attackIncrease = dataObj["attack_increase"]?.Value<int>() ?? 0;
                        
                        if (before != null && after != null)
                        {
                            int beforeLevel = before["level"]?.Value<int>() ?? 0;
                            int afterLevel = after["level"]?.Value<int>() ?? 0;
                            int beforeAttack = before["attack"]?.Value<int>() ?? 0;
                            int afterAttack = after["attack"]?.Value<int>() ?? 0;
                            
                            if (costPaid != null)
                            {
                                int crystalsPaid = costPaid["crystals"]?.Value<int>() ?? 0;
                                int goldPaid = costPaid["gold"]?.Value<int>() ?? 0;
                            }
                        }
                        else
                        {
                        }
                    }
                }
                else
                {
                    string error = response["error"]?.Value<string>() ?? response["msg"]?.Value<string>() ?? "Unknown error";
                    
                    // Show detailed error information if available
                    if (dataObj?["data"] != null)
                    {
                        var errorData = dataObj["data"];
                        var required = errorData["required"];
                        var current = errorData["current"];
                        if (required != null && current != null)
                        {
                        }
                    }
                    // Could show user error message here
                }
            }
            catch
            {
            }
            finally
            {
                // Re-enable buttons
                if (singleEnhanceButton != null)
                {
                    singleEnhanceButton.interactable = true;
                }
                if (autoEnhanceButton != null)
                {
                    autoEnhanceButton.interactable = true;
                }
            }
        }
        
        /// <summary>
        /// Update equipment in player profile
        /// </summary>
        private void UpdateEquipmentInProfile(Equipment updatedEquipment)
        {
            if (PlayerProfile.Data?.Player?.Equipments == null || updatedEquipment == null)
            {
                return;
            }
            
            var equipments = PlayerProfile.Data.Player.Equipments;
            for (int i = 0; i < equipments.Count; i++)
            {
                if (equipments[i].Id == updatedEquipment.Id)
                {
                    equipments[i] = updatedEquipment;
                    break;
                }
            }
            
            // Notify listeners of equipment changes
            PlayerProfile.Data.NotifyListeners("Equipments");
        }
        
        /// <summary>
        /// Perform equipment washing using backend wash API
        /// </summary>
        private void PerformWash()
        {
            if (currentSelectedEquipment == null)
            {
                Debug.LogWarning("[EquipmentForgeManager] No equipment selected for washing");
                return;
            }
            
            // Check if player has enough crystals (200 crystals required)
            Player player = PlayerProfile.Data?.Player;
            int playerCrystals = GetCrystalCount(player);
            const int WASH_COST = 200;
            
            if (playerCrystals < WASH_COST)
            {
                Debug.LogWarning($"[EquipmentForgeManager] Insufficient crystals for washing. Need {WASH_COST}, have {playerCrystals}");
                // Could show user feedback here
                return;
            }
            
            // Backend confirmed: equipped equipment CAN be washed (no inventory restriction removed)
            
            EquipmentWebSocketApi equipmentApi = EquipmentWebSocketApi.Instance;
            if (equipmentApi == null)
            {
                Debug.LogError("[EquipmentForgeManager] EquipmentWebSocketApi.Instance is null");
                return;
            }

            var apiParams = new
            {
                equipmentId = currentSelectedEquipment.Id
            };
            
            
            // Disable wash button to prevent double-clicking
            if (washButton != null)
            {
                washButton.interactable = false;
            }

            Debug.Log($"[EquipmentForgeManager] About to call wash API with equipment ID: {currentSelectedEquipment.Id}");
            
            equipmentApi.Action("wash", apiParams, (response) =>
            {
                Debug.Log($"[EquipmentForgeManager] Wash API success callback called with response: {response}");
                HandleWashResponse(response);
            }, (errorResponse) =>
            {
                Debug.LogError($"[EquipmentForgeManager] Wash API error callback called with: {errorResponse}");
                Debug.LogError($"[EquipmentForgeManager] Error response type: {errorResponse.GetType()}");
                
                // Re-enable button on error
                if (washButton != null)
                {
                    washButton.interactable = true;
                }
                
                // Could show user error message here
            });
        }
        
        /// <summary>
        /// Handle wash response from backend
        /// </summary>
        private void HandleWashResponse(JObject response)
        {
            try
            {
                // Check standard response format: {"action": "wash", "code": 200, "data": {...}}
                int responseCode = response["code"]?.Value<int>() ?? 400;
                JObject dataObj = response["data"] as JObject;
                
                if (responseCode == 200 && dataObj != null)
                {
                    bool success = dataObj["success"]?.Value<bool>() ?? false;
                    
                    if (success)
                    {
                        // Parse wash results
                        int equipmentId = dataObj["equipment_id"]?.Value<int>() ?? 0;
                        int costPaid = dataObj["cost_paid"]?.Value<int>() ?? 0;
                        
                        // Parse old and new attributes
                        var oldAttributes = dataObj["old_attributes"] as JObject;
                        var newAttributes = dataObj["new_attributes"] as JObject;
                        
                        Debug.Log($"[EquipmentForgeManager] 💰 Cost paid: {costPaid} crystals");
                        
                        if (oldAttributes != null && newAttributes != null)
                        {
                            Debug.Log("[EquipmentForgeManager] 🔄 Attribute changes:");
                            
                            // Log old attributes
                            foreach (var attr in oldAttributes)
                            {
                                Debug.Log($"  Old: {attr.Key} = {attr.Value}");
                            }
                            
                            // Log new attributes  
                            foreach (var attr in newAttributes)
                            {
                                Debug.Log($"  New: {attr.Key} = {attr.Value}");
                            }
                        }
                        
                        // Update equipment data from response
                        if (dataObj["updated_equipment"] != null)
                        {
                            var updatedEquipment = dataObj["updated_equipment"].ToObject<Equipment>();
                            if (updatedEquipment != null)
                            {
                                // Update the equipment in player profile
                                UpdateEquipmentInProfile(updatedEquipment);
                                
                                // Update current selected equipment reference
                                currentSelectedEquipment = updatedEquipment;
                                
                                // Refresh the forge block display
                                LoadSelectedEquipmentToForgeBlock(updatedEquipment);
                                
                            }
                        }
                        
                        // Update player profile (for crystal count)
                        if (dataObj["player_profile"] != null)
                        {
                            var updatedPlayer = dataObj["player_profile"].ToObject<Player>();
                            if (updatedPlayer != null)
                            {
                                PlayerProfile.Data.SetPlayer(updatedPlayer);
                            }
                        }
                        
                        // Update resource displays
                        UpdateResourceDisplays(false); // Refresh upgrade panel displays
                        
                        // TODO: Add visual animation for before/after attributes comparison
                        // This could show a popup or animate the attribute changes
                    }
                    else
                    {
                        Debug.LogError("[EquipmentForgeManager] Wash failed: success = false");
                    }
                }
                else
                {
                    // Handle error response - backend uses data.msg format for errors
                    string error = dataObj?["msg"]?.Value<string>() ?? response["error"]?.Value<string>() ?? "Unknown error";
                    Debug.LogError($"[EquipmentForgeManager] Wash failed: {error}");
                    
                    // Could show user error message here (e.g., "Insufficient crystals")
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[EquipmentForgeManager] Error processing wash response: {e.Message}");
            }
            finally
            {
                // Re-enable wash button
                if (washButton != null)
                {
                    washButton.interactable = true;
                }
            }
        }
        
    }
}