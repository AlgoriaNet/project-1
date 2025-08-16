using UnityEngine;
using model;
using battle;

public class SidekickTester : MonoBehaviour
{
    private void Start()
    {
        // Find the SidekickManager component
        var sidekickManager = FindObjectOfType<SidekickManager>();
        if (sidekickManager != null)
        {
            Debug.Log("[SidekickTester] Found SidekickManager, creating test sidekick...");
            
            // Create test sidekick data
            var testSidekick = new Sidekick
            {
                Id = 1,
                Name = "Zorath",
                Hp = 100,
                Atk = 50,
                Skill = new Skill
                {
                    Name = "TestSkill",
                    Cd = 5
                }
            };
            
            Debug.Log($"[SidekickTester] Calling Init with sidekick: {testSidekick.Name}, ID: {testSidekick.Id}");
            sidekickManager.Init(testSidekick);
        }
        else
        {
            Debug.LogError("[SidekickTester] No SidekickManager found!");
        }
    }
}