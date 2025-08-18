using battle;
using UnityEngine;
using utils;
using System.Collections;
using System.Collections.Generic;
using model;

namespace battle
{
    public class SkillIceSpear : BaseSkillController
    {
        private List<MonsterManager> affectedEnemies = new List<MonsterManager>();
        private float damageRange = 2f; // Range for damage detection
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
                    ApplyIceSpearDamage();
                    hasDealtDamage = true;
                    break; // Ice spear hits once
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
        
        private void ApplyIceSpearDamage()
        {
            foreach (var enemy in affectedEnemies)
            {
                if (enemy != null && !enemy.isDead)
                {
                    // Apply full damage (ice spear is a projectile that hits hard)
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
            // Ice spear effect after attack 
        }
    }
}