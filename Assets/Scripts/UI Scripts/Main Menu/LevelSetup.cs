using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;

public class LevelSetup : MonoBehaviour
{
    [System.Serializable]
    public class Level
    {
        public string LevelNumber; // e.g., 1.1, 1.2
        public string LevelName; // e.g., "Introduction"
    }

    public RectTransform mainMenu; // Assign the Main Menu RectTransform in Inspector
    public RectTransform levelGroup; // Assign Level Group in Inspector
    public RectTransform levelImage; // Assign Level Image in Inspector
    public Button leftArrow; // Assign Left Arrow in Inspector
    public Button rightArrow; // Assign Right Arrow in Inspector
    public TextMeshProUGUI levelText; // Assign Text for Level Name and Number
    public Sprite[] levelImages; // Assign Level Images in Inspector
    public int totalLevels = 25; // Total number of levels
    public int currentLevel = 1; // Current unlocked level
    public string csvFilePath; // Path to the CSV file in Resources

    private int currentIndex = 1;
    private List<Level> levels = new List<Level>();

    void Start()
    {
        float mainMenuWidth = mainMenu.rect.width;
        float dimension = mainMenuWidth / 2;
        levelGroup.sizeDelta = new Vector2(dimension, dimension);

        // Load Levels from CSV
        LoadLevelsFromCSV();

        // Set Initial Level View
        UpdateLevelView();

        // Set Arrow Button Listeners
        leftArrow.onClick.AddListener(ShowPreviousLevel);
        rightArrow.onClick.AddListener(ShowNextLevel);
    }

    void LoadLevelsFromCSV()
    {
        TextAsset csvFile = Resources.Load<TextAsset>(csvFilePath.Replace(".csv", ""));
        if (csvFile == null)
        {
            Debug.LogError($"CSV file not found at Resources/{csvFilePath}");
            return;
        }

        string[] lines = csvFile.text.Split('\n');
        for (int i = 1; i < lines.Length; i++) // Skip header row
        {
            string[] values = lines[i].Split(',');
            if (values.Length < 3) continue; // Skip invalid rows
            levels.Add(new Level
            {
                LevelNumber = values[0].Trim(), // Column 0: Level number (e.g., "1.1")
                LevelName = values[1].Trim()   // Column 1: English name (e.g., "Introduction")
            });
        }

        totalLevels = levels.Count;
    }

    void UpdateLevelView()
    {
        // Update Level Image
        int imageIndex = (currentIndex - 1) / 5; // Determine image based on level range
        levelImage.GetComponent<Image>().sprite = levelImages[imageIndex];

        // Update Level Text
        Level currentLevelData = levels[currentIndex - 1];
        levelText.text = $"{currentLevelData.LevelNumber}. {currentLevelData.LevelName}";

        // Update Arrow Buttons
        leftArrow.interactable = currentIndex > 1;
        rightArrow.interactable = currentIndex < totalLevels;

        // Update Interactivity and Color
        if (currentIndex > currentLevel)
        {
            levelImage.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f); // Greyed out
        }
        else
        {
            levelImage.GetComponent<Image>().color = Color.white; // Normal shade
        }
    }

    void ShowPreviousLevel()
    {
        if (currentIndex > 1)
        {
            currentIndex--;
            UpdateLevelView();
        }
    }

    void ShowNextLevel()
    {
        if (currentIndex < totalLevels)
        {
            currentIndex++;
            UpdateLevelView();
        }
    }
}