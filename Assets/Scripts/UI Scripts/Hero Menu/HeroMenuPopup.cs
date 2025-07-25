using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;


public class HeroMenuPopup : MonoBehaviour
{
    public GameObject upDownMenu; 
    public GameObject heroStep2; 
    public GameObject heroPopup; // Parent panel (semi-transparent background)
    public GameObject skinPage;
    public GameObject gunPage;
    public GameObject gemPage;
    public Button skinButton;
    public Button gunButton;
    public Button gemButton;
    
    public Button closeGunButton;
    public Button closeGemButton;
    public Button closeSkinButton; // Added close button for the skin page

    public Transform heroStep2ContentPanel;  // The original content panel that holds the BlockItems

    public GameObject skillItemPrefab; // Assign SkillItem prefab in Inspector
    public Transform skillContentPanel;  // Assign "Content" inside ScrollView
    private GameObject activeDescription = null; 


    // Mock List for test only, Need to delete later. 
    private List<(string skillId, string skillName, string skillImagePath, bool isActive)> skillImagePaths = new List<(string, string, string, bool)>
    {
        ("Skill 1", "Bullet Mutation", "01_Bullet Mutation", false),
        ("Skill 2", "Bullet +1", "02_Bullet +1", false),
        ("Skill 3", "Bullet +1", "02_Bullet +1", false),
        ("Skill 4", "Crit Damage", "03_Crit Damage", false),
        ("Skill 5", "Crit Damage", "03_Crit Damage", false),
        ("Skill 6", "Crit Damage", "03_Crit Damage", true),
        ("Skill 7", "Crit Rate", "04_Crit Rate", true),
        ("Skill 8", "Gun Damage", "05_Gun Damage", true),
        ("Skill 9", "Gun Damage", "05_Gun Damage", true),
        ("Skill 10", "Crit Rate", "04_Crit Rate", true)
    };


    void Start()
    {
        // Ensure all popups are initially hidden
        heroPopup.SetActive(false);
        skinPage.SetActive(false);
        gunPage.SetActive(false);
        gemPage.SetActive(false);

        // Assign button listeners
        skinButton.onClick.AddListener(OpenSkinPage);
        gunButton.onClick.AddListener(OpenGunPage);
        gemButton.onClick.AddListener(OpenGemPage);

        closeGunButton.onClick.AddListener(CloseGunPage);
        closeGemButton.onClick.AddListener(CloseGemPage);
        closeSkinButton.onClick.AddListener(CloseSkinPage); // Assign listener for skin page close button
    }

    private void OpenSkinPage()
    {
        heroPopup.SetActive(true);
        skinPage.SetActive(true);
        heroStep2.SetActive(false);
        upDownMenu.SetActive(false);
        Debug.Log("Skin panel opened.");
    }

    // 🔹 Call LoadSkillItems when GunPage opens
    private void OpenGunPage()
    {
        heroPopup.SetActive(true);
        gunPage.SetActive(true);
        heroStep2.SetActive(false);
        upDownMenu.SetActive(false);
        LoadSkillItems(); // ✅ Load skills here
        Debug.Log("Gun panel opened.");
    }

    private void OpenGemPage()
    {
        heroPopup.SetActive(true);
        gemPage.SetActive(true);
        heroStep2.SetActive(false);
        Debug.Log("Gem panel opened.");
    }


    public void CloseSkinPage()
    {
        skinPage.SetActive(false);
        heroStep2.SetActive(true);
        Debug.Log("Skin panel closed.");
        CheckAndCloseHeroPopup();
    }

    public void CloseGunPage()
    {
        gunPage.SetActive(false);
        heroStep2.SetActive(true);
        Debug.Log("Gun panel closed.");
        CheckAndCloseHeroPopup();
    }

    public void CloseGemPage()
    {
        gemPage.SetActive(false);
        heroStep2.SetActive(true);
        Debug.Log("Gem panel closed.");
        CheckAndCloseHeroPopup();
    }


    private void CheckAndCloseHeroPopup()
    {
        // Close the parent panel if no pages are active
        if (!skinPage.activeSelf && !gunPage.activeSelf && !gemPage.activeSelf)
        {
            heroPopup.SetActive(false);
            heroStep2.SetActive(true);
            upDownMenu.SetActive(true);
            Debug.Log("HeroPopup closed.");
        }
    }
    
    private void LoadSkillItems()
    {
        // Clear existing skill items only
        foreach (Transform child in skillContentPanel)
        {
            Destroy(child.gameObject);
        }

        // Instantiate skill items dynamically
        foreach (var skill in skillImagePaths)
        {
            string skillId = skill.skillId;
            string skillName = skill.skillName;
            string skillImagePath = skill.skillImagePath;
            bool isActive = skill.isActive;

            // Instantiate new skill item
            GameObject newSkillItem = Instantiate(skillItemPrefab, skillContentPanel);
            newSkillItem.name = skillId;

            // ✅ Assign Skill Name
            var skillNameText = newSkillItem.transform.Find("Description/Block/SkillName").GetComponent<TextMeshProUGUI>();
            if (skillNameText != null) skillNameText.text = skillName;

            // ✅ Load & Assign Skill Image (For both SkillImage positions)
            string path = $"UILoading/SkillItem/{skillImagePath}";
            Sprite skillSprite = Resources.Load<Sprite>(path);

            if (skillSprite != null)
            {
                // Set SkillImage (Main)
                var skillImageMain = newSkillItem.transform.Find("SkillImage").GetComponent<Image>();
                if (skillImageMain != null) skillImageMain.sprite = skillSprite;

                // Set SkillImage (Inside Description Block)
                var skillImageBlock = newSkillItem.transform.Find("Description/Block/SkillImage").GetComponent<Image>();
                if (skillImageBlock != null) skillImageBlock.sprite = skillSprite;
            }
            else
            {
                Debug.LogWarning($"❌ Image not found at path: {path}");
            }

            // ✅ Add Button Click Event to Show Description
            Button skillButton = newSkillItem.GetComponent<Button>();
            if (skillButton != null)
            {
                skillButton.onClick.AddListener(() => ToggleDescription(newSkillItem));
            }

            // ✅ Ensure description starts hidden
            Transform description = newSkillItem.transform.Find("Description");
            if (description != null) description.gameObject.SetActive(false);

            // ✅ Hide button inside the block if the skill is not active
            var blockButton = newSkillItem.transform.Find("Description/Block/Button").GetComponent<Button>();
            var shade = newSkillItem.transform.Find("Shade").GetComponent<Image>();

            if (!isActive)
            {
                if (blockButton != null)
                {
                    blockButton.gameObject.SetActive(false); // Disable Button if the skill is inactive
                }

                if (shade != null)
                {
                    shade.gameObject.SetActive(true); // Enable Shade if the skill is inactive
                }
            }
        }
    }

    // ✅ Function to toggle descriptions
    private void ToggleDescription(GameObject selectedSkill)
    {
        Transform selectedDesc = selectedSkill.transform.Find("Description");

        if (selectedDesc == null) return; // If no description, return

        // Disable the previous active description
        if (activeDescription != null && activeDescription != selectedDesc.gameObject)
        {
            activeDescription.SetActive(false);
        }

        // Toggle the current description
        bool isActive = selectedDesc.gameObject.activeSelf;
        selectedDesc.gameObject.SetActive(!isActive);

        // Update activeDescription reference
        activeDescription = selectedDesc.gameObject.activeSelf ? selectedDesc.gameObject : null;
    }
}
