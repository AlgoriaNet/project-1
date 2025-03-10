using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

public class FirstChargeController : MonoBehaviour
{
    public GameObject firstChargePage;  // Assign firstChargePage in Inspector
    public GameObject commonPage;
    public GameObject mainPanel;
    public Button highButton;
    public Button medButton;
    public Button lowButton;
    public Button purchaseButton;       // Assign PurchaseButton in Inspector
    public Button claimButton_1;        // Assign ClaimButton_1 in Inspector
    public Button claimButton_2; 
    public Button claimButton_3; 
    public TextMeshProUGUI priceText;   // Assign PriceText in Inspector
    public TextMeshProUGUI claimText_2; // Assign ClaimText_2 in Inspector
    public TextMeshProUGUI claimText_3; // Assign ClaimText_3 in Inspector

    // References to Tick GameObjects
    public GameObject tickD1Block1;  // Assign Tick under Block_1 in D1
    public GameObject tickD1Block2;  // Assign Tick under Block_2 in D1
    public GameObject tickD2Block1;  // Assign Tick under Block_1 in D2
    public GameObject tickD2Block2;  // Assign Tick under Block_2 in D2
    public GameObject tickD3Block1;  // Assign Tick under Block_1 in D3
    public GameObject tickD3Block2;  // Assign Tick under Block_2 in D3

    // References to Image GameObjects (declared like Tick)
    public GameObject imageD1Block1;  // Assign Image under Block_1 in D1
    public GameObject imageD1Block2;  // Assign Image under Block_2 in D1
    public GameObject imageD2Block1;  // Assign Image under Block_1 in D2
    public GameObject imageD2Block2;  // Assign Image under Block_2 in D2
    public GameObject imageD3Block1;  // Assign Image under Block_1 in D3
    public GameObject imageD3Block2;  // Assign Image under Block_2 in D3

    public GameObject FirstChargeButton;  // Assign the FirstChargeButton GameObject in Inspector

    private const string purchaseTimeKeyPrefix = "PurchaseTime_";
    private const string claimStatusKeyPrefix = "ClaimStatus_"; // Prefix for claim status keys

    // Track purchase status
    private bool hasPurchasedHigh = false;
    private bool hasPurchasedMed = false;
    private bool hasPurchasedLow = false;

    private int selectedTier = 1; // Track selected tier (1, 2, or 3)

    // Unified dictionary for price and quantities
    private Dictionary<int, string[]> chargeData = new Dictionary<int, string[]>
    {
        { 1, new string[] { "$14.99", "750", "30", "100" } }, // High tier: Price, Gem, Key, Skillbook
        { 2, new string[] { "$4.99", "330", "20", "30" } },    // Medium tier: Price, Gem, Key, Skillbook
        { 3, new string[] { "$0.99", "60", "10", "10" } }      // Low tier: Price, Gem, Key, Skillbook
    };

    private void Start()
    {
        LoadPurchaseStatus();
        LoadClaimStatus(); // Load claim status at start
        UpdatePurchaseButtonStatus();
        UpdateTickStatuses(); // Update Tick statuses on start
        CheckAndDisableFirstChargeButton(); // Check if all claims are done and disable button if so
        UpdateDotStatuses(); // Update Dot statuses
        Debug.Log("Starting with initial Tick statuses and FirstChargeButton check updated.");
        InvokeRepeating(nameof(UpdateClaimTexts), 0f, 60f);  // Update every 60 seconds
    }

    public void OpenFirstChargePage()
    {
        firstChargePage.SetActive(true);
        commonPage.SetActive(false);
        mainPanel.SetActive(false);
        TogglePrice(1); // Default to high tier on open
        UpdateClaimTexts(); // Refresh claim texts
        UpdateTickStatuses(); // Update Tick statuses for the default tier ($14.99)
        UpdateDotStatuses(); // Update Dot statuses
        CheckAndDisableFirstChargeButton(); // Check if all claims are done and disable button if so
        Debug.Log("Opened First Charge Page, Ticks updated for $14.99 tier, and checked FirstChargeButton disable status.");
    }

    public void CloseFirstChargePage()
    {
        firstChargePage.SetActive(false);
        commonPage.SetActive(true);
        mainPanel.SetActive(true);
    }

