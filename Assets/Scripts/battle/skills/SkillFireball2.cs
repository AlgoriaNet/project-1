using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace battle
{
    public class SkillFireball2 : BaseSkillController
    {
        [FormerlySerializedAs("ParticleSystems")] public ParticleSystem[] particleSystems;
        public GameObject fireball;
        public GameObject blast;

        public override void Start()
        {
            base.Start();
            transform.rotation = Quaternion.LookRotation(TargetDirection);
            foreach (var ps in particleSystems)
            {
                ParticleSystem.MainModule main = ps.main;
                main.startSpeed = Skill.Speed;
                ps.Emit(1);
            }
        }
        protected override void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Monster"))
            {
                var monsterManager = other.GetComponent<MonsterManager>();
                Attack(monsterManager);
                if (Skill.IsImpenetrability)
                {
                    Skill.IsDynamic = false;
                    Destroy(gameObject, Skill.DestroyDelay);
                }
                fireball.SetActive(false);
                blast.SetActive(true);
            }
        }
    }
}