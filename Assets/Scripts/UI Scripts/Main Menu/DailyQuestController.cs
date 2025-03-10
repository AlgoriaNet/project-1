using UnityEngine;
using UnityEngine.UI;

public class DailyQuestController : MonoBehaviour
{
    [SerializeField] private Transform taskPanel;
    [SerializeField] private Transform boxPanel;
    [SerializeField] private GameObject dotIndicator;
    [SerializeField] private Color greyishColor = new Color(0.5f, 0.5f, 0.5f);
    [SerializeField] private Transform greyLine;
    [SerializeField] private ResourceManager resourceManager;

    private const string TaskClaimKeyPrefix = "DailyQuest_Task_{0}";
    private const string BoxClaimKeyPrefix = "DailyQuest_Box_{0}";
    private const string PointsKey = "DailyQuest_Points";
    private const string LastResetKey = "DailyQuest_LastReset";
    private const int pointsPerClaim = 25;
    private const int maxPoints = 100;
    private const int goldPerClaim = 100;
    private const int totalTasks = 6;
    private int currentPoints = 0;
    private RectTransform greenLine;

    public void Start()
    {
        if (!ValidateComponents()) return;
        CheckDailyReset();
        currentPoints = PlayerPrefs.GetInt(PointsKey, 0);
        RefreshTaskPanel();
    }

    private bool ValidateComponents()
    {
        if (taskPanel == null) { Debug.LogError("TaskPanel not assigned!"); return false; }
        if (boxPanel == null) { Debug.LogError("BoxPanel not assigned!"); return false; }
        if (dotIndicator == null) { Debug.LogWarning("DotIndicator not assigned!"); }
        if (greyLine == null) { Debug.LogError("GreyLine not assigned!"); return false; }
        if (resourceManager == null) { Debug.LogError("ResourceManager not assigned!"); return false; }

        greenLine = greyLine.Find("GreenLine")?.GetComponent<RectTransform>();
        if (greenLine == null) { Debug.LogError("GreenLine not found!"); return false; }
        return true;
    }

    private void CheckDailyReset()
    {
        string lastReset = PlayerPrefs.GetString(LastResetKey, "");
        string today = System.DateTime.Now.ToString("yyyy-MM-dd");
        
        if (lastReset != today)
        {
            ResetDaily();
            PlayerPrefs.SetString(LastResetKey, today);
            PlayerPrefs.Save();
        }
    }

    public void OnPageActivated()
    {
        if (!ValidateComponents()) return;
        CheckDailyReset();
        RefreshTaskPanel();
    }