    public void TogglePrice(int selectedIndex)
    {
        selectedTier = selectedIndex; // Track selected tier

        // Handle Button visuals
        highButton.transform.Find("OrangeButton").gameObject.SetActive(selectedIndex == 1);
        highButton.transform.Find("GreyButton").gameObject.SetActive(selectedIndex != 1);
        medButton.transform.Find("OrangeButton").gameObject.SetActive(selectedIndex == 2);
        medButton.transform.Find("GreyButton").gameObject.SetActive(selectedIndex != 2);
        lowButton.transform.Find("OrangeButton").gameObject.SetActive(selectedIndex == 3);
        lowButton.transform.Find("GreyButton").gameObject.SetActive(selectedIndex != 3);

        // Update price text
        if (chargeData.ContainsKey(selectedIndex))
        {
            priceText.text = chargeData[selectedIndex][0];
        }

        // Load and update ClaimButton status for the selected tier
        UpdateClaimButtonStatus();

        // Prevent carry-over issue by disabling ClaimButtons if not purchased
        if (!hasPurchasedHigh && selectedIndex == 1)
        {
            claimButton_1.gameObject.SetActive(false);
            claimButton_2.gameObject.SetActive(false);
            claimButton_3.gameObject.SetActive(false);
        }
        if (!hasPurchasedMed && selectedIndex == 2)
        {
            claimButton_1.gameObject.SetActive(false);
            claimButton_2.gameObject.SetActive(false);
            claimButton_3.gameObject.SetActive(false);
        }
        if (!hasPurchasedLow && selectedIndex == 3)
        {
            claimButton_1.gameObject.SetActive(false);
            claimButton_2.gameObject.SetActive(false);
            claimButton_3.gameObject.SetActive(false);
        }

        UpdatePurchaseButtonStatus();
        UpdateClaimTexts(); // Refresh claim texts when toggling
        UpdateTickStatuses(); // Update Tick statuses for the selected tier
        UpdateDotStatuses(); // Update Dot statuses for the selected tier
        CheckAndDisableFirstChargeButton(); // Check if all claims are done and disable button if so
        Debug.Log($"Toggled to tier {selectedIndex} (${chargeData[selectedIndex][0]}), Ticks updated, and checked FirstChargeButton disable status.");
    }

    public void Purchase()
    {
        if (selectedTier == 1 && !hasPurchasedHigh)
        {
            hasPurchasedHigh = true;
            SavePurchaseTime(1);
            Debug.Log("✅ Complete paying $14.99");
        }
        else if (selectedTier == 2 && !hasPurchasedMed)
        {
            hasPurchasedMed = true;
            SavePurchaseTime(2);
            Debug.Log("✅ Complete paying $4.99");
        }
        else if (selectedTier == 3 && !hasPurchasedLow)
        {
            hasPurchasedLow = true;
            SavePurchaseTime(3);
            Debug.Log("✅ Complete paying $0.99");
        }

        UpdatePurchaseButtonStatus();
        ShowClaimSection();
        UpdateTickStatuses(); // Update Tick statuses after purchase
        UpdateDotStatuses(); // Update Dot statuses after purchase
        CheckAndDisableFirstChargeButton(); // Check if all claims are done and disable button if so
        Debug.Log($"Purchased tier {selectedTier}, Ticks updated, and checked FirstChargeButton disable status.");
    }

    private void SavePurchaseTime(int tier)
    {
        PlayerPrefs.SetString(purchaseTimeKeyPrefix + tier, DateTime.Now.ToString());
        PlayerPrefs.Save();
    }

    private void ShowClaimSection()
    {
        purchaseButton.gameObject.SetActive(false);
        claimButton_1.gameObject.SetActive(true);
        UpdateClaimTexts();
    }

