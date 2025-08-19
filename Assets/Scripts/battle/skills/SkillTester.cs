using UnityEngine;
using model;
using battle;
using System.Collections;

namespace battle
{
    public class SkillTester : MonoBehaviour
    {
        [Header("Test Settings")]
        public KeyCode blackHoleKey = KeyCode.B;
        public KeyCode blazingRayKey = KeyCode.R;
        public KeyCode megaLaserKey = KeyCode.G;
        public KeyCode bombBlastKey = KeyCode.Alpha3;
        public KeyCode darkTouchKey = KeyCode.K;
        public KeyCode fireballKey = KeyCode.F;
        public KeyCode fireTornadoKey = KeyCode.T;
        public KeyCode iceCrackBulletKey = KeyCode.C;
        public KeyCode iceSpearKey = KeyCode.D;
        public KeyCode iceSpike = KeyCode.I;
        public KeyCode infemoKey = KeyCode.E;
        public KeyCode ChainLightningKey = KeyCode.L;
        public KeyCode skill1Key = KeyCode.Alpha1;
        public KeyCode skill2Key = KeyCode.Alpha4;
        public KeyCode splitBulletKey = KeyCode.S;
        public KeyCode stormBladeKey = KeyCode.M;
        public KeyCode thunderBoltKey = KeyCode.Q;
        public KeyCode thunderPunishmentKey = KeyCode.P;
        public KeyCode tornadoKey = KeyCode.N;
        public KeyCode undeadSummoningKey = KeyCode.U;
        public KeyCode volcanicStormKey = KeyCode.Alpha2;
        public KeyCode windFeatherKey = KeyCode.W;
        public Transform spawnPoint;
        
        private Skill blackHoleSkill;
        private Skill chainLightningSkill;
        private Skill fireballSkill;
        private Skill volcanicStormSkill;
        private Skill bombBlastSkill;
        private Skill fireTornadoSkill;
        private Skill blazingRaySkill;
        private Skill megalaserSkill;
        private Skill iceSpikeSkill;
        private Skill iceCrackBulletSkill;
        private Skill thunderPunishmentSkill;
        private Skill tornadoSkill;
        private Skill splitBulletSkill;
        private Skill stormBladeSkill;
        private Skill undeadSummoningSkill;
        private Skill windFeatherSkill;
        private Skill iceSpearSkill;
        private Skill infemoSkill;
        private Skill thunderBoltSkill;
        private Skill darkTouchSkill;
        private Skill skill1;
        private Skill skill2;
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

            fireballSkill = new Skill
            {
                Name = "Skill_Fireball",
                Icon = "skill_icon_fire",
                Description = "Classic fireball projectile",
                Duration = 3f,
                Cd = 5f,
                DamageType = DamageType.Fire,
                SkillTargetType = SkillTargetType.Latest,
                DamageRatio = 1.2f,
                Speed = 15,
                ReleaseCount = 1,
                LaunchesCount = 1,
                IsDynamic = false,
                IsImpenetrability = true,
                Scope = 2f
            };

            volcanicStormSkill = new Skill
            {
                Name = "Skill_Volcanic_Storm",
                Icon = "skill_icon_fire",
                Description = "Volcanic storm with lightning",
                Duration = 1.5f,
                Cd = 6f,
                DamageType = DamageType.Fire,
                SkillTargetType = SkillTargetType.Latest,
                DamageRatio = 1.5f,
                Speed = 25,
                ReleaseCount = 1,
                LaunchesCount = 1,
                IsDynamic = false,
                IsImpenetrability = true
            };

            bombBlastSkill = new Skill
            {
                Name = "Skill_Bomb_Blast",
                Icon = "skill_icon_physical",
                Description = "Explosive bomb blast with shockwave damage",
                Duration = 3f,
                Cd = 7f,
                DamageType = DamageType.Fire,
                SkillTargetType = SkillTargetType.Latest,
                DamageRatio = 2.0f,
                Speed = 20,
                ReleaseCount = 1,
                LaunchesCount = 1,
                IsDynamic = false,
                IsImpenetrability = true
            };

            fireTornadoSkill = new Skill
            {
                Name = "Skill_SmallFireTornado",
                Icon = "skill_icon_fire",
                Description = "Small spinning tornado of flames",
                Duration = 3f,
                Cd = 10f,
                DamageType = DamageType.Fire,
                SkillTargetType = SkillTargetType.LatestMultiple,
                DamageRatio = 2.0f,
                Speed = 8,
                ReleaseCount = 1,
                LaunchesCount = 1,
                IsDynamic = true,
                IsImpenetrability = false
            };

