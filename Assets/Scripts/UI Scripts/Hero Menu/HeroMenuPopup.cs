using UnityEngine;
using UnityEngine.UI;

public class HeroMenuPopup : MonoBehaviour
{
    public GameObject upDownMenu; 
    public GameObject heroStep2; 
    public GameObject heroPopup; // Parent panel (semi-transparent background)
    public GameObject skinPage;
    public GameObject gunPage;
    public GameObject gemPage;
    public GameObject gemMergePage;

    public Button skinButton;
    public Button gunButton;
    public Button gemButton;
    public Button gemMergeButton;
    
    public Button closeGunButton;
    public Button closeGemButton;
    public Button closeGemMergeButton;
    public Button closeSkinButton; // Added close button for the skin page

    public Transform heroStep2ContentPanel;  // The original content panel that holds the BlockItems
    public Transform gemMergeContentPanel;  // The content panel on the Gem Merge Page
    public GridLayoutGroup gemMergeGridLayout; // Reference to the GridLayoutGroup in Gem Merge Page

    public ScrollRect gemMergeScrollView;  // ScrollView in Gem Merge Page
    public RectTransform gemMergeViewport;  // Viewport in Gem Merge Page


    void Start()
    {
        // Ensure all popups are initially hidden
        heroPopup.SetActive(false);
        skinPage.SetActive(false);
        gunPage.SetActive(false);
        gemPage.SetActive(false);

        // Assign button listeners
        skinButton.onClick.AddListener(OpenSkinPage);
        gunButton.onClick.AddListener(OpenGunPage);
        gemButton.onClick.AddListener(OpenGemPage);
        gemMergeButton.onClick.AddListener(OpenGemMergePage); // Changed from ToggleGemPage to OpenGemPage

        closeGunButton.onClick.AddListener(CloseGunPage);
        closeGemButton.onClick.AddListener(CloseGemPage);
        closeSkinButton.onClick.AddListener(CloseSkinPage); // Assign listener for skin page close button
    }

    private void OpenSkinPage()
    {
        heroPopup.SetActive(true);
        skinPage.SetActive(true);
        heroStep2.SetActive(false);
        upDownMenu.SetActive(false);
        Debug.Log("Skin panel opened.");
    }

    private void OpenGunPage()
    {
        heroPopup.SetActive(true);
        gunPage.SetActive(true);
        heroStep2.SetActive(false);
        upDownMenu.SetActive(false);
        Debug.Log("Gun panel opened.");
    }

    private void OpenGemPage()
    {
        heroPopup.SetActive(true);
        gemPage.SetActive(true);
        heroStep2.SetActive(false);
        Debug.Log("Gem panel opened.");
    }

    public void OpenGemMergePage()
    {
        heroPopup.SetActive(true);
        gemMergePage.SetActive(true);
        heroStep2.SetActive(false);
        ReloadBlockItemsForMerge();  
        Debug.Log("Gem panel opened.");
    }

    // Reload BlockItems from Hero Step 2 to Gem Merge Page (non-clickable)
    private void ReloadBlockItemsForMerge()
    {
        // Clear existing items in Gem Merge page
        foreach (Transform child in gemMergeContentPanel)
        {
            Destroy(child.gameObject);
        }

        // Copy over items from Hero Step 2's content panel
        foreach (Transform block in heroStep2ContentPanel)
        {
            GameObject newBlock = Instantiate(block.gameObject, gemMergeContentPanel);
            newBlock.name = block.name;  // Copy the name of the block
        }

        // Adjust the GridLayoutGroup for the new blocks in the Gem Merge Page
        UpdateGridLayoutForGemMerge();
    }

    // Adjust the GridLayoutGroup for the Gem Merge Page
    private void UpdateGridLayoutForGemMerge()
    {
        if (gemMergeGridLayout == null)
        {
            Debug.LogError("GridLayoutGroup reference is missing in Gem Merge Page!");
            return;
        }

        // Copy the layout settings from Hero Step 2
        GridLayoutGroup heroGridLayout = heroStep2ContentPanel.GetComponent<GridLayoutGroup>();

        if (heroGridLayout != null)
        {
            gemMergeGridLayout.cellSize = heroGridLayout.cellSize;
            gemMergeGridLayout.spacing = heroGridLayout.spacing;
            gemMergeGridLayout.padding = heroGridLayout.padding;
            gemMergeGridLayout.constraint = heroGridLayout.constraint;
            gemMergeGridLayout.constraintCount = heroGridLayout.constraintCount;
        }

        // Rebuild the layout to ensure proper spacing and alignment
        LayoutRebuilder.ForceRebuildLayoutImmediate(gemMergeContentPanel.GetComponent<RectTransform>());
    }

    public void CloseSkinPage()
    {
        skinPage.SetActive(false);
        heroStep2.SetActive(true);
        Debug.Log("Skin panel closed.");
        CheckAndCloseHeroPopup();
    }

    public void CloseGunPage()
    {
        gunPage.SetActive(false);
        heroStep2.SetActive(true);
        Debug.Log("Gun panel closed.");
        CheckAndCloseHeroPopup();
    }

    public void CloseGemPage()
    {
        gemPage.SetActive(false);
        heroStep2.SetActive(true);
        Debug.Log("Gem panel closed.");
        CheckAndCloseHeroPopup();
    }

    public void CloseGemMergePage()
    {
        gemMergePage.SetActive(false);
        heroStep2.SetActive(true);
        Debug.Log("Gem panel closed.");
        CheckAndCloseHeroPopup();
    }

    private void CheckAndCloseHeroPopup()
    {
        // Close the parent panel if no pages are active
        if (!skinPage.activeSelf && !gunPage.activeSelf && !gemPage.activeSelf && !gemMergePage.activeSelf)
        {
            heroPopup.SetActive(false);
            heroStep2.SetActive(true);
            upDownMenu.SetActive(true);
            Debug.Log("HeroPopup closed.");
        }
    }
}