    private void UpdateClaimTexts()
    {
        string purchaseTimeKey = purchaseTimeKeyPrefix + selectedTier;
        if (PlayerPrefs.HasKey(purchaseTimeKey))
        {
            DateTime purchaseTime = DateTime.Parse(PlayerPrefs.GetString(purchaseTimeKey));
            DateTime now = DateTime.Now;

            // Calculate when Day 2 and Day 3 become available (start of next calendar day and day after)
            DateTime day2cute = purchaseTime.Date.AddDays(1); // Start of next day
            DateTime day3cute = purchaseTime.Date.AddDays(2); // Start of day after next

            bool isDay2Claimable = now >= day2cute;
            bool isDay3Claimable = now >= day3cute;

            claimText_2.gameObject.SetActive(!isDay2Claimable);
            claimText_3.gameObject.SetActive(!isDay3Claimable);

            // Update ClaimText_2 and ClaimButton_2 (Day 2)
            if (!isDay2Claimable)
            {
                TimeSpan timeUntilDay2 = day2cute - now;
                claimText_2.text = $"Claim in {Mathf.FloorToInt((float)timeUntilDay2.TotalHours)}H {Mathf.FloorToInt((float)timeUntilDay2.Minutes)}M";
                claimText_2.gameObject.SetActive(true);
                claimButton_2.gameObject.SetActive(false);
            }
            else
            {
                claimText_2.gameObject.SetActive(false);
                claimButton_2.gameObject.SetActive(true && !IsClaimed(2)); // Only show if not claimed
            }

            // Update ClaimText_3 and ClaimButton_3 (Day 3)
            if (!isDay3Claimable)
            {
                TimeSpan timeUntilDay3 = day3cute - now;
                claimText_3.text = $"Claim in {Mathf.FloorToInt((float)timeUntilDay3.TotalHours)}H {Mathf.FloorToInt((float)timeUntilDay3.Minutes)}M";
                claimText_3.gameObject.SetActive(true);
                claimButton_3.gameObject.SetActive(false);
            }
            else
            {
                claimText_3.gameObject.SetActive(false);
                claimButton_3.gameObject.SetActive(true && !IsClaimed(3)); // Only show if not claimed
            }
        }
        else
        {
            claimText_2.gameObject.SetActive(false);
            claimText_3.gameObject.SetActive(false);
        }
    }

    private void UpdatePurchaseButtonStatus()
    {
        bool isPurchased = (selectedTier == 1 && hasPurchasedHigh) ||
                        (selectedTier == 2 && hasPurchasedMed) ||
                        (selectedTier == 3 && hasPurchasedLow);

        purchaseButton.gameObject.SetActive(!isPurchased);
        
        // Show ClaimButton_1 immediately after purchase for the correct tier
        string claimStatusKey1 = $"{claimStatusKeyPrefix}1_Tier_{selectedTier}";
        bool isClaimed1 = PlayerPrefs.GetInt(claimStatusKey1, 0) == 1;
        claimButton_1.gameObject.SetActive(isPurchased && !isClaimed1);
    }

    private void LoadPurchaseStatus()
    {
        hasPurchasedHigh = PlayerPrefs.HasKey(purchaseTimeKeyPrefix + 1);
        hasPurchasedMed = PlayerPrefs.HasKey(purchaseTimeKeyPrefix + 2);
        hasPurchasedLow = PlayerPrefs.HasKey(purchaseTimeKeyPrefix + 3);
    }

    private void LoadClaimStatus()
    {
        // Load claim status for all buttons and tiers
        for (int tier = 1; tier <= 3; tier++)
        {
            for (int button = 1; button <= 3; button++)
            {
                string claimStatusKey = $"{claimStatusKeyPrefix}{button}_Tier_{tier}";
                PlayerPrefs.GetInt(claimStatusKey, 0); // Ensure the key exists, default to 0 (not claimed)
            }
        }
    }

    private void UpdateClaimButtonStatus()
    {
        // Load and update the status of all claim buttons for the selected tier
        for (int button = 1; button <= 3; button++)
        {
            string claimStatusKey = $"{claimStatusKeyPrefix}{button}_Tier_{selectedTier}";
            bool isClaimed = PlayerPrefs.GetInt(claimStatusKey, 0) == 1;
            bool isPurchased = hasPurchasedForTier(selectedTier);
            if (button == 1)
                claimButton_1.gameObject.SetActive(isPurchased && !isClaimed);
            else if (button == 2)
                claimButton_2.gameObject.SetActive(isPurchased && !isClaimed);
            else if (button == 3)
                claimButton_3.gameObject.SetActive(isPurchased && !isClaimed);
        }
    }

    private bool hasPurchasedForTier(int tier)
    {
        return (tier == 1 && hasPurchasedHigh) || (tier == 2 && hasPurchasedMed) || (tier == 3 && hasPurchasedLow);
    }

    private bool IsClaimed(int buttonIndex)
    {
        string claimStatusKey = $"{claimStatusKeyPrefix}{buttonIndex}_Tier_{selectedTier}";
        return PlayerPrefs.GetInt(claimStatusKey, 0) == 1;
    }

