using battle;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using model;

namespace battle
{
    public class SkillLightningStorm : BaseSkillController
    {
        private List<MonsterManager> affectedEnemies = new List<MonsterManager>();
        private float damageRange = 4f; // Large range for storm effect
        private bool hasDealtDamage = false;
        
        protected override void Init()
        {
            // Start damage detection coroutine
            StartCoroutine(DamageDetectionCoroutine());
            
            // Destroy after duration
            Destroy(gameObject, Skill.Duration);
        }
        
        private IEnumerator DamageDetectionCoroutine()
        {
            float elapsed = 0f;
            
            while (elapsed < Skill.Duration && !hasDealtDamage)
            {
                // Check for monsters in range
                UpdateAffectedEnemies();
                
                // Apply damage if any monsters are found
                if (affectedEnemies.Count > 0)
                {
                    ApplyLightningStormDamage();
                    hasDealtDamage = true;
                    break; // Lightning storm hits once with big impact
                }
                
                elapsed += Time.deltaTime;
                yield return null;
            }
        }
        
        private void UpdateAffectedEnemies()
        {
            affectedEnemies.Clear();
            var allMonsters = FindObjectsOfType<MonsterManager>();
            
            foreach (var monster in allMonsters)
            {
                if (monster != null && !monster.isDead)
                {
                    float distance = Vector3.Distance(monster.transform.position, transform.position);
                    if (distance <= damageRange)
                    {
                        affectedEnemies.Add(monster);
                    }
                }
            }
        }
        
        private void ApplyLightningStormDamage()
        {
            foreach (var enemy in affectedEnemies)
            {
                if (enemy != null && !enemy.isDead)
                {
                    // Apply lightning storm damage
                    var result = Living.Attack(Skill.DamageType, enemy.Monster, Skill.DamageRatio);
                    enemy.BeHarmed(result);
                    enemy.CheckDead();
                    
                    // Apply any buffs from skill
                    var buffs = Skill.ActiveCharacter.FindAll(character => character.Contains("ADD_BUFF_"));
                    buffs.ForEach(buff => AppendBuff(enemy.Monster, buff));
                }
            }
        }
        
        protected override void WhenAttackAfter()
        {
            // Lightning storm effect after attack
        }
    }
}