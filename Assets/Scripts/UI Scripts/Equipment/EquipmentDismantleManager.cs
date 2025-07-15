using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using model;
using TMPro;

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
        
        public static EquipmentDismantleManager Instance;
        
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
                Debug.Log("[DismantleManager] HeroMenu is active - using Hero context");
            }
            else if (allyMenu != null && allyMenu.activeInHierarchy)
            {
                currentContext = DismantleContext.Ally;
                Debug.Log("[DismantleManager] AllyMenu is active - using Ally context");
            }
            else
            {
                currentContext = context; // fallback to passed context
                Debug.Log($"[DismantleManager] No menu detected as active - using fallback context: {context}");
            }
            
            dismantlePage.SetActive(true);
            commonPage.SetActive(false);
            CloseEquipComp();
            heroStep2Panel.SetActive(false);
            allyStep2Panel.SetActive(false);
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
            Debug.Log("✅ DismantlePage Data Loaded Successfully!");
        }

        
        public void LoadDismantlePagePack()
        {
            if (dismantlePackContent == null)
            {
                Debug.LogError("❌ DismantlePage Pack Content NOT assigned in Inspector!");
                return;
            }

            // Clear existing items in DismantlePage Pack
            foreach (Transform child in dismantlePackContent)
            {
                Destroy(child.gameObject);
            }

            // Get the correct source based on current context
            Transform sourcePanel = currentContext == DismantleContext.Hero ? heroSourceContentPanel : allySourceContentPanel;
            
            // Clone each item from the appropriate source Pack
            if (sourcePanel != null)
            {
                foreach (Transform item in sourcePanel)
                {
                    GameObject newItem = Instantiate(item.gameObject, dismantlePackContent);
                    newItem.name = item.name; // Keep the same name
                }
                Debug.Log($"✅ DismantlePage Pack Loaded Successfully from {currentContext} source!");
            }
            else
            {
                Debug.LogError($"❌ {currentContext} Source Content Panel NOT assigned in Inspector!");
            }

            // Apply dynamic grid adjustments with delayed setup to ensure RectTransform is properly sized
            if (dismantlePackGrid != null)
            {
                StartCoroutine(DelayedSetupDismantlePackGrid());
            }
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
                Debug.LogWarning($"[DismantleManager] dismantlePackContent width is too small ({panelWidth}), using fallback calculation");
                
                // Get width from parent or use a reasonable fallback
                RectTransform parentRect = dismantleContentRect.parent as RectTransform;
                if (parentRect != null && parentRect.rect.width > 10f)
                {
                    panelWidth = parentRect.rect.width;
                    Debug.Log($"[DismantleManager] Using parent width: {panelWidth}");
                }
                else
                {
                    // Last resort fallback - use a standard screen proportion
                    panelWidth = Screen.width * 0.8f;
                    Debug.Log($"[DismantleManager] Using screen-based fallback width: {panelWidth}");
                }
            }
            
            int blocksPerRow = 5;
            blockWidth = panelWidth / (blocksPerRow + 1);
            leftPadding = blockWidth * 0.25f;
            rightPadding = blockWidth * 0.25f;
            spacingX = blockWidth * 0.125f;
            spacingY = blockWidth * 0.125f;
            
            Debug.Log($"[DismantleManager] Grid setup - panelWidth: {panelWidth}, blockWidth: {blockWidth}, context: {currentContext}");
            
            dismantlePackGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            dismantlePackGrid.constraintCount = 5;
            dismantlePackGrid.cellSize = new Vector2(blockWidth, blockWidth);
            dismantlePackGrid.spacing = new Vector2(spacingX, spacingY);
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

            // Return to the correct panel based on context
            if (currentContext == DismantleContext.Hero)
            {
                Debug.Log("[DismantleManager] Returning to Hero Step 2 Panel.");
                heroStep2Panel.SetActive(true);
            }
            else
            {
                Debug.Log("[DismantleManager] Returning to Ally Step 2 Panel.");
                allyStep2Panel.SetActive(true);
                heroStep2Panel.SetActive(true);
            }

            congratsPage.SetActive(false);
            dismantlePack.SetActive(true);
            MoveBlockUp();


            Debug.Log($"✅ DismantlePage Block Cleared on Close! Returned to {currentContext} panel.");
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
    }
}