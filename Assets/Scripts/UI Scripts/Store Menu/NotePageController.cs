using UnityEngine;

public class NotePageController : MonoBehaviour
{
    public GameObject notePage;

    public void ToggleNotePage()
    {
        notePage.SetActive(!notePage.activeSelf);
    }

    public void CloseNotePage()
    {
        notePage.SetActive(false);
    }
}
