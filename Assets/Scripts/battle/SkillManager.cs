using entity;
using Unity.VisualScripting;
using UnityEngine;

namespace battle
{
    public class SkillManager : MonoBehaviour
    {
        public Sidekick Sidekick;
        public float timedata;

        private void Update()
        {
            if (Sidekick == null) return;
            timedata += Time.deltaTime;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (Sidekick.Skill.IsDurationDamage) return;
            if (other.gameObject.CompareTag("Monster"))
            {
                var monsterManager = other.GetComponent<MonsterManager>();
                var result = Sidekick.AttackDamage(Sidekick.Skill.DamageType, monsterManager.monster);
                monsterManager.BeHarmed(result);
                monsterManager.CheckDead();
                if (Sidekick.Skill.AdditionalBuff != null)
                {
                    monsterManager.monster.BuffManager.AddBuff(Sidekick.Skill.AdditionalBuff.ShallowCopy());
                }
            }
        }
    }
}