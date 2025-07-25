using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

        private void Awake()
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
        }

        public void OpenGemMergePage()
        {
            if (gemMergePage != null)
            {
                gemMergePage.SetActive(true);
                ReloadBlockItemsForMerge();
                Debug.Log("[GemMergePageManager] Gem merge page opened");
            }
        }

        public void CloseGemMergePage()
        {
            if (gemMergePage != null)
            {
                gemMergePage.SetActive(false);
                Debug.Log("[GemMergePageManager] Gem merge page closed");
            }
        }

        private void ReloadBlockItemsForMerge()
        {
            if (gemMergeContentPanel == null)
            {
                Debug.LogError("[GemMergePageManager] gemMergeContentPanel is null");
                return;
            }

            // Clear existing items in Gem Merge page
            foreach (Transform child in gemMergeContentPanel)
            {
                Destroy(child.gameObject);
            }

            // Find the source content panel based on active menu
            Transform sourceContentPanel = GetSourceContentPanel();
            if (sourceContentPanel == null)
            {
                Debug.LogError("[GemMergePageManager] Could not find source content panel");
                return;
            }

            // Copy over items from source content panel
            foreach (Transform block in sourceContentPanel)
            {
                GameObject newBlock = Instantiate(block.gameObject, gemMergeContentPanel);
                newBlock.name = block.name;
            }

            // Adjust the GridLayoutGroup for the new blocks
            UpdateGridLayoutForGemMerge(sourceContentPanel);
        }

        private Transform GetSourceContentPanel()
        {
            MenuController menuController = FindObjectOfType<MenuController>();
            if (menuController == null)
            {
                Debug.LogError("[GemMergePageManager] MenuController not found");
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
                Debug.LogError("[GemMergePageManager] gemMergeGridLayout reference is missing");
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
    }
}