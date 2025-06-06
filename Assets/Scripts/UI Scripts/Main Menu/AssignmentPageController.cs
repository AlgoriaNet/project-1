using UnityEngine;
using System;

public class AssignmentPageController : MonoBehaviour
{
    public static AssignmentPageController Instance { get; private set; } // Singleton for real-time updates

    public GameObject AssignmentPage;
    public GameObject commonPage;
    public GameObject mainPanel;
    public GameObject DailySignPage;
    public GameObject DailyQuestPage;
    public GameObject SevenDQuestPage;
    public GameObject AssignmentButton; // Reference to the main Assignment button in the UI

    // References to the button objects
    public GameObject DailySignButton;
    public GameObject SevenDQuestButton;
    public GameObject DailyQuestButton;

    // References to other controllers
    public DayItemLoader dayItemLoader; // Assign in Inspector
    public SevenDQuestController sevenDQuestController; // Assign in Inspector
    public DailyQuestController dailyQuestController; // Add reference to DailyQuestController

    private int selectedPage; // Track the current page

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Initialize all controllers first
        if (dayItemLoader != null) dayItemLoader.Start(); // Ensure DayItemLoader initializes
        if (sevenDQuestController != null) sevenDQuestController.Start(); // Ensure SevenDQuest initializes
        if (dailyQuestController != null) dailyQuestController.Start(); // Ensure DailyQuest initializes

        // Explicitly disable all dots initially
        SetAllDotsInactive();