    private bool IsClaimable(int tier, int dayIndex)
    {
        string purchaseTimeKey = $"{purchaseTimeKeyPrefix}{tier}";
        if (PlayerPrefs.HasKey(purchaseTimeKey))
        {
            DateTime purchaseTime = DateTime.Parse(PlayerPrefs.GetString(purchaseTimeKey));
            DateTime now = DateTime.Now;
            DateTime dayAvailable = purchaseTime.Date.AddDays(dayIndex - 1); // Adjust for 1-based day index
            bool isClaimable = now >= dayAvailable;
            string claimStatusKey = $"{claimStatusKeyPrefix}{dayIndex}_Tier_{tier}";
            bool isNotClaimed = PlayerPrefs.GetInt(claimStatusKey, 0) != 1; // 1 means claimed
            return isClaimable && isNotClaimed;
        }
        return false;
    }

    private bool AreAllClaimsMade(int tier)
    {
        for (int day = 1; day <= 3; day++)
        {
            string claimStatusKey = $"{claimStatusKeyPrefix}{day}_Tier_{tier}";
            if (PlayerPrefs.GetInt(claimStatusKey, 0) != 1) // 1 means claimed
            {
                return false;
            }
        }
        return true;
    }

    private void UpdateDotStatuses()
    {
        // Update Dot for highButton (Tier 1)
        Transform highDot = highButton.transform.Find("Dot");
        if (highDot != null)
        {
            bool hasPurchased = hasPurchasedHigh;
            bool hasClaimable = hasPurchased && (IsClaimable(1, 1) || IsClaimable(1, 2) || IsClaimable(1, 3));
            highDot.gameObject.SetActive(hasPurchased && hasClaimable && !AreAllClaimsMade(1));
        }

        // Update Dot for medButton (Tier 2)
        Transform medDot = medButton.transform.Find("Dot");
        if (medDot != null)
        {
            bool hasPurchased = hasPurchasedMed;
            bool hasClaimable = hasPurchased && (IsClaimable(2, 1) || IsClaimable(2, 2) || IsClaimable(2, 3));
            medDot.gameObject.SetActive(hasPurchased && hasClaimable && !AreAllClaimsMade(2));
        }

        // Update Dot for lowButton (Tier 3)
        Transform lowDot = lowButton.transform.Find("Dot");
        if (lowDot != null)
        {
            bool hasPurchased = hasPurchasedLow;
            bool hasClaimable = hasPurchased && (IsClaimable(3, 1) || IsClaimable(3, 2) || IsClaimable(3, 3));
            lowDot.gameObject.SetActive(hasPurchased && hasClaimable && !AreAllClaimsMade(3));
        }
    }