    private void RefreshTaskPanel()
    {
        bool hasUnclaimed = false;
        float progress = currentPoints / (float)maxPoints;
        greenLine.anchorMax = new Vector2(progress, 1f);

        // Task panel refresh
        for (int i = 1; i <= totalTasks; i++)
        {
            Transform taskTransform = taskPanel.Find($"Task_{i}");
            if (taskTransform == null) continue;

            Transform blockTransform = taskTransform.Find("Block");
            Image blockImage = blockTransform?.Find("Image")?.GetComponent<Image>();
            GameObject tick = blockTransform?.Find("Tick")?.gameObject;
            Button claimButton = taskTransform.Find("ClaimButton")?.GetComponent<Button>();

            string taskClaimKey = string.Format(TaskClaimKeyPrefix, i);
            bool isTaskClaimed = PlayerPrefs.GetInt(taskClaimKey, 0) == 1;

            if (blockImage != null && tick != null && claimButton != null)
            {
                blockImage.color = isTaskClaimed ? greyishColor : Color.white;
                tick.SetActive(isTaskClaimed);
                claimButton.gameObject.SetActive(!isTaskClaimed);
                claimButton.interactable = !isTaskClaimed;

                claimButton.onClick.RemoveAllListeners();
                if (!isTaskClaimed)
                {
                    int taskIndex = i;
                    claimButton.onClick.AddListener(() => OnClaimTask(taskIndex));
                    hasUnclaimed = true;
                }
            }
        }

        // Box panel refresh
        for (int i = 1; i <= 4; i++)
        {
            Transform blockTransform = boxPanel.Find($"Block_{i}");
            if (blockTransform == null) continue;

            Transform closeBox = blockTransform.Find("CloseBox");
            Transform claimText = closeBox?.Find("ClaimText");
            Button claimTextButton = claimText?.GetComponent<Button>();
            Transform openBox = blockTransform.Find("OpenBox");

            string boxClaimKey = string.Format(BoxClaimKeyPrefix, i);
            bool isBoxClaimed = PlayerPrefs.GetInt(boxClaimKey, 0) == 1;

            if (closeBox != null && claimText != null && claimTextButton != null && openBox != null)
            {
                closeBox.gameObject.SetActive(!isBoxClaimed);
                openBox.gameObject.SetActive(isBoxClaimed);

                // Simple points check to show ClaimText
                bool canShowText = currentPoints >= (i * pointsPerClaim); // 25, 50, 75, 100
                claimText.gameObject.SetActive(canShowText && !isBoxClaimed);

                if (canShowText && !isBoxClaimed)
                {
                    hasUnclaimed = true;
                    // Add button functionality
                    claimTextButton.onClick.RemoveAllListeners();
                    int boxIndex = i; // Local variable to capture correct box index
                    claimTextButton.onClick.AddListener(() => OnClaimBox(boxIndex));
                }
            }
            else
            {
                if (closeBox == null) Debug.LogError($"CloseBox not found in Block_{i}!");
                if (claimText == null) Debug.LogError($"ClaimText not found in CloseBox of Block_{i}!");
                if (claimTextButton == null) Debug.LogError($"Button component not found on ClaimText in Block_{i}!");
                if (openBox == null) Debug.LogError($"OpenBox not found in Block_{i}!");
            }
        }

        if (dotIndicator != null)
            dotIndicator.SetActive(hasUnclaimed);
    }

    private void OnClaimTask(int taskIndex)
    {
        string taskClaimKey = string.Format(TaskClaimKeyPrefix, taskIndex);
        PlayerPrefs.SetInt(taskClaimKey, 1);

        int currentGold = PlayerPrefs.GetInt("PlayerGold", 300);
        PlayerPrefs.SetInt("PlayerGold", currentGold + goldPerClaim);
        resourceManager.UpdateResourceUI();

        if (currentPoints < maxPoints)
        {
            currentPoints += pointsPerClaim;
            PlayerPrefs.SetInt(PointsKey, currentPoints);
        }
        
        PlayerPrefs.Save();
        RefreshTaskPanel();
    }

    private void OnClaimBox(int boxIndex)
    {
        Debug.Log($"Claiming box {boxIndex}");
        string boxClaimKey = string.Format(BoxClaimKeyPrefix, boxIndex);
        PlayerPrefs.SetInt(boxClaimKey, 1);
        
        // Add rewards here if needed
        int rewardAmount = boxIndex * 50; // Example: increasing rewards
        int currentGold = PlayerPrefs.GetInt("PlayerGold", 300);
        PlayerPrefs.SetInt("PlayerGold", currentGold + rewardAmount);
        
        if (resourceManager != null)
        {
            resourceManager.UpdateResourceUI();
        }
        
        PlayerPrefs.Save();
        RefreshTaskPanel();
    }

    private void ResetDaily()
    {
        currentPoints = 0;
        PlayerPrefs.SetInt(PointsKey, currentPoints);

        for (int i = 1; i <= totalTasks; i++)
        {
            PlayerPrefs.SetInt(string.Format(TaskClaimKeyPrefix, i), 0);
        }
        for (int i = 1; i <= 4; i++)
        {
            PlayerPrefs.SetInt(string.Format(BoxClaimKeyPrefix, i), 0);
        }
        PlayerPrefs.Save();
        RefreshTaskPanel();
    }
}