using UnityEngine;

public class BattleStarter : MonoBehaviour
{
    public Canvas mainCanvas;
    public GameObject battleObject;

    public void StartBattle()
    {
        if (mainCanvas != null) mainCanvas.enabled = false;
        if (battleObject != null) battleObject.SetActive(true);
        Debug.Log("Battle initiated. Main canvas hidden.");
    }
}