    private void UpdateTickStatuses()
    {
        bool isPurchased = hasPurchasedForTier(selectedTier);
        if (!isPurchased)
        {
            // If not purchased, hide all Ticks for this tier and restore Images
            if (tickD1Block1 != null) tickD1Block1.SetActive(false);
            if (tickD1Block2 != null) tickD1Block2.SetActive(false);
            if (tickD2Block1 != null) tickD2Block1.SetActive(false);
            if (tickD2Block2 != null) tickD2Block2.SetActive(false);
            if (tickD3Block1 != null) tickD3Block1.SetActive(false);
            if (tickD3Block2 != null) tickD3Block2.SetActive(false);
            if (imageD1Block1 != null) GetImageComponent(imageD1Block1).color = new Color(1, 1, 1, 1f); // Restore
            if (imageD1Block2 != null) GetImageComponent(imageD1Block2).color = new Color(1, 1, 1, 1f); // Restore
            if (imageD2Block1 != null) GetImageComponent(imageD2Block1).color = new Color(1, 1, 1, 1f); // Restore
            if (imageD2Block2 != null) GetImageComponent(imageD2Block2).color = new Color(1, 1, 1, 1f); // Restore
            if (imageD3Block1 != null) GetImageComponent(imageD3Block1).color = new Color(1, 1, 1, 1f); // Restore
            if (imageD3Block2 != null) GetImageComponent(imageD3Block2).color = new Color(1, 1, 1, 1f); // Restore
            Debug.Log($"Ticks for tier {selectedTier} (${chargeData[selectedTier][0]}) hidden (not purchased).");
            return;
        }

        // Check purchase time and claim status for each day (D1, D2, D3)
        string purchaseTimeKey = purchaseTimeKeyPrefix + selectedTier;
        if (PlayerPrefs.HasKey(purchaseTimeKey))
        {
            DateTime purchaseTime = DateTime.Parse(PlayerPrefs.GetString(purchaseTimeKey));
            DateTime now = DateTime.Now;

            // Calculate when Day 2 and Day 3 become available (start of next calendar day and day after)
            DateTime day2Available = purchaseTime.Date.AddDays(1); // Start of next day
            DateTime day3Available = purchaseTime.Date.AddDays(2); // Start of day after next

            bool isDay2Claimable = now >= day2Available;
            bool isDay3Claimable = now >= day3Available;

            // Day 1 (D1, ClaimButton_1) - Always claimable after purchase
            bool isD1Claimed = IsClaimed(1);
            if (tickD1Block1 != null) tickD1Block1.SetActive(isD1Claimed);
            if (tickD1Block2 != null) tickD1Block2.SetActive(isD1Claimed);
            if (imageD1Block1 != null) GetImageComponent(imageD1Block1).color = isD1Claimed ? new Color(1, 1, 1, 0.5f) : new Color(1, 1, 1, 1f);
            if (imageD1Block2 != null) GetImageComponent(imageD1Block2).color = isD1Claimed ? new Color(1, 1, 1, 0.5f) : new Color(1, 1, 1, 1f);
            Debug.Log($"Ticks for Day 1 (tier {selectedTier}) set to {isD1Claimed} (claimed: {isD1Claimed}).");

            // Day 2 (D2, ClaimButton_2) - Only show Ticks if claimed and calendar day has started
            bool isD2Claimed = IsClaimed(2);
            bool isD2Active = isDay2Claimable && isD2Claimed;
            if (tickD2Block1 != null) tickD2Block1.SetActive(isD2Active);
            if (tickD2Block2 != null) tickD2Block2.SetActive(isD2Active);
            if (imageD2Block1 != null) GetImageComponent(imageD2Block1).color = isD2Active ? new Color(1, 1, 1, 0.5f) : new Color(1, 1, 1, 1f);
            if (imageD2Block2 != null) GetImageComponent(imageD2Block2).color = isD2Active ? new Color(1, 1, 1, 0.5f) : new Color(1, 1, 1, 1f);
            Debug.Log($"Ticks for Day 2 (tier {selectedTier}) set to {isD2Active} (claimable: {isDay2Claimable}, claimed: {isD2Claimed}).");

            // Day 3 (D3, ClaimButton_3) - Only show Ticks if claimed and calendar day has started
            bool isD3Claimed = IsClaimed(3);
            bool isD3Active = isDay3Claimable && isD3Claimed;
            if (tickD3Block1 != null) tickD3Block1.SetActive(isD3Active);
            if (tickD3Block2 != null) tickD3Block2.SetActive(isD3Active);
            if (imageD3Block1 != null) GetImageComponent(imageD3Block1).color = isD3Active ? new Color(1, 1, 1, 0.5f) : new Color(1, 1, 1, 1f);
            if (imageD3Block2 != null) GetImageComponent(imageD3Block2).color = isD3Active ? new Color(1, 1, 1, 0.5f) : new Color(1, 1, 1, 1f);
            Debug.Log($"Ticks for Day 3 (tier {selectedTier}) set to {isD3Active} (claimable: {isDay3Claimable}, claimed: {isD3Claimed}).");
        }
        else
        {
            // If no purchase time, hide all Ticks and restore Images
            if (tickD1Block1 != null) tickD1Block1.SetActive(false);
            if (tickD1Block2 != null) tickD1Block2.SetActive(false);
            if (tickD2Block1 != null) tickD2Block1.SetActive(false);
            if (tickD2Block2 != null) tickD2Block2.SetActive(false);
            if (tickD3Block1 != null) tickD3Block1.SetActive(false);
            if (tickD3Block2 != null) tickD3Block2.SetActive(false);
            if (imageD1Block1 != null) GetImageComponent(imageD1Block1).color = new Color(1, 1, 1, 1f); // Restore
            if (imageD1Block2 != null) GetImageComponent(imageD1Block2).color = new Color(1, 1, 1, 1f); // Restore
            if (imageD2Block1 != null) GetImageComponent(imageD2Block1).color = new Color(1, 1, 1, 1f); // Restore
            if (imageD2Block2 != null) GetImageComponent(imageD2Block2).color = new Color(1, 1, 1, 1f); // Restore
            if (imageD3Block1 != null) GetImageComponent(imageD3Block1).color = new Color(1, 1, 1, 1f); // Restore
            if (imageD3Block2 != null) GetImageComponent(imageD3Block2).color = new Color(1, 1, 1, 1f); // Restore
            Debug.Log($"Ticks for tier {selectedTier} hidden (no purchase time).");
        }
    }

