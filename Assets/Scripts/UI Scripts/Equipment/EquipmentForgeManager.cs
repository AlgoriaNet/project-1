using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
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
        public GameObject page1;
        
        [Header("Menu References")]
        public GameObject heroMenu;
        public GameObject allyMenu;
        
        [Header("Pack Sources")]
        public Transform heroSourceContentPanel; // Reference to Hero menu contentPanel
        public Transform allySourceContentPanel; // Reference to Ally menu contentPanel
        
        public static EquipmentForgeManager Instance;
        
        public enum ForgeContext
        {
            Hero,
            Ally
        }
        
        private ForgeContext currentContext;
        
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
        
        public void OpenForgePageFromHero()
        {
            OpenForgePage(ForgeContext.Hero);
        }
        
        public void OpenForgePageFromAlly()
        {
            OpenForgePage(ForgeContext.Ally);
        }
        
        public void OpenForgePage(ForgeContext context = ForgeContext.Hero)
        {
            // Determine context based on which menu is currently active
            if (heroMenu != null && heroMenu.activeInHierarchy)
            {
                currentContext = ForgeContext.Hero;
                Debug.Log("[ForgeManager] HeroMenu is active - using Hero context");
            }
            else if (allyMenu != null && allyMenu.activeInHierarchy)
            {
                currentContext = ForgeContext.Ally;
                Debug.Log("[ForgeManager] AllyMenu is active - using Ally context");
            }
            else
            {
                currentContext = context; // fallback to passed context
                Debug.Log($"[ForgeManager] No menu detected as active - using fallback context: {context}");
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

            // Set default state
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
                Debug.LogError("❌ ForgePage Block NOT assigned in Inspector!");
                return;
            }

            // Get the selected equipment from EquipmentComparisonManager
            Equipment selectedEquipment = null;
            if (EquipmentComparisonManager.Instance != null && EquipmentComparisonManager.Instance.ComparedEquippedId > 0)
            {
                selectedEquipment = PlayerProfile.Data.Player.Equipments.Find(equipment => 
                    equipment.Id == EquipmentComparisonManager.Instance.ComparedEquippedId);
            }

            if (selectedEquipment == null)
            {
                Debug.LogError("❌ No equipment selected for forging!");
                return;
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
                Debug.LogError("❌ EquipImage NOT found in ForgeBlock! Expected hierarchy: Block/EquipImage");
                return;
            }

            if (equipNameText == null)
            {
                Debug.LogError("❌ EquipNameText NOT found in ForgeBlock! Expected hierarchy: Block/EquipNameText");
                return;
            }

            // Load equipment image
            string imagePath = $"UILoading/Equipment/{equipment.Name}";
            Sprite equipmentSprite = Resources.Load<Sprite>(imagePath);
            if (equipmentSprite != null)
            {
                equipImage.sprite = equipmentSprite;
                equipImage.color = Color.white; // Ensure visibility
                Debug.Log($"✅ Loaded equipment image: {imagePath}");
            }
            else
            {
                Debug.LogError($"❌ Equipment sprite not found at path: {imagePath}");
                equipImage.color = Color.clear; // Hide if no sprite found
            }

            // Set equipment name (extract equipment type from name, e.g., "Chest" from "Chest_06")
            string equipmentDisplayName = equipment.Name;
            if (equipment.Name.Contains("_"))
            {
                equipmentDisplayName = equipment.Name.Split('_')[0]; // Get "Chest" from "Chest_06"
            }
            equipNameText.text = equipmentDisplayName;

            // Set background color based on equipment quality
            if (backgroundImage != null)
            {
                Color qualityColor = ItemLoader.quantityColor.GetValueOrDefault(equipment.Quality, Color.white);
                backgroundImage.color = qualityColor;
                Debug.Log($"✅ Set equipment quality color: Quality {equipment.Quality} = {qualityColor}");
            }
            else
            {
                Debug.LogWarning("⚠️ No background Image component found on ForgeBlock for quality color");
            }

            Debug.Log($"✅ ForgePage Block Updated Successfully with {equipment.Name} (ID: {equipment.Id}, Quality: {equipment.Quality})!");
        }
        
        public void LoadForgePagePack()
        {
            if (forgePackContent == null)
            {
                Debug.LogError("❌ ForgePage Pack Content NOT assigned in Inspector!");
                return;
            }

            // Clear existing items in ForgePage Pack
            foreach (Transform child in forgePackContent)
            {
                Destroy(child.gameObject);
            }

            // Get the correct source based on current context
            Transform sourcePanel = currentContext == ForgeContext.Hero ? heroSourceContentPanel : allySourceContentPanel;
            
            // Clone each item from the appropriate source Pack
            if (sourcePanel != null)
            {
                foreach (Transform item in sourcePanel)
                {
                    GameObject newItem = Instantiate(item.gameObject, forgePackContent);
                    newItem.name = item.name; // Keep the same name
                }
                Debug.Log($"✅ ForgePage Pack Loaded Successfully from {currentContext} source!");
            }
            else
            {
                Debug.LogError($"❌ {currentContext} Source Content Panel NOT assigned in Inspector!");
            }

            // Apply dynamic grid adjustments with delayed setup to ensure RectTransform is properly sized
            if (forgePackGrid != null)
            {
                StartCoroutine(DelayedSetupForgePackGrid());
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
                Debug.LogWarning($"[ForgeManager] forgePackContent width is too small ({panelWidth}), using fallback calculation");
                
                // Get width from parent or use a reasonable fallback
                RectTransform parentRect = forgeContentRect.parent as RectTransform;
                if (parentRect != null && parentRect.rect.width > 10f)
                {
                    panelWidth = parentRect.rect.width;
                    Debug.Log($"[ForgeManager] Using parent width: {panelWidth}");
                }
                else
                {
                    // Last resort fallback - use a standard screen proportion
                    panelWidth = Screen.width * 0.8f;
                    Debug.Log($"[ForgeManager] Using screen-based fallback width: {panelWidth}");
                }
            }
            
            int blocksPerRow = 5;
            blockWidth = panelWidth / (blocksPerRow + 1);
            leftPadding = blockWidth * 0.25f;
            rightPadding = blockWidth * 0.25f;
            spacingX = blockWidth * 0.125f;
            spacingY = blockWidth * 0.125f;
            
            Debug.Log($"[ForgeManager] Grid setup - panelWidth: {panelWidth}, blockWidth: {blockWidth}, context: {currentContext}");
            
            forgePackGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            forgePackGrid.constraintCount = 5;
            forgePackGrid.cellSize = new Vector2(blockWidth, blockWidth);
            forgePackGrid.spacing = new Vector2(spacingX, spacingY);
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
        }
        
        public void CloseForgePage()
        {
            forgePage.SetActive(false);
            commonPage.SetActive(true);

            // Return to the correct panel based on context
            if (currentContext == ForgeContext.Hero)
            {
                Debug.Log("[ForgeManager]Returning to Hero Step 2 Panel.");
                heroStep2Panel.SetActive(true);
            }
            else
            {
                Debug.Log("[ForgeManager]Returning to Ally Step 2 Panel.");
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

            Debug.Log($"✅ ForgePage Block Cleared on Close! Returned to {currentContext} panel.");
        }
    }
}