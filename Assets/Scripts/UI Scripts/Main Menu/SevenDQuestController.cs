using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SevenDQuestController : MonoBehaviour
{
    [SerializeField] private Transform dayPanel; // Assign DayPanel in Inspector
    [SerializeField] private Transform taskPanel; // Assign TaskPanel in Inspector
    [SerializeField] private TextMeshProUGUI progressText; // Assign ProgressText in Inspector
    [SerializeField] private GameObject dotIndicator; // Assign DotIndicator GameObject in Inspector
    [SerializeField] private Color unlockedColor = new Color(0.596f, 0.310f, 0.98f); // Hex 984FFA in RGB
    [SerializeField] private Color greyishColor = new Color(0.5f, 0.5f, 0.5f); // Greyish color for claimed
    [SerializeField] private Color redColor = new Color(1f, 0f, 0f); // Red color for percentage
    public DateTime questStartDate; // Store the start date for the 7-day quest cycle
    private const string QuestStartDateKey = "SevenDQuestStartDate"; // PlayerPrefs key
    private const string TaskClaimKeyPrefix = "TaskClaim_Day_{0}_Task_{1}"; // PlayerPrefs key for task claims
    private const string goldKey = "PlayerGold"; // PlayerPrefs key for gold, matching ResourceManager
    private const int TotalTasks = 35; // 7 days * 5 tasks
    private const int goldPerClaim = 100; // 100 gold per claim

    public void Start()
    {
        questStartDate = StartGame.firstLoginTime;
        if (questStartDate == default)
        {
            questStartDate = DateTime.Now;
        }
        PlayerPrefs.SetString(QuestStartDateKey, questStartDate.ToString("o"));
        PlayerPrefs.Save();

        UpdateDayStates();
        SetupDayButtons();

        // Default to Day 1
        OnDayButtonClicked(1);
        UpdateProgressText();
    }

    public void UpdateDayStates()
    {
        if (dayPanel == null)
        {
            Debug.LogError("DayPanel is not assigned!");
            return;
        }

        DateTime currentDate = DateTime.Now;
        for (int i = 1; i <= 7; i++)
        {
            Transform dayTransform = dayPanel.Find($"Day_{i}");
            if (dayTransform != null)
            {
                TextMeshProUGUI dayText = dayTransform.Find("DayText")?.GetComponent<TextMeshProUGUI>();
                Transform lockTransform = dayTransform.Find("Lock");
                GameObject lockObject = lockTransform != null ? lockTransform.gameObject : null;

                if (dayText != null && lockObject != null)
                {
                    DateTime unlockDate = questStartDate.AddDays(i - 1);
                    bool isUnlocked = currentDate >= unlockDate;
                    lockObject.SetActive(!isUnlocked);
                    dayText.color = isUnlocked ? unlockedColor : Color.black;
                    Button dayButton = dayTransform.GetComponent<Button>();
                    if (dayButton != null)
                    {
                        dayButton.interactable = isUnlocked;
                    }
                }
            }
        }
    }

    private void SetupDayButtons()
    {
        for (int i = 1; i <= 7; i++)
        {
            Transform dayTransform = dayPanel.Find($"Day_{i}");
            if (dayTransform != null)
            {
                Button dayButton = dayTransform.GetComponent<Button>();
                if (dayButton != null)
                {
                    int dayIndex = i;
                    dayButton.onClick.RemoveAllListeners();
                    dayButton.onClick.AddListener(() => OnDayButtonClicked(dayIndex));
                }
            }
        }
    }

    private void OnDayButtonClicked(int day)
    {
        DateTime currentDate = DateTime.Now;
        DateTime unlockDate = questStartDate.AddDays(day - 1);
        if (currentDate < unlockDate)
        {
            Debug.Log($"Day {day} is locked!");
            return;
        }

        RefreshTaskPanel(day);
    }

    private void RefreshTaskPanel(int day)
    {
        if (taskPanel == null)
        {
            Debug.LogError("TaskPanel is not assigned!");
            return;
        }

        Debug.Log("Now calling SevenDQuestController's RefreshTaskPanel");
        for (int i = 1; i <= 5; i++)
        {
            Transform taskTransform = taskPanel.Find($"Task_{i}");
            if (taskTransform != null)
            {
                // -- Task_i
                Transform blockTransform = taskTransform.Find("Block");
                if (blockTransform != null)
                {
                    // -- Block: Dynamic width calculation based on formula
                    RectTransform blockRect = blockTransform.GetComponent<RectTransform>();
                    Image blockImage = blockTransform.Find("Image")?.GetComponent<Image>();
                    GameObject tick = blockTransform.Find("Tick")?.gameObject;
                    
                    // -- ClaimButton: Unchanged
                    Button claimButton = taskTransform.Find("ClaimButton")?.GetComponent<Button>();

                    if (blockImage != null && tick != null && claimButton != null)
                    {
                        string claimKey = string.Format(TaskClaimKeyPrefix, day, i);
                        bool isClaimed = PlayerPrefs.GetInt(claimKey, 0) == 1;

                        blockImage.color = isClaimed ? greyishColor : Color.white;
                        tick.SetActive(isClaimed);
                        claimButton.gameObject.SetActive(!isClaimed);
                        claimButton.interactable = !isClaimed; // Changed to !isClaimed to disable if claimed

                        claimButton.onClick.RemoveAllListeners();
                        if (!isClaimed)
                        {
                            int taskIndex = i;
                            claimButton.onClick.AddListener(() => OnClaimTask(day, taskIndex));
                        }
                    }
                }
            }
        }
    }

    private void OnClaimTask(int day, int taskIndex)
    {
        string claimKey = string.Format(TaskClaimKeyPrefix, day, taskIndex);
        PlayerPrefs.SetInt(claimKey, 1);

        // Add 100 gold per claim
        int currentGold = PlayerPrefs.GetInt(goldKey, 0); // Default to 0 if not set
        PlayerPrefs.SetInt(goldKey, currentGold + goldPerClaim);

        PlayerPrefs.Save();

        Transform taskTransform = taskPanel.Find($"Task_{taskIndex}");
        if (taskTransform != null)
        {
            Transform blockTransform = taskTransform.Find("Block");
            if (blockTransform != null)
            {
                Image blockImage = blockTransform.Find("Image")?.GetComponent<Image>();
                GameObject tick = blockTransform.Find("Tick")?.gameObject;
                Button claimButton = taskTransform.Find("ClaimButton")?.GetComponent<Button>();

                if (blockImage != null && tick != null && claimButton != null)
                {
                    blockImage.color = greyishColor;
                    tick.SetActive(true);
                    claimButton.gameObject.SetActive(false);
                }
            }
        }

        UpdateProgressText();
    }

    private void UpdateProgressText()
    {
        if (progressText == null)
        {
            Debug.LogError("ProgressText is not assigned!");
            return;
        }

        int claimedCount = 0;
        int availableTasks = 0;
        DateTime currentDate = DateTime.Now;
        bool hasUnclaimedAvailableTasks = false;

        for (int day = 1; day <= 7; day++)
        {
            DateTime unlockDate = questStartDate.AddDays(day - 1);
            bool isUnlocked = currentDate >= unlockDate;

            for (int task = 1; task <= 5; task++)
            {
                string claimKey = string.Format(TaskClaimKeyPrefix, day, task);
                bool isClaimed = PlayerPrefs.GetInt(claimKey, 0) == 1;

                if (isClaimed)
                {
                    claimedCount++;
                }

                if (isUnlocked)
                {
                    availableTasks++;
                    if (!isClaimed)
                    {
                        hasUnclaimedAvailableTasks = true;
                    }
                }
            }
        }

        int percentage = (int)((float)claimedCount / TotalTasks * 100);
        string progressTextFormatted = $"Complete <color=red>{percentage}%</color> – Ultimate Reward Incoming";
        progressText.text = progressTextFormatted;

        if (dotIndicator != null)
        {
            // Show dot only if there are unclaimed tasks among currently unlocked days
            dotIndicator.SetActive(hasUnclaimedAvailableTasks);
        }
    }

    public bool HasLockedDays()
    {
        DateTime currentDate = DateTime.Now;
        for (int i = 1; i <= 7; i++)
        {
            DateTime unlockDate = questStartDate.AddDays(i - 1);
            if (currentDate < unlockDate)
            {
                return true;
            }
        }
        return false;
    }

    public void ResetQuestCycle()
    {
        questStartDate = DateTime.Now;
        PlayerPrefs.SetString(QuestStartDateKey, questStartDate.ToString("o"));
        PlayerPrefs.Save();
        UpdateDayStates();

        if (AssignmentPageController.Instance != null)
        {
            AssignmentPageController.Instance.UpdateDotStatuses();
        }
    }
}