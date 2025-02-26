using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class mailPopupController : MonoBehaviour
{
    public GameObject mailPopup; // Assign the mail Popup object
    public RectTransform board1; // Assign the Board object
    public RectTransform board2; // Assign the Board 2 object
    public RectTransform bowField; // Assign the BowField RectTransform
    private Vector2 originalBoard1Size; // Store the original board size
    private Vector2 originalBoard2Size; // Store the original board2 size

    private RectTransform mailPopupRect; // Reference to the mail Popup RectTransform
    public GameObject mailItemPrefab; // Assign the MailItem prefab
    public Transform contentTransform; // Assign the Content of ScrollView

    public GameObject step1Panel; // Assign Step 1 panel in the Inspector
    public GameObject step2Panel; // Assign Step 2 panel in the Inspector
    public Button deleteReadButton;
    public Button deleteButton;
    private GameObject currentOpenMailItem;


    void Start()
    {
        mailPopupRect = mailPopup.GetComponent<RectTransform>();
        originalBoard1Size = board1.sizeDelta;
        originalBoard2Size = board2.sizeDelta;

        LoadMailItems(); // Load stored mails when popup opens

        deleteReadButton.onClick.AddListener(DeleteReadMails);
        deleteButton.onClick.AddListener(DeleteCurrentMail);
    }

    void Update()
    {
        // Ensure the board adjusts dynamically if the popup is active
        if (mailPopup.activeSelf)
        {
            AdjustBoardSize();
        }
    }

    public void OpenmailPopup()
    {
        mailPopup.SetActive(true);
        AdjustBoardSize();
    }

    public void ClosemailPopup()
    {
        mailPopup.SetActive(false);
    }

    private void AdjustBoardSize()
    {
        if (mailPopupRect != null)
        {
            float popupWidth = mailPopupRect.rect.width;

            // Adjust the board 1 width if the popup is smaller than the original board
            if (popupWidth < originalBoard1Size.x + 60)
            {
                board1.sizeDelta = new Vector2(popupWidth -60, originalBoard1Size.y);
                board1.GetComponent<Image>().preserveAspect = false; // Uncheck Preserve Aspect
            }
            else
            {
                board1.sizeDelta = originalBoard1Size;
                board1.GetComponent<Image>().preserveAspect = true; // Check Preserve Aspect
            }

            // Adjust the BowField width to 80% of the board width
            if (bowField != null)
            {
                float bowFieldWidth = board1.rect.width * 0.8f; // Calculate BowField width as 70% of Board width
                bowField.sizeDelta = new Vector2(bowFieldWidth, bowField.sizeDelta.y); // Adjust BowField width while keeping its height unchanged
                bowField.anchoredPosition = new Vector2(0, bowField.anchoredPosition.y); // Center BowField horizontally if needed
            }
  
            // Adjust the board 2 width if the popup is smaller than the original board
            if (popupWidth < originalBoard2Size.x + 60)
            {
                board2.sizeDelta = new Vector2(popupWidth -60, originalBoard2Size.y);
                board2.GetComponent<Image>().preserveAspect = false; // Uncheck Preserve Aspect
            }
            else
            {
                board2.sizeDelta = originalBoard2Size;
                board2.GetComponent<Image>().preserveAspect = true; // Check Preserve Aspect
            }
        }
    }

    // Load existing mail data (Placeholder logic)
    private void LoadMailItems()
    {
        string savedMails = PlayerPrefs.GetString("StoredMails", "");
        if (!string.IsNullOrEmpty(savedMails))
        {
            string[] storedMails = savedMails.Split('|');
            foreach (string mail in storedMails)
            {
                CreateMailItem(mail);
            }
        }

        FetchMailsFromServer(); // Simulate fetching mails
    }

    private void CreateMailItem(string mailTitle)
    {
        // Instantiate the mail item
        GameObject newMailItem = Instantiate(mailItemPrefab, contentTransform);

        // Ensure board is adjusted first
        AdjustBoardSize();

        // Get RectTransform components
        RectTransform mailItemRect = newMailItem.GetComponent<RectTransform>();
        RectTransform envelopeRect = newMailItem.transform.Find("Envelope").GetComponent<RectTransform>();
        RectTransform textRect = newMailItem.transform.Find("TitleText").GetComponent<RectTransform>();
        GameObject dotIndicator = newMailItem.transform.Find("DotIndicator").gameObject; // Find the dot

        // Calculate dynamic widths
        float mailItemWidth = ((RectTransform)contentTransform).rect.width;
        float envelopeWidth = mailItemWidth * 0.2f;
        float textWidth = mailItemWidth * 0.65f;

        // Apply calculated widths
        envelopeRect.sizeDelta = new Vector2(envelopeWidth, envelopeRect.sizeDelta.y);
        textRect.sizeDelta = new Vector2(textWidth, textRect.sizeDelta.y);

        // Find and update the title text inside the prefab
        TextMeshProUGUI titleText = textRect.GetComponent<TextMeshProUGUI>();

        // Enable auto-fit within the text box but keep font size fixed
        titleText.enableAutoSizing = false; // Keep font size fixed
        titleText.overflowMode = TextOverflowModes.Ellipsis; // Ensure ellipsis for overflow text
        titleText.text = mailTitle;

        // Check if mail was already read (PlayerPrefs for persistence)
        if (PlayerPrefs.GetInt(mailTitle, 0) == 1)
        {
            dotIndicator.SetActive(false); // Hide dot if mail is already read
        }
    
        // Attach click event for opening the mail
        newMailItem.GetComponent<Button>().onClick.AddListener(() => OpenMail(mailTitle, newMailItem, dotIndicator));

        // Adjust layout (force update)
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentTransform.GetComponent<RectTransform>());
    }

    private void FetchMailsFromServer()
    {
        // Placeholder for server logic
        // Example: Connect to API and get mail data
        
        // Retrieve stored mails
        string savedMails = PlayerPrefs.GetString("StoredMails", "");
        List<string> mailList = new List<string>(savedMails.Split('|'));

        // Remove any accidental empty entries
        mailList.RemoveAll(mail => string.IsNullOrWhiteSpace(mail));

        // Retrieve deleted mails
        string deletedMails = PlayerPrefs.GetString("DeletedMails", "");
        HashSet<string> deletedMailSet = new HashSet<string>(deletedMails.Split('|'));

        // Simulate received mails
        string[] fetchedMails = 
        {
            "Server Mail 1 - This is a long message to check ellipsis...",
            "Server Mail 2 - This is a long message to check ellipsis...",
            "Server Mail 3 - This is a long message to check ellipsis...",
            "Server Mail 4 - Another long message to check truncation...",
            "Server Mail 5 - Sample content for a long subject..."
        };

        foreach (string mail in fetchedMails)
        {
            if (!mailList.Contains(mail) && !deletedMailSet.Contains(mail))
            {
                mailList.Add(mail);
                CreateMailItem(mail);
            }
        }

        // Sort mails before saving to maintain consistent order
        mailList.Sort();

        // Save updated mail list (ensuring no empty entries)
        mailList.RemoveAll(mail => string.IsNullOrWhiteSpace(mail));
        PlayerPrefs.SetString("StoredMails", string.Join("|", mailList));
        PlayerPrefs.Save();
    }

    private void OpenMail(string mailTitle, GameObject mailItem, GameObject dotIndicator)
    {
        step1Panel.SetActive(false);
        step2Panel.SetActive(true);

        // Mark mail as read
        if (dotIndicator != null)
        {
            dotIndicator.SetActive(false);
        }
        PlayerPrefs.SetInt(mailTitle, 1); // Save read status
        PlayerPrefs.Save();
    
        // Track the currently opened mail item
        currentOpenMailItem = mailItem;
    }

    public void CloseMail()
    {
        // Switch panels back
        step2Panel.SetActive(false);
        step1Panel.SetActive(true);
    }

    private void DeleteCurrentMail()
    {
        if (currentOpenMailItem != null)
        {
            string mailTitle = currentOpenMailItem.transform.Find("TitleText").GetComponent<TextMeshProUGUI>().text;

            // Remove from PlayerPrefs
            RemoveMailFromStorage(mailTitle);

            // Destroy the mail item
            Destroy(currentOpenMailItem);
            currentOpenMailItem = null; 

            CloseMail();
        }
    }

    public void DeleteReadMails()
    {
        List<string> updatedMails = new List<string>();

        for (int i = contentTransform.childCount - 1; i >= 0; i--)
        {
            Transform mailItem = contentTransform.GetChild(i);
            GameObject dotIndicator = mailItem.Find("DotIndicator")?.gameObject;
            string mailTitle = mailItem.Find("TitleText").GetComponent<TextMeshProUGUI>().text;

            if (dotIndicator == null || !dotIndicator.activeSelf) // If read, delete
            {
                RemoveMailFromStorage(mailTitle);
                Destroy(mailItem.gameObject);
            }
            else
            {
                updatedMails.Add(mailTitle);
            }
        }

        // Save remaining mails back to PlayerPrefs
        PlayerPrefs.SetString("StoredMails", string.Join("|", updatedMails));
        PlayerPrefs.Save();
    }

    private void RemoveMailFromStorage(string mailTitle)
    {
        string savedMails = PlayerPrefs.GetString("StoredMails", "");
        List<string> mailList = new List<string>(savedMails.Split('|'));

        if (mailList.Contains(mailTitle))
        {
            mailList.Remove(mailTitle);
            PlayerPrefs.SetString("StoredMails", string.Join("|", mailList));
            PlayerPrefs.Save();
        }

        // Track deleted mails separately
        string deletedMails = PlayerPrefs.GetString("DeletedMails", "");
        List<string> deletedMailList = new List<string>(deletedMails.Split('|'));

        if (!deletedMailList.Contains(mailTitle))
        {
            deletedMailList.Add(mailTitle);
            PlayerPrefs.SetString("DeletedMails", string.Join("|", deletedMailList));
            PlayerPrefs.Save();
        }
    }
}