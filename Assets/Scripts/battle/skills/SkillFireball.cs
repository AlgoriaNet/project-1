using UnityEngine;

namespace battle
{
    public class SkillFireball : BaseSkillController
    {
        public GameObject fireball;
        public GameObject blast;

        protected override void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Monster"))
            {
                var monsterManager = other.GetComponent<MonsterManager>();
                Attack(monsterManager);
                if (Skill.IsImpenetrability)
                {
                    // Skill.IsDynamic = false;
                    Destroy(gameObject, Skill.DestroyDelay);
                }
                // fireball.SetActive(false);
                // blast.SetActive(true);
            }
        }
    }
}