    private Image GetImageComponent(GameObject imageObject)
    {
        return imageObject.GetComponent<Image>();
    }

    public void ClaimReward(int claimButtonIndex)
    {
        // Save claim status for the specific tier and button
        string claimStatusKey = $"{claimStatusKeyPrefix}{claimButtonIndex}_Tier_{selectedTier}";
        PlayerPrefs.SetInt(claimStatusKey, 1);  // 1 means claimed
        PlayerPrefs.Save();

        // Disable the clicked ClaimButton
        if (claimButtonIndex == 1)
            claimButton_1.gameObject.SetActive(false);
        else if (claimButtonIndex == 2)
            claimButton_2.gameObject.SetActive(false);
        else if (claimButtonIndex == 3)
            claimButton_3.gameObject.SetActive(false);

        // Activate the appropriate Ticks based on the claim button and tier
        switch (claimButtonIndex)
        {
            case 1: // Day 1 (D1)
                if (tickD1Block1 != null) tickD1Block1.SetActive(true);
                if (tickD1Block2 != null) tickD1Block2.SetActive(true);
                if (imageD1Block1 != null) GetImageComponent(imageD1Block1).color = new Color(1, 1, 1, 0.5f); // Grey out
                if (imageD1Block2 != null) GetImageComponent(imageD1Block2).color = new Color(1, 1, 1, 0.5f); // Grey out
                Debug.Log($"Activated Ticks for Day 1 (tier {selectedTier}, ${chargeData[selectedTier][0]}).");
                break;
            case 2: // Day 2 (D2)
                if (tickD2Block1 != null) tickD2Block1.SetActive(true);
                if (tickD2Block2 != null) tickD2Block2.SetActive(true);
                if (imageD2Block1 != null) GetImageComponent(imageD2Block1).color = new Color(1, 1, 1, 0.5f); // Grey out
                if (imageD2Block2 != null) GetImageComponent(imageD2Block2).color = new Color(1, 1, 1, 0.5f); // Grey out
                Debug.Log($"Activated Ticks for Day 2 (tier {selectedTier}, ${chargeData[selectedTier][0]}).");
                break;
            case 3: // Day 3 (D3)
                if (tickD3Block1 != null) tickD3Block1.SetActive(true);
                if (tickD3Block2 != null) tickD3Block2.SetActive(true);
                if (imageD3Block1 != null) GetImageComponent(imageD3Block1).color = new Color(1, 1, 1, 0.5f); // Grey out
                if (imageD3Block2 != null) GetImageComponent(imageD3Block2).color = new Color(1, 1, 1, 0.5f); // Grey out
                Debug.Log($"Activated Ticks for Day 3 (tier {selectedTier}, ${chargeData[selectedTier][0]}).");
                break;
        }

        UpdateTickStatuses(); // Ensure Tick statuses are updated after claiming
        UpdateDotStatuses(); // Update Dot statuses after claiming
        CheckAndDisableFirstChargeButton(); // Check if all claims are done and disable button if so
        Debug.Log($"Claimed button {claimButtonIndex} for tier {selectedTier}, Ticks updated, and checked FirstChargeButton disable status.");
    }

    private void GreyOutImagesForTier()
    {
        // This method is no longer needed since Images are handled directly
        Debug.Log("GreyOutImagesForTier is deprecated; Images are now handled directly.");
    }

    private void GreyOutBlockImages(Transform block)
    {
        // This method is no longer needed since Images are handled directly
        Debug.Log("GreyOutBlockImages is deprecated; Images are now handled directly.");
    }

    private void CheckAndDisableFirstChargeButton()
    {
        // Check if all 9 claims (3 tiers × 3 days) have been made
        bool allClaimsMade = true;
        for (int tier = 1; tier <= 3; tier++)
        {
            for (int day = 1; day <= 3; day++)
            {
                string claimStatusKey = $"{claimStatusKeyPrefix}{day}_Tier_{tier}";
                if (PlayerPrefs.GetInt(claimStatusKey, 0) != 1) // 1 means claimed
                {
                    allClaimsMade = false;
                    break;
                }
            }
            if (!allClaimsMade) break;
        }

        if (allClaimsMade && FirstChargeButton != null)
        {
            FirstChargeButton.SetActive(false); // Disable and hide the FirstChargeButton permanently
            Debug.Log("All 9 claims completed—FirstChargeButton disabled forever.");
        }
    }
}