using UnityEngine;
using model;
using battle;

namespace battle
{
    public class SkillTester : MonoBehaviour
    {
        [Header("Test Settings")]
        public KeyCode blackHoleKey = KeyCode.B;
        public KeyCode lightningKey = KeyCode.L;
        public Transform spawnPoint;
        
        private Skill blackHoleSkill;
        private Skill chainLightningSkill;
        private Living testLiving;
        
        void Start()
        {
            // Create test skill data
            blackHoleSkill = new Skill
            {
                Name = "Skill_Black_Hole",
                Icon = "skill_icon_dark", 
                Description = "Gravitational vortex that pulls enemies to their doom",
                Duration = 4.5f,
                Cd = 12f,
                DamageType = DamageType.Darkly,
                SkillTargetType = SkillTargetType.LatestMultiple,
                DamageRatio = 1.8f,
                Speed = 0,
                ReleaseCount = 1,
                LaunchesCount = 1,
                IsDynamic = true,
                IsImpenetrability = false
            };
            
            // Create Chain Lightning skill data
            chainLightningSkill = new Skill
            {
                Name = "Skill_Chain_Lightning",
                Icon = "skill_icon_lightning", 
                Description = "Lightning that chains between enemies",
                Duration = 2f,
                Cd = 8f,
                DamageType = DamageType.Light,
                SkillTargetType = SkillTargetType.LatestMultiple,
                DamageRatio = 1.5f,
                Speed = 0,
                ReleaseCount = 1,
                LaunchesCount = 1,
                IsDynamic = false,
                IsImpenetrability = true
            };
            
            // Create test living entity
            testLiving = new Sidekick
            {
                Name = "SkillTester",
                Hp = 100,
                Atk = 60,
                Def = 10,
                Skill = blackHoleSkill
            };
            
            Debug.Log("[SkillTester] Test setup complete. Press 'B' for Black Hole, 'L' for Chain Lightning!");
        }
        
        void Update()
        {
            if (Input.GetKeyDown(blackHoleKey))
            {
                TestBlackHole();
            }
            
            if (Input.GetKeyDown(lightningKey))
            {
                TestChainLightning();
            }
        }
        
        private void TestBlackHole()
        {
            Vector3 spawnPosition = spawnPoint ? spawnPoint.position : transform.position;
            
            Debug.Log($"[SkillTester] Testing Black Hole at {spawnPosition}");
            
            // Use SkillFactory to create the black hole from prefab (production system)
            var blackHoleObject = SkillFactory.Create(
                "Black_Hole", 
                testLiving, 
                blackHoleSkill, 
                0, 
                Vector2.right, 
                spawnPosition
            );
            
            if (blackHoleObject)
            {
                Debug.Log("[SkillTester] 🌌 Black Hole test created successfully!");
                Debug.Log("[SkillTester] Watch for the gravitational vortex effect!");
            }
            else
            {
                Debug.LogError("[SkillTester] Failed to create Black Hole - check if prefab exists!");
            }
        }
        
        private void TestChainLightning()
        {
            Vector3 spawnPosition = spawnPoint ? spawnPoint.position : transform.position;
            
            Debug.Log($"[SkillTester] Testing Chain Lightning at {spawnPosition}");
            
            // Use SkillFactory to create the chain lightning from prefab
            var lightningObject = SkillFactory.Create(
                "Chain_Lightning", 
                testLiving, 
                chainLightningSkill, 
                0, 
                Vector2.right, 
                spawnPosition
            );
            
            if (lightningObject)
            {
                Debug.Log("[SkillTester] ⚡ Chain Lightning test created successfully!");
                Debug.Log("[SkillTester] Watch for the chaining lightning effect!");
            }
            else
            {
                Debug.LogError("[SkillTester] Failed to create Chain Lightning - check if prefab exists!");
            }
        }
        
        private void OnGUI()
        {
            GUI.Label(new Rect(10, 10, 300, 20), $"Press '{blackHoleKey}' for Black Hole, '{lightningKey}' for Chain Lightning");
            GUI.Label(new Rect(10, 30, 300, 20), $"Monsters in scene: {FindObjectsOfType<MonsterManager>().Length}");
        }
        
        private void OnDrawGizmos()
        {
            // Draw spawn point
            if (spawnPoint)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
                Gizmos.color = new Color(0.5f, 0f, 0.5f, 1f); // Purple color
                Gizmos.DrawWireSphere(spawnPoint.position, 7f); // Pull range visualization
            }
            else
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(transform.position, 0.5f);
                Gizmos.color = new Color(0.5f, 0f, 0.5f, 1f); // Purple color
                Gizmos.DrawWireSphere(transform.position, 7f); // Pull range visualization
            }
        }
    }
}