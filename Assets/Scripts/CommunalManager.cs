using UnityEngine;

public class CommunalManager : MonoBehaviour
{
    public static CommunalManager Instance;
    public GameObject loadingPage;
    public GameObject reConnectPage;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }
    
}
