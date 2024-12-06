using entity;
using UnityEngine;
using UnityEngine.Serialization;

namespace battle
{
    public class BaseSkillController : MonoBehaviour
    {
        public AudioSource releasingAudio;
        public AudioSource releaseAudio;
        public AudioSource hitAudio;
        [FormerlySerializedAs("IsDestroyAfterDuration")] 
        public bool isDestroyAfterDuration = true;

        protected Living Living;
        protected SkillSetting SkillSetting;
        protected Vector2 TargetDirection;
        protected int TargetIndex;
        protected Skill Skill;
        protected int Stage = 1;


        protected virtual void Start()
        {
            SetSkillSetting(GetComponent<SkillSetting>());
            Init();
            if(isDestroyAfterDuration) Destroy(gameObject, Skill.Duration);
        }

        protected void Init()
        {
            gameObject.transform.localScale *= Skill.Scope;

            if (releaseAudio) releaseAudio.Play();
            if (releasingAudio) releasingAudio.PlayDelayed(releaseAudio ? releaseAudio.time : 0);
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

        // ReSharper disable Unity.PerformanceAnalysis
        protected void Attack(MonsterManager monster)
        {
            if (hitAudio && !hitAudio.isPlaying)
            {
                if (releaseAudio) releaseAudio.Stop();
                if (releasingAudio) releasingAudio.Stop();
                hitAudio.Play();
            }

            var damageRatio = Skill.DamageRatio;
            if (Stage == 2) damageRatio = Skill.TwoStageDamageRatio;
            if (Stage == 3) damageRatio = Skill.ThreeStageDamageRatio;

            var ultimatelyDamageRatio = damageRatio * (1 + Skill.ExtraDamageGain);
            var result = Living.Attack(Skill.DamageType, monster.Monster, ultimatelyDamageRatio);
            monster.BeHarmed(result);
            monster.CheckDead();
            var buffs = Skill.ActiveCharacter.FindAll(character => character.Contains("ADD_BUFF_"));
            buffs.ForEach(buff => AppendBuff(monster.Monster, buff));
            WhenAttackAfter();
        }

        protected virtual MonsterManager GetTarget()
        {
            return BattleGridManager.Instance.GetTarget(Skill.SkillTargetType, TargetIndex);
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