using UnityEngine;
using model;
using battle;

public class SimpleSidekickTest : MonoBehaviour
{
    public GameObject sidekickPrefab;
    
    private void Start()
    {
        Debug.Log("[SimpleSidekickTest] Starting sidekick sprite test...");
        
        if (sidekickPrefab == null)
        {
            Debug.LogError("[SimpleSidekickTest] sidekickPrefab is null! Assign it in the inspector.");
            return;
        }
        
        // Create a test sidekick at origin
        GameObject sidekickObj = Instantiate(sidekickPrefab, Vector3.zero, Quaternion.identity);
        var sidekickManager = sidekickObj.GetComponent<SidekickManager>();
        
        if (sidekickManager == null)
        {
            Debug.LogError("[SimpleSidekickTest] No SidekickManager found on prefab!");
            return;
        }
        
        // Create test sidekick data - using Zorath with ID 1
        var testSidekick = new Sidekick
        {
            Id = 1,
            Name = "Zorath",
            Hp = 100,
            MaxHp = 100,
            Atk = 50,
            Skill = new Skill
            {
                Name = "TestSkill",
                Cd = 5
            }
        };
        
        Debug.Log($"[SimpleSidekickTest] Testing sprite loading for {testSidekick.Name}, ID: {testSidekick.Id}");
        Debug.Log($"[SimpleSidekickTest] Expected path: Sidekicks/Back/B_01_Zorath/B_01_1");
        
        // Call Init - this should load the sprite and show our debug logs
        sidekickManager.Init(testSidekick);
        
        Debug.Log("[SimpleSidekickTest] Init called - check console for SidekickManager logs");
    }
}