using UnityEngine;

public class Row1GroupDynamicSize : MonoBehaviour
{
    public RectTransform topPanel;       // User topPanel RectTransform
    public RectTransform userIcon;       // User Icon RectTransform
    public RectTransform nameText;       // Name Text RectTransform
    public RectTransform indexGroup;     // Index Group RectTransform
    public RectTransform index1;         // Index 1 RectTransform
    public RectTransform index2;         // Index 2 RectTransform
    public RectTransform index3;         // Index 3 RectTransform

    void Start()
    {
        float panelWidth = topPanel.rect.width; // Use the rendered width of the Top Panel
        float userIconWidth = panelWidth * 0.125f;
        float nameTextWidth = panelWidth * 0.2f;

        // Debug: Print values for verification
        Debug.Log($"userIconWidth: {userIconWidth}");
        Debug.Log($"nameTextWidth: {nameTextWidth}");

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

        // Debug: Print values for verification
        Debug.Log($"Index Group Width: {indexGroupWidth}, Height: {indexGroupHeight}");
        Debug.Log($"Index 1 Width: {index1.rect.width}, Height: {index1.rect.height}");
        Debug.Log($"Index 2 Width: {index2.rect.width}, Height: {index2.rect.height}");
        Debug.Log($"Index 3 Width: {index3.rect.width}, Height: {index3.rect.height}");
    }
}