            blazingRaySkill = new Skill
            {
                Name = "Skill_Blazing_Ray",
                Icon = "skill_icon_fire",
                Description = "Concentrated beam of fire",
                Duration = 2f,
                Cd = 8f,
                DamageType = DamageType.Fire,
                SkillTargetType = SkillTargetType.Latest,
                DamageRatio = 2.2f,
                Speed = 25,
                ReleaseCount = 1,
                LaunchesCount = 1,
                IsDynamic = false,
                IsImpenetrability = true
            };

            megalaserSkill = new Skill
            {
                Name = "Skill_Mega_Laser",
                Icon = "skill_icon_light",
                Description = "Massive laser beam attack",
                Duration = 2f,
                Cd = 8f,
                DamageType = DamageType.Light,
                SkillTargetType = SkillTargetType.Latest,
                DamageRatio = 2.2f,
                Speed = 25,
                ReleaseCount = 1,
                LaunchesCount = 1,
                IsDynamic = false,
                IsImpenetrability = true
            };

            iceSpikeSkill = new Skill
            {
                Name = "Skill_Ice_Spike",
                Icon = "skill_icon_ice",
                Description = "Sharp ice projectile",
                Duration = 3f,
                Cd = 6f,
                DamageType = DamageType.Ice,
                SkillTargetType = SkillTargetType.Latest,
                DamageRatio = 1.4f,
                Speed = 20,
                ReleaseCount = 1,
                LaunchesCount = 1,
                IsDynamic = false,
                IsImpenetrability = true
            };

            iceCrackBulletSkill = new Skill
            {
                Name = "Skill_IceCrackBullet",
                Icon = "skill_icon_ice",
                Description = "Ice bullet that cracks on impact",
                Duration = 2f,
                Cd = 5f,
                DamageType = DamageType.Ice,
                SkillTargetType = SkillTargetType.Latest,
                DamageRatio = 1.3f,
                Speed = 22,
                ReleaseCount = 1,
                LaunchesCount = 1,
                IsDynamic = false,
                IsImpenetrability = true
            };


            thunderPunishmentSkill = new Skill
            {
                Name = "Skill_Thunder_Punishment",
                Icon = "skill_icon_lightning",
                Description = "Divine thunder strike",
                Duration = 1.5f,
                Cd = 12f,
                DamageType = DamageType.Light,
                SkillTargetType = SkillTargetType.Latest,
                DamageRatio = 2.0f,
                Speed = 0,
                ReleaseCount = 1,
                LaunchesCount = 1,
                IsDynamic = false,
                IsImpenetrability = true,
                Scope = 3f
            };

            tornadoSkill = new Skill
            {
                Name = "Swift_Tornado",
                Icon = "skill_icon_wind",
                Description = "Powerful wind tornado",
                Duration = 4f,
                Cd = 9f,
                DamageType = DamageType.Wind,
                SkillTargetType = SkillTargetType.LatestMultiple,
                DamageRatio = 1.9f,
                Speed = 10,
                ReleaseCount = 1,
                LaunchesCount = 1,
                IsDynamic = true,
                IsImpenetrability = false
            };

            splitBulletSkill = new Skill
            {
                Name = "skill_split_bullet",
                Icon = "skill_icon_physical",
                Description = "Bullet that splits into multiple projectiles",
                Duration = 3f,
                Cd = 6f,
                DamageType = DamageType.Physics,
                SkillTargetType = SkillTargetType.LatestMultiple,
                DamageRatio = 1.1f,
                Speed = 18,
                ReleaseCount = 3,
                LaunchesCount = 1,
                IsDynamic = false,
                IsImpenetrability = true
            };

            stormBladeSkill = new Skill
            {
                Name = "Skill_Storm_Blade",
                Icon = "skill_icon_wind",
                Description = "Wind blade attack",
                Duration = 2f,
                Cd = 7f,
                DamageType = DamageType.Wind,
                SkillTargetType = SkillTargetType.Latest,
                DamageRatio = 1.7f,
                Speed = 25,
                ReleaseCount = 1,
                LaunchesCount = 1,
                IsDynamic = false,
                IsImpenetrability = false
            };

            undeadSummoningSkill = new Skill
            {
                Name = "Skill_Undead_Summoning",
                Icon = "skill_icon_dark",
                Description = "Summons undead allies",
                Duration = 10f,
                Cd = 20f,
                DamageType = DamageType.Darkly,
                SkillTargetType = SkillTargetType.LatestMultiple,
                DamageRatio = 1.0f,
                Speed = 0,
                ReleaseCount = 3,
                LaunchesCount = 1,
                IsDynamic = true,
                IsImpenetrability = false
            };

