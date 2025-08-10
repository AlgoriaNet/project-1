using UnityEngine;
using model;
using battle;

namespace battle
{
    public class BlackHoleTester : MonoBehaviour
    {
        [Header("Test Settings")]
        public KeyCode testKey = KeyCode.B;
        public Transform spawnPoint;
        
        private Skill blackHoleSkill;
        private Living testLiving;
        
        void Start()
        {
            // Create test skill data
            blackHoleSkill = new Skill
            {
                Name = "Black_Hole",
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
            
            // Create test living entity
            testLiving = new Sidekick
            {
                Name = "BlackHoleCaster",
                Hp = 100,
                Atk = 60,
                Def = 10,
                Skill = blackHoleSkill
            };
            
            Debug.Log("[BlackHoleTester] Test setup complete. Press 'B' to test Black Hole skill!");
        }
        
        void Update()
        {
            if (Input.GetKeyDown(testKey))
            {
                TestBlackHole();
            }
        }
        
        private void TestBlackHole()
        {
            Vector3 spawnPosition = spawnPoint ? spawnPoint.position : transform.position;
            
            Debug.Log($"[BlackHoleTester] Testing Black Hole at {spawnPosition}");
            
            // Create black hole test
            GameObject skillObject = new GameObject("TestBlackHole");
            skillObject.transform.position = spawnPosition;
            
            // Add the black hole component
            var blackHole = skillObject.AddComponent<SkillBlackHole>();
            var skillSetting = skillObject.AddComponent<SkillSetting>();
            
            // Set up the skill setting
            skillSetting.Living = testLiving;
            skillSetting.Skill = blackHoleSkill;
            skillSetting.targetIndex = 0;
            skillSetting.targetDirection = Vector2.right;
            
            Debug.Log("[BlackHoleTester] 🌌 Black Hole test created successfully!");
            Debug.Log("[BlackHoleTester] Watch for the gravitational vortex effect!");
        }
        
        private void OnGUI()
        {
            GUI.Label(new Rect(10, 10, 300, 20), $"Press '{testKey}' to test Black Hole");
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