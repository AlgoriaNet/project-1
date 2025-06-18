using UnityEngine;
using TMPro;

public class GlobalPopup : MonoBehaviour
{
    public GameObject popupPanel;
    public TMP_Text messageText;

    public static GlobalPopup Instance;

    void Awake()
    {
        Instance = this;
        popupPanel.SetActive(false);
    }

    public void Show(string message)
    {
        popupPanel.SetActive(true);
        messageText.text = message;
    }

    public void Hide()
    {
        popupPanel.SetActive(false);
    }
}

