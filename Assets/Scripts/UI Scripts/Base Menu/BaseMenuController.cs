using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BaseMenuController : MonoBehaviour
{
    public GameObject commonPage;
    public GameObject baseScrollView;

    public GameObject expeditionPage;
    public GameObject mailHall;
    public GameObject braveTrialPage;
    public GameObject abyssClashPage;
    public GameObject divineTowerPage;
    public GameObject defensePage;
    public GameObject rewardsPage; // Assign RewardsPage in Inspector

    public GameObject leaderPage;
    public GameObject leaderItemPrefab; // Assign LeaderItem prefab in Inspector
    public Transform leaderContent; // Assign Content (inside Scroll View) in Inspector
    private List<GameObject> spawnedLeaders = new List<GameObject>(); // Track spawned items
   
    public GameObject achievePage;
    public GameObject achieveItemPrefab; // Assign LeaderItem prefab in Inspector
    public Transform achieveContent; // Assign Content (inside Scroll View) in Inspector
    private List<GameObject> spawnedAchievers = new List<GameObject>(); // Track spawned items

    public GameObject tavernPage;
    public GameObject travernButton1; // Assign Button_1 in TravenPage
    public GameObject travernButton2; // Assign Button_2 in TravenPage
    private const string lastEnergyClaimTimeKey1 = "LastEnergyClaimTime_12PM";
    private const string lastEnergyClaimTimeKey2 = "LastEnergyClaimTime_19PM";

    public TextMeshProUGUI attemptsText; // Assign AttemptsText in Inspector
    public Button saluteButton; // Assign Salute Button in Inspector
    private const string lastSaluteKey = "LastSaluteDate";
    private const string saluteAttemptsKey = "SaluteAttempts";
    private const int maxAttempts = 3;
    private int remainingAttempts;

    private void Start()
    {
        CheckEnergyClaimStatus(); // Check button visibility for travern's energy claim on start
        CheckResetDaily(); // Reset if a new day
        UpdateAttempts();
    }

    public void CloseBackground()
    {
        baseScrollView.SetActive(false);
        commonPage.SetActive(false);
    }

    public void OpenRewardsPage()
    {
        rewardsPage.SetActive(true);
        commonPage.SetActive(false);
        baseScrollView.SetActive(false);
        // ⚠ Reward claim logic (12-hour countup) is handled in OfflineTimer.cs
    }

    public void CloseRewardsPage()
    {
        rewardsPage.SetActive(false);
        commonPage.SetActive(true);
        baseScrollView.SetActive(true);
    }

    // ******************************************************************
    // LeaderPage Codes begin here
    // ******************************************************************
    public void OpenLeaderPage()
    {
        leaderPage.SetActive(true);
        commonPage.SetActive(false);
        baseScrollView.SetActive(false);
        LoadLeaderItems(); // 🔹 Load dynamically when page opens
    }

    public void CloseLeaderPage()
    {
        leaderPage.SetActive(false);
        commonPage.SetActive(true);
        baseScrollView.SetActive(true);
    }

    private void LoadLeaderItems()
    {
        // Clear existing items
        foreach (GameObject item in spawnedLeaders)
        {
            Destroy(item);
        }
        spawnedLeaders.Clear();

        // Load ranking images (4 to 20)
        string rankPath = "UILoading/LeaderRank";
        Sprite[] rankImages = Resources.LoadAll<Sprite>(rankPath);

        // Load user icons (Mock logic for now)
        string iconPath = "UILoading/CharacterImages/UserIcons/WhiteBorderIcons";
        Sprite[] userIcons = Resources.LoadAll<Sprite>(iconPath);

        // Loop from rank 4 to 20
        for (int rank = 4; rank <= 20; rank++)
        {
            GameObject newItem = Instantiate(leaderItemPrefab, leaderContent);
            newItem.name = $"Leader_{rank}";

            // **Set spacing dynamically after first item instantiation**
            if (rank == 4) 
            {
                float itemHeight = newItem.GetComponent<RectTransform>().rect.height;
                leaderContent.GetComponent<VerticalLayoutGroup>().spacing = itemHeight / 6f;            
            }

            // Assign Rank Image
            Image rankImage = newItem.transform.Find("Rank").GetComponent<Image>();
            rankImage.sprite = Array.Find(rankImages, img => img.name == rank.ToString());

            // Assign Icon Image (Mock logic: Load icons randomly)
            Image iconImage = newItem.transform.Find("Icon").GetComponent<Image>();
            if (userIcons.Length > 0)
            {
                iconImage.sprite = userIcons[UnityEngine.Random.Range(0, userIcons.Length)];
            }

            spawnedLeaders.Add(newItem);
        }
    }

    private void CheckResetDaily()
    {
        string lastSaluteDate = PlayerPrefs.GetString(lastSaluteKey, "");
        string today = DateTime.Now.ToString("yyyy-MM-dd");

        if (lastSaluteDate != today)
        {
            PlayerPrefs.SetString(lastSaluteKey, today);
            PlayerPrefs.SetInt(saluteAttemptsKey, maxAttempts);
            PlayerPrefs.Save();
        }

        remainingAttempts = PlayerPrefs.GetInt(saluteAttemptsKey, maxAttempts);
    }

    public void Salute()
    {
        if (remainingAttempts > 0)
        {
            remainingAttempts--;
            PlayerPrefs.SetInt(saluteAttemptsKey, remainingAttempts);
            PlayerPrefs.Save();
            UpdateAttempts();
        }
    }

    private void UpdateAttempts()
    {
        attemptsText.text = $"Attempts: {remainingAttempts}/{maxAttempts}";
        // saluteButton.interactable = remainingAttempts > 0;
        saluteButton.gameObject.SetActive(remainingAttempts > 0);
    }

    // ******************************************************************
    // LeaderPage Codes end here
    // ******************************************************************


    public void OpenDefensePage()
    {
        defensePage.SetActive(true);
        commonPage.SetActive(false);
        baseScrollView.SetActive(false);
    }

    public void CloseDefensePage()
    {
        defensePage.SetActive(false);
        commonPage.SetActive(true);
        baseScrollView.SetActive(true);
    }

    public void OpenAchievePage()
    {
        achievePage.SetActive(true);
        commonPage.SetActive(false);
        baseScrollView.SetActive(false);

        LoadAchieveItems();
    }

    private void LoadAchieveItems()
    {
        // Clear existing items
        foreach (GameObject item in spawnedAchievers)
        {
            Destroy(item);
        }
        spawnedAchievers.Clear();

        // Load user icons (Mock logic: Randomly load 20 icons)
        string iconPath = "UILoading/CharacterImages/UserIcons/WhiteBorderIcons";
        Sprite[] userIcons = Resources.LoadAll<Sprite>(iconPath);

        for (int i = 0; i < 20; i++)
        {
            GameObject newItem = Instantiate(achieveItemPrefab, achieveContent);
            newItem.name = $"Achieve_{i + 1}";

            // Assign Icon Image (Random mock logic)
            Image iconImage = newItem.transform.Find("Icon").GetComponent<Image>();
            if (userIcons.Length > 0)
            {
                iconImage.sprite = userIcons[UnityEngine.Random.Range(0, userIcons.Length)];
            }

            spawnedAchievers.Add(newItem);
        }
    }

    public void CloseAchievePage()
    {
        achievePage.SetActive(false);
        commonPage.SetActive(true);
        baseScrollView.SetActive(true);
    }

    // ******************************************************************
    // ExpeditionPage Codes Begins
    // ******************************************************************
    public void OpenExpeditionPage()
    {
        expeditionPage.SetActive(true);
        commonPage.SetActive(false);
        baseScrollView.SetActive(false);
    }

    public void CloseExpeditionPage()
    {
        expeditionPage.SetActive(false);
        commonPage.SetActive(true);
        baseScrollView.SetActive(true);
    }

    public void OpenBraveTrialPage()
    {
        braveTrialPage.SetActive(true);
        mailHall.SetActive(false);
    }

    public void CloseBraveTrialPage()
    {
        braveTrialPage.SetActive(false);
        mailHall.SetActive(true);
    }

    public void OpenAbyssClashPage()
    {
        abyssClashPage.SetActive(true);
        mailHall.SetActive(false);
    }

    public void CloseAbyssClashPage()
    {
        abyssClashPage.SetActive(false);
        mailHall.SetActive(true);
    }

    public void OpenDivineTowerPage()
    {
        divineTowerPage.SetActive(true);
        mailHall.SetActive(false);
    }

    public void CloseDivineTowerPage()
    {
        divineTowerPage.SetActive(false);
        mailHall.SetActive(true);
    }
    // ******************************************************************
    // ExpeditionPage Codes Ends here
    // ******************************************************************


    // ******************************************************************
    // TraverPage Codes Begins
    // ******************************************************************
    public void OpenTavernPage()
    {
        tavernPage.SetActive(true);
        commonPage.SetActive(false);
        baseScrollView.SetActive(false);
        CheckEnergyClaimStatus(); // Ensure buttons update when page opens
    }

    public void CloseTavernPage()
    {
        tavernPage.SetActive(false);
        commonPage.SetActive(true);
        baseScrollView.SetActive(true);
    }

    private void CheckEnergyClaimStatus()
    {
        DateTime now = DateTime.Now;
        DateTime today12PM = new DateTime(now.Year, now.Month, now.Day, 12, 0, 0);
        DateTime today19PM = new DateTime(now.Year, now.Month, now.Day, 19, 0, 0);

        // Check Button_1 visibility
        if (now >= today12PM && !HasEnergyClaimedToday(lastEnergyClaimTimeKey1))
        {
            travernButton1.SetActive(true);
        }
        else
        {
            travernButton1.SetActive(false);
        }

        // Check Button_2 visibility
        if (now >= today19PM && !HasEnergyClaimedToday(lastEnergyClaimTimeKey2))
        {
            travernButton2.SetActive(true);
        }
        else
        {
            travernButton2.SetActive(false);
        }
    }

    private bool HasEnergyClaimedToday(string key)
    {
        if (PlayerPrefs.HasKey(key))
        {
            string lastEnergyClaimString = PlayerPrefs.GetString(key);
            DateTime lastEnergyClaimTime = DateTime.Parse(lastEnergyClaimString);
            return lastEnergyClaimTime.Date == DateTime.Now.Date; // Same day = already claimed
        }
        return false;
    }

    public void ClaimEnergy(int buttonIndex)
    {
        if (buttonIndex == 1)
        {
            PlayerPrefs.SetString(lastEnergyClaimTimeKey1, DateTime.Now.ToString());
            travernButton1.SetActive(false);
        }
        else if (buttonIndex == 2)
        {
            PlayerPrefs.SetString(lastEnergyClaimTimeKey2, DateTime.Now.ToString());
            travernButton2.SetActive(false);
        }
        PlayerPrefs.Save();
    }
    // ******************************************************************
    // TraverPage Codes end here
    // ******************************************************************
    
}