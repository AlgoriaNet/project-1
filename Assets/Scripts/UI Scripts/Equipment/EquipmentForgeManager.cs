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
        public GameObject step3Panel;
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
            CloseStep3();
            heroStep2Panel.SetActive(false);
            allyStep2Panel.SetActive(false);
            LoadForgePagePack();

            // Add button listeners when ForgePage opens
            enhanceButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.RemoveAllListeners();
            enhanceButton.onClick.AddListener(() => ToggleEnhanceUpgrade(true));
            upgradeButton.onClick.AddListener(() => ToggleEnhanceUpgrade(false));

            // Set default state
            ToggleEnhanceUpgrade(true);
        }
        
        private void CloseStep3()
        {
            if (step3Panel != null)
            {
                step3Panel.SetActive(false);
            }
        }
        
        public void LoadForgePageData()
        {
            if (forgeBlock == null)
            {
                Debug.LogError("❌ ForgePage Block NOT assigned in Inspector!");
                return;
            }

            // Reference Hero Step 3 → Page_1 → UpperGroup → Block
            Transform heroBlock = page1.transform.Find("UpperGroup/Block");
            if (heroBlock == null)
            {
                Debug.LogError("❌ Hero Step 3 Block NOT found!");
                return;
            }

            // Copy TopText
            TextMeshProUGUI heroTopText = heroBlock.Find("TopText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI forgeTopText = forgeBlock.transform.Find("TopText")?.GetComponent<TextMeshProUGUI>();
            if (heroTopText != null && forgeTopText != null)
            {
                forgeTopText.text = heroTopText.text;
            }

            // Copy Image
            Image heroImage = heroBlock.Find("Image")?.GetComponent<Image>();
            Image forgeImage = forgeBlock.transform.Find("Image")?.GetComponent<Image>();
            if (heroImage != null && forgeImage != null)
            {
                forgeImage.sprite = heroImage.sprite;
                forgeImage.color = Color.white; // Ensure visibility
            }

            Debug.Log("✅ ForgePage Block Updated Successfully!");
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

            // Apply dynamic grid adjustments
            if (forgePackGrid != null)
            {
                SetupForgePackGrid();
            }
        }
        
        private void SetupForgePackGrid()
        {
            // Try to get grid settings from HeroBlockSetup
            HeroBlockSetup heroBlockSetup = FindObjectOfType<HeroBlockSetup>();
            if (heroBlockSetup != null)
            {
                // Access the grid layout variables through reflection or make them public
                // For now, use default values similar to HeroBlockSetup
                float panelWidth = forgePackContent.GetComponent<RectTransform>().rect.width;
                int blocksPerRow = 5;
                
                blockWidth = panelWidth / (blocksPerRow + 1);
                leftPadding = blockWidth * 0.25f;
                rightPadding = blockWidth * 0.25f;
                spacingX = blockWidth * 0.125f;
                spacingY = blockWidth * 0.125f;
            }
            
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

            // Clear ForgeBlock TopText and Image
            TextMeshProUGUI forgeTopText = forgeBlock.transform.Find("TopText")?.GetComponent<TextMeshProUGUI>();
            Image forgeImage = forgeBlock.transform.Find("Image")?.GetComponent<Image>();

            if (forgeTopText != null) forgeTopText.text = "";
            if (forgeImage != null)
            {
                forgeImage.sprite = null;
                forgeImage.color = new Color(0, 0, 0, 0); // Fully transparent
            }

            Debug.Log($"✅ ForgePage Block Cleared on Close! Returned to {currentContext} panel.");
        }
    }
}