        // Default to page 1 and update states
        ToggleAssignmentPage(1);
    }

    public void OpenAssignmentPage()
    {
        AssignmentPage.SetActive(true);
        commonPage.SetActive(false);
        mainPanel.SetActive(false);
        
        // Ensure all buttons are visible when opening the page
        if (DailySignButton != null) DailySignButton.SetActive(true);
        if (SevenDQuestButton != null) SevenDQuestButton.SetActive(true);
        if (DailyQuestButton != null) DailyQuestButton.SetActive(true);
        
        // Update dot statuses when opening the page
        UpdateDotStatuses();
    }

    public void CloseAssignmentPage()
    {
        AssignmentPage.SetActive(false);
        commonPage.SetActive(true);
        mainPanel.SetActive(true);
    }

    public void ToggleAssignmentPage(int selectedIndex)
    {
        // Track selected page
        selectedPage = selectedIndex;

        // Handle Button visuals (toggle orange/grey buttons on the button objects)
        if (DailySignButton != null)
        {
            DailySignButton.transform.Find("OrangeButton")?.gameObject.SetActive(selectedIndex == 1);
            DailySignButton.transform.Find("GreyButton")?.gameObject.SetActive(selectedIndex != 1);
        }
        
        if (SevenDQuestButton != null)
        {
            SevenDQuestButton.transform.Find("OrangeButton")?.gameObject.SetActive(selectedIndex == 2);
            SevenDQuestButton.transform.Find("GreyButton")?.gameObject.SetActive(selectedIndex != 2);
        }
        
        if (DailyQuestButton != null)
        {
            DailyQuestButton.transform.Find("OrangeButton")?.gameObject.SetActive(selectedIndex == 3);
            DailyQuestButton.transform.Find("GreyButton")?.gameObject.SetActive(selectedIndex != 3);
        }

        // Toggle page visibility
        if (DailySignPage != null) DailySignPage.SetActive(selectedIndex == 1);
        if (SevenDQuestPage != null) SevenDQuestPage.SetActive(selectedIndex == 2);
        if (DailyQuestPage != null) DailyQuestPage.SetActive(selectedIndex == 3);

        // Ensure all buttons remain visible regardless of which page is selected
        if (DailySignButton != null) DailySignButton.SetActive(true);
        if (SevenDQuestButton != null) SevenDQuestButton.SetActive(true);
        if (DailyQuestButton != null) DailyQuestButton.SetActive(true);

        // Update page-specific states
        if (selectedIndex == 2 && sevenDQuestController != null)
        {
            sevenDQuestController.UpdateDayStates();
        }
        
        // Trigger OnPageActivated for DailyQuestController
        if (selectedIndex == 3 && dailyQuestController != null)
        {
            dailyQuestController.OnPageActivated();
        }

        // Update dot statuses
        UpdateDotStatuses();
    }

    // New method to disable all dots initially
    private void SetAllDotsInactive()
    {
        if (DailySignButton != null)
        {
            GameObject dailySignDot = DailySignButton.transform.Find("DotIndicator")?.gameObject;
            if (dailySignDot != null) dailySignDot.SetActive(false);
        }
        
        if (SevenDQuestButton != null)
        {
            GameObject sevenDQuestDot = SevenDQuestButton.transform.Find("DotIndicator")?.gameObject;
            if (sevenDQuestDot != null) sevenDQuestDot.SetActive(false);
        }
        
        if (DailyQuestButton != null)
        {
            GameObject dailyQuestDot = DailyQuestButton.transform.Find("DotIndicator")?.gameObject;
            if (dailyQuestDot != null) dailyQuestDot.SetActive(false);
        }
        
        if (AssignmentButton != null)
        {
            GameObject mainDot = AssignmentButton.transform.Find("DotIndicator")?.gameObject;
            if (mainDot != null) mainDot.SetActive(false);
        }
    }

    // Method to update dot statuses
    public void UpdateDotStatuses()
    {
        // Find and toggle DotIndicator for each button
        if (DailySignButton != null)
        {
            GameObject dailySignDot = DailySignButton.transform.Find("DotIndicator")?.gameObject;
            if (dailySignDot != null)
            {
                if (dayItemLoader != null)
                {
                    dailySignDot.SetActive(dayItemLoader.HasUnclaimedDays());
                }
                else
                {
                    dailySignDot.SetActive(false);
                    Debug.LogWarning("dayItemLoader reference is missing in AssignmentPageController!");
                }
            }
        }
        
        if (SevenDQuestButton != null)
        {
            GameObject sevenDQuestDot = SevenDQuestButton.transform.Find("DotIndicator")?.gameObject;
            if (sevenDQuestDot != null)
            {
                if (sevenDQuestController != null)
                {
                    // Check for unclaimed tasks in unlocked days only
                    bool hasUnclaimedAvailableTasks = false;
                    DateTime currentDate = DateTime.Now;
                    for (int day = 1; day <= 7; day++)
                    {
                        DateTime unlockDate = sevenDQuestController.questStartDate.AddDays(day - 1);
                        bool isUnlocked = currentDate >= unlockDate;
                        if (isUnlocked)
                        {
                            for (int task = 1; task <= 5; task++)
                            {
                                string claimKey = string.Format("TaskClaim_Day_{0}_Task_{1}", day, task);
                                if (PlayerPrefs.GetInt(claimKey, 0) == 0) // Unclaimed
                                {
                                    hasUnclaimedAvailableTasks = true;
                                    break;
                                }
                            }
                            if (hasUnclaimedAvailableTasks) break;
                        }
                    }
                    sevenDQuestDot.SetActive(hasUnclaimedAvailableTasks);
                }
                else
                {
                    sevenDQuestDot.SetActive(false);
                    Debug.LogWarning("sevenDQuestController reference is missing in AssignmentPageController!");
                }
            }
        }
        
        if (DailyQuestButton != null)
        {
            GameObject dailyQuestDot = DailyQuestButton.transform.Find("DotIndicator")?.gameObject;
            if (dailyQuestDot != null)
            {
                if (dailyQuestController != null)
                {
                    // Look for unclaimed tasks or boxes in DailyQuestController
                    bool hasUnclaimedItems = HasDailyQuestUpdates();
                    dailyQuestDot.SetActive(hasUnclaimedItems);
                }
                else
                {
                    dailyQuestDot.SetActive(false);
                    Debug.LogWarning("dailyQuestController reference is missing in AssignmentPageController!");
                }
            }
        }
        
        // Update the main AssignmentButton dot
        UpdateMainButtonDotStatus();

        Debug.Log($"Dot statuses updated for page {selectedPage}");
    }

    // Method to check for unclaimed items in DailyQuestController
    private bool HasDailyQuestUpdates()
    {
        if (dailyQuestController == null) return false;
        
        bool hasUnclaimedTasks = false;
        
        // Check for unclaimed tasks
        for (int i = 1; i <= 6; i++)
        {
            string taskClaimKey = string.Format("DailyQuest_Task_{0}", i);
            if (PlayerPrefs.GetInt(taskClaimKey, 0) == 0)
            {
                hasUnclaimedTasks = true;
                break;
            }
        }
        
        if (hasUnclaimedTasks) return true;
        
        // Check for boxes that can be claimed based on points
        int currentPoints = PlayerPrefs.GetInt("DailyQuest_Points", 0);
        for (int i = 1; i <= 4; i++)
        {
            int requiredPoints = i * 25; // 25, 50, 75, 100
            if (currentPoints >= requiredPoints)
            {
                string boxClaimKey = string.Format("DailyQuest_Box_{0}", i);
                if (PlayerPrefs.GetInt(boxClaimKey, 0) == 0)
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    // Method to check all dots and update the main button dot
    public void UpdateMainButtonDotStatus()
    {
        if (AssignmentButton == null) return;
        
        GameObject mainDotIndicator = AssignmentButton.transform.Find("DotIndicator")?.gameObject;
        if (mainDotIndicator == null) return;
        
        // Check if any of the page dots are active
        bool anyDotActive = false;
        
        // Check DailySign dot
        if (DailySignButton != null)
        {
            GameObject dailySignDot = DailySignButton.transform.Find("DotIndicator")?.gameObject;
            if (dailySignDot != null && dailySignDot.activeSelf)
            {
                anyDotActive = true;
            }
        }
        
        // Check SevenDQuest dot
        if (!anyDotActive && SevenDQuestButton != null)
        {
            GameObject sevenDQuestDot = SevenDQuestButton.transform.Find("DotIndicator")?.gameObject;
            if (sevenDQuestDot != null && sevenDQuestDot.activeSelf)
            {
                anyDotActive = true;
            }
        }
        
        // Check DailyQuest dot
        if (!anyDotActive && DailyQuestButton != null)
        {
            GameObject dailyQuestDot = DailyQuestButton.transform.Find("DotIndicator")?.gameObject;
            if (dailyQuestDot != null && dailyQuestDot.activeSelf)
            {
                anyDotActive = true;
            }
        }
        
        // Update main button dot based on child dots
        mainDotIndicator.SetActive(anyDotActive);
    }
}