using entity;
using UnityEngine;

namespace battle
{
    public abstract class BaseSkillController : MonoBehaviour
    {
        protected Living Living;
        protected SkillSetting SkillSetting;
        protected Vector2 TargetDirection;
        protected int TargetIndex;
        protected Skill Skill;


        protected virtual void Start()
        {
            SetSkillSetting(GetComponent<SkillSetting>());
            Init();
        }

        protected void Init()
        {
            Debug.Log("Skill Scope:" + Skill.Scope);
            gameObject.transform.localScale *= Skill.Scope;
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Monster"))
            {
                Attack(other.GetComponent<MonsterManager>());
                if (Living.Skill.IsImpenetrability)
                    Destroy(gameObject, Living.Skill.DestroyDelay);
            }
        }

        protected virtual void SetSkillSetting(SkillSetting skillSetting)
        {
            SkillSetting = skillSetting;
            Living = skillSetting.Living;
            TargetIndex = skillSetting.targetIndex;
            Skill = Living.Skill;
            TargetDirection = skillSetting.targetDirection;
        }

        protected void Attack(MonsterManager monster)
        {
            var damageRatio = Skill.DamageRatio * (1 + Skill.ExtraDamageGain);
            var result = Living.Attack(Skill.DamageType, monster.monster, damageRatio);
            monster.BeHarmed(result);
            monster.CheckDead();
            var buffs = Skill.ActiveCharacter.FindAll(character => character.Contains("ADD_BUFF_"));
            buffs.ForEach(buff => AppendBuff(monster.monster, buff));
            WhenAttackAfter();
        }

        protected virtual MonsterManager GetTarget()
        {
            return BattleGridManager.Instance.GetTarget(Skill.TargetType, TargetIndex);
        }

        protected virtual void WhenAttackAfter()
        {
        }

        protected virtual void AppendBuff(Monster monster, string buffCharacter)
        {
            var buff = BuffFactory.CreateBuff(buffCharacter);
            monster.BuffManager.AddBuff(buff);
        }
    }
}