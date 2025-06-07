using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Row1GroupDynamicSize : MonoBehaviour
{
    public RectTransform topPanel;       // User topPanel RectTransform
    public RectTransform userIcon;       // User Icon RectTransform
    public RectTransform nameText;       // Name Text RectTransform
    public RectTransform indexGroup;     // Index Group RectTransform
    public RectTransform index1;         // Index 1 RectTransform
    public RectTransform index2;         // Index 2 RectTransform
    public RectTransform index3;         // Index 3 RectTransform

    public Image userIconImage;          // Image component for user icon
    public TextMeshProUGUI topLevelNameText; // TextMeshPro for username
    public TextMeshProUGUI expText;
    public TextMeshProUGUI diamondText;
    public TextMeshProUGUI goldCoinText;

    void Start()
    {
        // Retrieve stored values from PlayerPrefs
        string username = PlayerPrefs.GetString("Username", "DefaultName"); 
        string userIconPath = PlayerPrefs.GetString("UserIconPath", "UILoading/CharacterImages/UserIcons/WhiteBorderIcons/00.png"); 

        // Update the UI
        if (topLevelNameText != null)
        {
            topLevelNameText.text = username;
        }

        if (userIconImage != null)
        {
            // Load user icon dynamically (ensure the icons are in the Resources folder)
            Sprite userIconSprite = Resources.Load<Sprite>(userIconPath);
            if (userIconSprite != null)
            {
                userIconImage.sprite = userIconSprite;
            }
            else
            {
                Debug.LogWarning($"User icon not found at path: {userIconPath}. Using default.");
            }
        }

        int exp = model.PlayerProfile.Data.Player.Exp;
        int gold = model.PlayerProfile.Data.Player.GoldCoin;
        int diamond = model.PlayerProfile.Data.Player.Diamond;

        if (expText != null) expText.text = exp.ToString();
        if (diamondText != null) diamondText.text = diamond.ToString();
        if (goldCoinText != null) goldCoinText.text = gold.ToString();    

        // Dynamically adjust the layout
        AdjustLayout();
    }

    public void RefreshCurrencyDisplay()
    {
        int gold = model.PlayerProfile.Data.Player.GoldCoin;
        int diamond = model.PlayerProfile.Data.Player.Diamond;
        if (goldCoinText != null) goldCoinText.text = gold.ToString();
        if (diamondText != null) diamondText.text = diamond.ToString();
    }

    private void AdjustLayout()
    {
        float panelWidth = topPanel.rect.width; // Use the rendered width of the Top Panel
        float userIconWidth = panelWidth * 0.125f;
        float nameTextWidth = panelWidth * 0.2f;

        // Set User section width and child alignment
        userIcon.sizeDelta = new Vector2(userIconWidth, userIconWidth); // Square
        nameText.sizeDelta = new Vector2(nameTextWidth, nameText.sizeDelta.y);

        float indexGroupWidth = panelWidth * 0.5f; // Remaining space after user
        float indexWidth = indexGroupWidth / 6f;     // Divide equally among indexes
        float indexGroupHeight = indexGroup.rect.height / 2f;

        // Set Index Group size
        indexGroup.sizeDelta = new Vector2(indexGroupWidth, indexGroupHeight);

        // Set Index sizes dynamically
        index1.sizeDelta = new Vector2(indexWidth, indexGroupHeight);
        index2.sizeDelta = new Vector2(indexWidth, indexGroupHeight);
        index3.sizeDelta = new Vector2(indexWidth, indexGroupHeight);
    }
}