using UnityEngine;
using TMPro;

public class IndexDynamicSize : MonoBehaviour
{
    public RectTransform topPanel;       // User topPanel RectTransform
    public RectTransform indexGroup;     // Index Group RectTransform
    public RectTransform index1;         // Index 1 RectTransform
    public RectTransform index2;         // Index 2 RectTransform
    public RectTransform index3;         // Index 3 RectTransform
    public RectTransform index4;         // Index 3 RectTransform

    public TextMeshProUGUI diamondText;
    public TextMeshProUGUI index2Text;
    public TextMeshProUGUI index3Text;
    public TextMeshProUGUI index4Text;

    void Start()
    {
        // Dynamically adjust the layout
        AdjustLayout();

        int diamond = model.PlayerProfile.Data.Player.Diamond;
        if (diamondText != null) diamondText.text = diamond.ToString();
    }

    public void RefreshDiamondDisplay()
    {
        int diamond = model.PlayerProfile.Data.Player.Diamond;
        if (diamondText != null) diamondText.text = diamond.ToString();
    }

    private void AdjustLayout()
    {
        float panelWidth = topPanel.rect.width; // Use the rendered width of the Top Panel

        float indexGroupWidth = panelWidth * 0.8f; // Remaining space after user
        float indexWidth = indexGroupWidth / 8f;     // Divide equally among indexes
        float indexGroupHeight = indexGroup.rect.height / 2f;

        // Set Index Group size
        indexGroup.sizeDelta = new Vector2(indexGroupWidth, indexGroupHeight);

        // Set Index sizes dynamically
        index1.sizeDelta = new Vector2(indexWidth, indexGroupHeight);
        index2.sizeDelta = new Vector2(indexWidth, indexGroupHeight);
        index3.sizeDelta = new Vector2(indexWidth, indexGroupHeight);
        index4.sizeDelta = new Vector2(indexWidth, indexGroupHeight);
    }
}
