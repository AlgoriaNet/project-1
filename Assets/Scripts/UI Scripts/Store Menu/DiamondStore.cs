using UnityEngine;

public class DiamondStore : MonoBehaviour
{
    public GameObject diamondStore;

    public void Open()
    {
        diamondStore.SetActive(true);
    }

    public void Close()
    {
        diamondStore.SetActive(false);
    }
}