            windFeatherSkill = new Skill
            {
                Name = "Skill_Wind_Feather",
                Icon = "skill_icon_wind",
                Description = "Light wind feather projectiles",
                Duration = 3f,
                Cd = 5f,
                DamageType = DamageType.Wind,
                SkillTargetType = SkillTargetType.LatestMultiple,
                DamageRatio = 1.0f,
                Speed = 15,
                ReleaseCount = 5,
                LaunchesCount = 1,
                IsDynamic = false,
                IsImpenetrability = true
            };

            iceSpearSkill = new Skill
            {
                Name = "skill_Ice_Spear",
                Icon = "skill_icon_physical",
                Description = "Heavy impact attack",
                Duration = 1.5f,
                Cd = 8f,
                DamageType = DamageType.Physics,
                SkillTargetType = SkillTargetType.Latest,
                DamageRatio = 2.3f,
                Speed = 12,
                ReleaseCount = 1,
                LaunchesCount = 1,
                IsDynamic = false,
                IsImpenetrability = true
            };

            infemoSkill = new Skill
            {
                Name = "Skill_Infemo",
                Icon = "skill_icon_fire",
                Description = "Infernal flame attack",
                Duration = 4f,
                Cd = 11f,
                DamageType = DamageType.Fire,
                SkillTargetType = SkillTargetType.LatestMultiple,
                DamageRatio = 2.1f,
                Speed = 8,
                ReleaseCount = 1,
                LaunchesCount = 1,
                IsDynamic = true,
                IsImpenetrability = false
            };

            thunderBoltSkill = new Skill
            {
                Name = "Skill_Thunder_Bolt",
                Icon = "skill_icon_light",
                Description = "Thunder bolt lightning attack",
                Duration = 3f,
                Cd = 9f,
                DamageType = DamageType.Light,
                SkillTargetType = SkillTargetType.Latest,
                DamageRatio = 1.9f,
                Speed = 20,
                ReleaseCount = 1,
                LaunchesCount = 1,
                IsDynamic = false,
                IsImpenetrability = true
            };

            darkTouchSkill = new Skill
            {
                Name = "Skill_Dark_Touch",
                Icon = "skill_icon_dark",
                Description = "Dark energy touch",
                Duration = 1f,
                Cd = 3f,
                DamageType = DamageType.Darkly,
                SkillTargetType = SkillTargetType.Latest,
                DamageRatio = 1.6f,
                Speed = 0,
                ReleaseCount = 1,
                LaunchesCount = 1,
                IsDynamic = false,
                IsImpenetrability = true
            };

            skill1 = new Skill
            {
                Name = "Skill1",
                Icon = "skill_icon_basic",
                Description = "Basic skill 1",
                Duration = 2f,
                Cd = 3f,
                DamageType = DamageType.Fire,
                SkillTargetType = SkillTargetType.Latest,
                DamageRatio = 1.0f,
                Speed = 15,
                ReleaseCount = 1,
                LaunchesCount = 1,
                IsDynamic = false,
                IsImpenetrability = true
            };

            skill2 = new Skill
            {
                Name = "Skill2",
                Icon = "skill_icon_basic",
                Description = "Basic skill 2",
                Duration = 2f,
                Cd = 3f,
                DamageType = DamageType.Ice,
                SkillTargetType = SkillTargetType.Latest,
                DamageRatio = 1.0f,
                Speed = 15,
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
            if (Input.GetKeyDown(blackHoleKey)) TestSkill("Black_Hole", blackHoleSkill);
            if (Input.GetKeyDown(ChainLightningKey)) TestSkill("Chain_Lightning", chainLightningSkill);
            if (Input.GetKeyDown(fireballKey)) TestSkill("Fireball", fireballSkill);
            if (Input.GetKeyDown(volcanicStormKey)) TestSkillDirect("skills/Skill_Volcanic_Storm", volcanicStormSkill);
            if (Input.GetKeyDown(bombBlastKey)) TestSkill("Bomb_Blast", bombBlastSkill);
            if (Input.GetKeyDown(fireTornadoKey)) TestSkill("SmallFireTornado", fireTornadoSkill);
            if (Input.GetKeyDown(blazingRayKey)) TestSkill("Blazing_Ray", blazingRaySkill);
            if (Input.GetKeyDown(megaLaserKey)) TestSkillDirect("skills/Skill_Mega_Laser", megalaserSkill);
            if (Input.GetKeyDown(iceSpike)) TestSkill("Ice_Spike", iceSpikeSkill);
            if (Input.GetKeyDown(iceCrackBulletKey)) TestSkill("IceCrackBullet", iceCrackBulletSkill);
            if (Input.GetKeyDown(thunderPunishmentKey)) TestSkill("Thunder_Punishment", thunderPunishmentSkill);
            if (Input.GetKeyDown(tornadoKey)) TestSkill("FireTornado", tornadoSkill);
            if (Input.GetKeyDown(splitBulletKey)) TestSkillDirect("skill_split_bullet", splitBulletSkill);
            if (Input.GetKeyDown(stormBladeKey)) TestSkill("Storm_Blade", stormBladeSkill);
            if (Input.GetKeyDown(undeadSummoningKey)) TestSkill("Undead_Summoning", undeadSummoningSkill);
            if (Input.GetKeyDown(windFeatherKey)) TestSkill("Wind_Feather", windFeatherSkill);
            if (Input.GetKeyDown(iceSpearKey)) TestSkill("Ice_Spear", iceSpearSkill);
            if (Input.GetKeyDown(infemoKey)) TestSkill("Inferno", infemoSkill);
            if (Input.GetKeyDown(thunderBoltKey)) TestSkill("Thunder_Bolt", thunderBoltSkill); 
            if (Input.GetKeyDown(darkTouchKey)) TestSkill("Dark_Touch", darkTouchSkill);
            if (Input.GetKeyDown(skill1Key)) TestSkill("1", skill1);
            if (Input.GetKeyDown(skill2Key)) TestSkill("2", skill2);
        }
        
