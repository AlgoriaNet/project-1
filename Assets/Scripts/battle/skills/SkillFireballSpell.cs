using UnityEngine;

namespace battle
{
    public class SkillFireballSpell : BaseSkillController
    {
        public GameObject fireball;
        public GameObject blast;
        
        protected override void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Monster"))
            {
                var monsterManager = other.GetComponent<MonsterManager>();
                Attack(monsterManager);
                if (Living.Skill.IsImpenetrability)
                {
                    SkillSetting.IsDynamic = false;
                    Destroy(gameObject, Living.Skill.DestroyDelay);
                }
                fireball.SetActive(false);
                if(Skill.HasCharacter("blast")) blast.SetActive(true);
            }
        }
    }
}