using UnityEngine;

public class IndexDynamicSize : MonoBehaviour
{
    public RectTransform topPanel;       // User topPanel RectTransform
    public RectTransform indexGroup;     // Index Group RectTransform
    public RectTransform index1;         // Index 1 RectTransform
    public RectTransform index2;         // Index 2 RectTransform
    public RectTransform index3;         // Index 3 RectTransform
    public RectTransform index4;         // Index 3 RectTransform

    void Start()
    {
        // Dynamically adjust the layout
        AdjustLayout();
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
