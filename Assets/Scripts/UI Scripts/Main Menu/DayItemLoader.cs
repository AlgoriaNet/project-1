using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class DayItemLoader : MonoBehaviour
{
    public GameObject dayItemPrefab; // Assign the DayItem prefab in the Inspector
    public Transform contentTransform; // Assign the Content transform (with GridLayoutGroup) in the Inspector
    public RectTransform viewportTransform; // Assign the Viewport RectTransform in the Inspector
    public RectTransform greenLine; // Assign the GreenLine RectTransform in the Inspector
    public RectTransform greyLine; // Assign the GreyLine RectTransform in the Inspector
    public Transform block7D; // Assign the Block_7D parent transform
    public Transform block14D; // Assign the Block_14D parent transform
    public Transform block21D; // Assign the Block_21D parent transform
    public Transform block30D; // Assign the Block_30D parent transform
    public ResourceManager resourceManager; // Assign the ResourceManager in the Inspector

    private GridLayoutGroup gridLayoutGroup;
    private RectTransform contentRect;

    private const int columns = 5;
    private const float spacingRatio = 1f / 13f; // Spacing is 1/13 of DayItem width
    private const float aspectRatio = 0.86f; // Width/Height ratio from AspectRatioFitter
    private const string totalClaimedDaysKey = "TotalClaimedDays"; // Key for 30-day cycle
    private const int reclaimGemCost = 20; // Cost to reclaim a day

    private int daysInMonth; // Number of days in the current month
    private int rows; // Number of rows needed based on daysInMonth

    public static DateTime loginTime; // Store the login time
    public static string detectedLanguage; // Store the detected language

    public void Start()
    {
        gridLayoutGroup = contentTransform.GetComponent<GridLayoutGroup>();
        contentRect = contentTransform.GetComponent<RectTransform>();

        if (gridLayoutGroup == null || contentRect == null || dayItemPrefab == null || viewportTransform == null ||
            greenLine == null || greyLine == null || block7D == null || block14D == null || block21D == null || block30D == null ||
            resourceManager == null)
        {
            Debug.LogError("Required components (GridLayoutGroup, Content RectTransform, DayItem Prefab, Viewport, GreenLine, GreyLine, Block_XD, or ResourceManager) are missing.");
            return;
        }

        // Store the login time
        loginTime = DateTime.Now;
        Debug.Log($"Login Time: {loginTime}");

        // Detect and store the system language
        detectedLanguage = Application.systemLanguage.ToString();
        Debug.Log($"Detected Language: {detectedLanguage}");

        // Calculate days in the current month
        DateTime now = DateTime.Now;
        daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
        rows = Mathf.CeilToInt((float)daysInMonth / columns);

        SetupGridLayout();
        LoadDayItems();
    }

    private void SetupGridLayout()
    {
        gridLayoutGroup.padding = new RectOffset(0, 0, 0, 0);
        gridLayoutGroup.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayoutGroup.startAxis = GridLayoutGroup.Axis.Horizontal;
        gridLayoutGroup.childAlignment = TextAnchor.UpperLeft;
        gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayoutGroup.constraintCount = columns;

        float viewportWidth = viewportTransform.rect.width;
        float dayItemWidth = viewportWidth / (columns + (columns - 1) * spacingRatio);
        float dayItemHeight = dayItemWidth / aspectRatio;

        gridLayoutGroup.cellSize = new Vector2(dayItemWidth, dayItemHeight);
        float spacing = dayItemWidth * spacingRatio;
        gridLayoutGroup.spacing = new Vector2(spacing, spacing);

        float contentWidth = dayItemWidth * columns + (columns - 1) * spacing;
        contentRect.sizeDelta = new Vector2(contentWidth, contentRect.sizeDelta.y);

        float totalSpacingHeight = (rows - 1) * spacing;
        float requiredContentHeight = (rows * dayItemHeight) + totalSpacingHeight;
        contentRect.sizeDelta = new Vector2(contentWidth, requiredContentHeight);

        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
    }

    private void LoadDayItems()
    {
        foreach (Transform child in contentTransform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < daysInMonth; i++)
        {
            int dayNumber = i + 1;
            GameObject newDayItem = Instantiate(dayItemPrefab, contentTransform);
            newDayItem.name = $"DayItem_{dayNumber}";

            TextMeshProUGUI dayText = newDayItem.transform.Find("DayText").GetComponent<TextMeshProUGUI>();
            if (dayText != null)
            {
                dayText.text = $"Day {dayNumber}";
            }

            AspectRatioFitter aspectRatioFitter = newDayItem.GetComponent<AspectRatioFitter>();
            if (aspectRatioFitter != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(newDayItem.GetComponent<RectTransform>());
            }

            GameObject itemImage = newDayItem.transform.Find("ItemImage")?.gameObject;
            GameObject claimText = itemImage?.transform.Find("ClaimText")?.gameObject;
            GameObject reclaimPanel = newDayItem.transform.Find("ReclaimPanel")?.gameObject;

            if (claimText != null)
            {
                Button claimButton = claimText.GetComponent<Button>();
                if (claimButton == null)
                {
                    claimButton = claimText.AddComponent<Button>();
                }
                claimButton.onClick.RemoveAllListeners();
                claimButton.onClick.AddListener(() => MarkDayAsClaimed(dayNumber));
            }

            if (reclaimPanel != null)
            {
                Button reclaimButton = reclaimPanel.GetComponent<Button>();
                if (reclaimButton == null)
                {
                    reclaimButton = reclaimPanel.AddComponent<Button>();
                }
                reclaimButton.onClick.RemoveAllListeners();
                reclaimButton.onClick.AddListener(() => ReclaimDay(dayNumber));
            }

            UpdateDayItemState(newDayItem, dayNumber);
        }

        UpdateRewardBlocks();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
    }

    private void UpdateDayItemState(GameObject dayItem, int dayNumber)
    {
        GameObject mask = dayItem.transform.Find("Mask")?.gameObject;
        GameObject itemImage = dayItem.transform.Find("ItemImage")?.gameObject;
        GameObject claimText = itemImage?.transform.Find("ClaimText")?.gameObject;
        GameObject reclaimPanel = dayItem.transform.Find("ReclaimPanel")?.gameObject;
        GameObject tick = dayItem.transform.Find("Tick")?.gameObject;
        GameObject dayText = dayItem.transform.Find("DayText")?.gameObject;
        GameObject qtyText = dayItem.transform.Find("QntyText")?.gameObject;

        DateTime now = DateTime.Now;
        int currentDay = now.Day;

        string claimKey = $"Claimed_Day_{now.Year}_{now.Month}_{dayNumber}";
        bool isClaimed = PlayerPrefs.GetInt(claimKey, 0) == 1;

        if (currentDay == 1 && dayNumber == 1)
        {
            for (int i = 1; i <= daysInMonth; i++)
            {
                PlayerPrefs.SetInt($"Claimed_Day_{now.Year}_{now.Month}_{i}", 0);
            }
            PlayerPrefs.Save();
        }

        if (mask != null) mask.SetActive(false);
        if (claimText != null) claimText.SetActive(false);
        if (reclaimPanel != null) reclaimPanel.SetActive(false);
        if (tick != null) tick.SetActive(false);
        if (dayText != null) dayText.SetActive(true);
        if (qtyText != null) qtyText.SetActive(true);
        if (itemImage != null) itemImage.SetActive(true);

        if (dayNumber < currentDay)
        {
            if (isClaimed)
            {
                if (tick != null) tick.SetActive(true);
                if (reclaimPanel != null) reclaimPanel.SetActive(false);
            }
            else
            {
                if (mask != null) mask.SetActive(true);
                if (reclaimPanel != null) reclaimPanel.SetActive(true);
            }
        }
        else if (dayNumber == currentDay)
        {
            if (mask != null) mask.SetActive(false);
            if (isClaimed)
            {
                if (tick != null) tick.SetActive(true);
                if (claimText != null) claimText.SetActive(false);
            }
            else
            {
                if (claimText != null) claimText.SetActive(true);
            }
        }
    }

    private void UpdateRewardBlocks()
    {
        int totalClaimedDays = PlayerPrefs.GetInt(totalClaimedDaysKey, 0);
        Debug.Log($"Total Claimed Days: {totalClaimedDays}");

        // Update 7-day block
        GameObject openBox7D = block7D.transform.Find("OpenBox")?.gameObject;
        GameObject closeBox7D = block7D.transform.Find("CloseBox")?.gameObject;
        if (openBox7D != null && closeBox7D != null)
        {
            bool is7DayReached = totalClaimedDays >= 7;
            openBox7D.SetActive(is7DayReached);
            closeBox7D.SetActive(!is7DayReached);
            Debug.Log($"7D Box - Open: {is7DayReached}, OpenBox: {openBox7D.activeSelf}, CloseBox: {closeBox7D.activeSelf}");
        }
        else
        {
            Debug.LogError("Block_7D hierarchy missing OpenBox or CloseBox!");
        }

        // Update 14-day block
        GameObject openBox14D = block14D.transform.Find("OpenBox")?.gameObject;
        GameObject closeBox14D = block14D.transform.Find("CloseBox")?.gameObject;
        if (openBox14D != null && closeBox14D != null)
        {
            bool is14DayReached = totalClaimedDays >= 14;
            openBox14D.SetActive(is14DayReached);
            closeBox14D.SetActive(!is14DayReached);
        }

        // Update 21-day block
        GameObject openBox21D = block21D.transform.Find("OpenBox")?.gameObject;
        GameObject closeBox21D = block21D.transform.Find("CloseBox")?.gameObject;
        if (openBox21D != null && closeBox21D != null)
        {
            bool is21DayReached = totalClaimedDays >= 21;
            openBox21D.SetActive(is21DayReached);
            closeBox21D.SetActive(!is21DayReached);
        }

        // Update 30-day block
        GameObject openBox30D = block30D.transform.Find("OpenBox")?.gameObject;
        GameObject closeBox30D = block30D.transform.Find("CloseBox")?.gameObject;
        if (openBox30D != null && closeBox30D != null)
        {
            bool is30DayReached = totalClaimedDays >= 30;
            openBox30D.SetActive(is30DayReached);
            closeBox30D.SetActive(!is30DayReached);

            if (is30DayReached && openBox30D.activeSelf)
            {
                PlayerPrefs.SetInt(totalClaimedDaysKey, 0);
                totalClaimedDays = 0;
                PlayerPrefs.Save();
            }
        }

        // Update GreenLine proportionally starting from GreyLine, with x = 0 relative to parent
        if (greenLine != null && greyLine != null && greenLine.parent != null)
        {
            RectTransform parentRect = greenLine.parent.GetComponent<RectTransform>();
            if (parentRect != null)
            {
                float parentWidth = parentRect.rect.width;
                float greyLineWidth = greyLine.rect.width; // Preserve GreyLine width
                float greyLineStart = parentWidth * greyLine.anchorMin.x; // GreyLine's left edge (0.12 * parentWidth)
                float progress = Mathf.Clamp01((float)totalClaimedDays / 30f);
                float remainingWidth = parentWidth - greyLineStart; // Space from GreyLine to parent right edge
                float greenLineWidth = remainingWidth * progress; // Stretch from GreyLine onward
                greenLine.sizeDelta = new Vector2(greyLineStart + greenLineWidth, greenLine.sizeDelta.y); // Start at GreyLine's left edge
                greenLine.anchoredPosition = new Vector2(0, greenLine.anchoredPosition.y); // Always 0 relative to parent
                Debug.Log($"GreenLine - Progress: {progress}, ParentWidth: {parentWidth}, GreyLineStart: {greyLineStart}, RemainingWidth: {remainingWidth}, New Width: {greenLineWidth}, TotalClaimedDays: {totalClaimedDays}");
            }
            else
            {
                Debug.LogError("GreenLine parent does not have a RectTransform!");
            }
        }
        else
        {
            Debug.LogError("GreenLine or GreyLine or their parent is null!");
        }

        PlayerPrefs.SetInt(totalClaimedDaysKey, totalClaimedDays);
        PlayerPrefs.Save();
    }

    public void MarkDayAsClaimed(int dayNumber)
    {
        DateTime now = DateTime.Now;
        string claimKey = $"Claimed_Day_{now.Year}_{now.Month}_{dayNumber}";
        bool isAlreadyClaimed = PlayerPrefs.GetInt(claimKey, 0) == 1;

        if (!isAlreadyClaimed)
        {
            PlayerPrefs.SetInt(claimKey, 1);
            int totalClaimedDays = PlayerPrefs.GetInt(totalClaimedDaysKey, 0) + 1;
            PlayerPrefs.SetInt(totalClaimedDaysKey, totalClaimedDays);
            PlayerPrefs.Save();
        }

        LoadDayItems();
    }

    // public void ReclaimDay(int dayNumber)
    // {
    //     if (resourceManager != null)
    //     {
    //         int currentGems = resourceManager.GetGemCount();
    //         if (currentGems < reclaimGemCost)
    //         {
    //             Debug.LogWarning("Not enough gems to reclaim this day!");
    //             return;
    //         }

    //         resourceManager.UpdateGemCount(-reclaimGemCost); // Deduct 20 gems
    //         MarkDayAsClaimed(dayNumber); // This will handle the claim logic
    //     }
    // }

    public void ReclaimDay(int dayNumber)
    {
        int currentGems = PlayerPrefs.GetInt("PlayerGems", 0); // Direct access to PlayerPrefs

        if (currentGems < reclaimGemCost)
        {
            Debug.LogWarning("Not enough gems to reclaim this day!");
            return;
        }

        PlayerPrefs.SetInt("PlayerGems", currentGems - reclaimGemCost); // Direct deduction
        PlayerPrefs.Save(); // Save changes immediately

        MarkDayAsClaimed(dayNumber); // Handle the claim logic
    }

    // New method to check for unclaimed days
    public bool HasUnclaimedDays()
    {
        DateTime now = DateTime.Now;
        int currentDay = now.Day;
        int daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);

        for (int dayNumber = 1; dayNumber <= currentDay; dayNumber++)
        {
            string claimKey = $"Claimed_Day_{now.Year}_{now.Month}_{dayNumber}";
            bool isClaimed = PlayerPrefs.GetInt(claimKey, 0) == 1;
            if (!isClaimed)
            {
                return true; // Found an unclaimed day
            }
        }
        return false; // All days up to currentDay are claimed
    }
}