        private void TestSkill(string skillPrefabName, Skill skill)
        {
            Vector3 spawnPosition = spawnPoint ? spawnPoint.position : transform.position;            
          
            var skillObject = SkillFactory.Create(
                skillPrefabName, 
                testLiving, 
                skill, 
                0, 
                Vector2.right, 
                spawnPosition
            );

            if (skillObject != null)
            {
                Debug.Log($"Testing skill: {skill.Name} - {skillPrefabName}");
                
                // Auto-destroy infinite loop skills after a reasonable time
                if (skillPrefabName == "Thunder_Bolt" || skillPrefabName == "Ice_Spear" || skillPrefabName == "2")
                {
                    StartCoroutine(DestroySkillAfterDelay(skillObject, 2f));
                }
            }
            else
            {
                Debug.LogWarning($"Skill prefab not found: {skillPrefabName}");
            }
        }

        private void TestSkillDirect(string fullPrefabPath, Skill skill)
        {
            Vector3 spawnPosition = spawnPoint ? spawnPoint.position : transform.position;
            
            var skillPrefab = Resources.Load<GameObject>(fullPrefabPath);
            if (skillPrefab)
            {
                GameObject skillObject = Object.Instantiate(skillPrefab);
                skillObject.transform.position = spawnPosition;
                SkillSetting setting = skillObject.GetComponent<SkillSetting>();
                if (setting)
                {
                    setting.Living = testLiving;
                    setting.Skill = skill;
                    setting.targetIndex = 0;
                    setting.targetDirection = Vector2.right;
                }
                
                Debug.Log($"Testing skill direct: {skill.Name} - {fullPrefabPath}");
                
                // Auto-destroy problematic infinite loop skills
                if (fullPrefabPath.Contains("Thunder_Bolt") || fullPrefabPath.Contains("Ice_Spear"))
                {
                    Object.Destroy(skillObject, 2f);
                }
            }
            else
            {
                Debug.LogWarning($"Skill prefab not found: {fullPrefabPath}");
            }
        }

        private System.Collections.IEnumerator DestroySkillAfterDelay(GameObject skillObject, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (skillObject != null)
            {
                Object.Destroy(skillObject);
                Debug.Log($"Auto-destroyed skill after {delay} seconds");
            }
        }
        
        private void OnGUI()
        {
            GUI.Label(new Rect(10, 10, 400, 20), "SKILL TESTING - Available Keys:");
            GUI.Label(new Rect(10, 30, 400, 20), $"1=Skill1, 4=Skill2, B=BlackHole, L=Lightning, F=Fireball");
            GUI.Label(new Rect(10, 50, 400, 20), $"2=VolcanicStorm, 3=BombBlast, T=FireTornado, R=BlazingRay, G=MegaLaser, I=IceSpike");
            GUI.Label(new Rect(10, 70, 400, 20), $"C=IceCrack, P=Thunder, N=SwiftTornado, S=SplitBullet");
            GUI.Label(new Rect(10, 90, 400, 20), $"M=StormBlade, U=Undead, W=WindFeather, D=IceSpear");
            GUI.Label(new Rect(10, 110, 400, 20), $"E=Inferno, Q=ThunderBolt, K=DarkTouch");
            GUI.Label(new Rect(10, 130, 400, 20), $"Monsters in scene: {FindObjectsOfType<MonsterManager>().Length}");
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