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
          
            // Use SkillFactory to create the black hole from prefab (production system)
            var blackHoleObject = SkillFactory.Create(
                "Black_Hole", 
                testLiving, 
                blackHoleSkill, 
                0, 
                Vector2.right, 
                spawnPosition
            );
        }
        
        private void TestChainLightning()
        {
            Vector3 spawnPosition = spawnPoint ? spawnPoint.position : transform.position;
            
            // Use SkillFactory to create the chain lightning from prefab
            var lightningObject = SkillFactory.Create(
                "Chain_Lightning", 
                testLiving, 
                chainLightningSkill, 
                0, 
                Vector2.right, 
                spawnPosition
            );            
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