using battle;
using UnityEngine;
using utils;

namespace battle
{
    public class SkillIceSpear : BaseSkillController
    {
        private Rigidbody2D _spearRigidbody;
        private bool hasHit = false;
        
        public override void Start()
        {
            base.Start();
            transform.rotation = Utils.DirectionQuaternion(TargetDirection, Vector2.left);
            _spearRigidbody = GetComponent<Rigidbody2D>();
            if (_spearRigidbody)
            {
                _spearRigidbody.velocity = TargetDirection * Skill.Speed;
            }
            
            // Ensure skill is destroyed after duration even if no collision
            Destroy(gameObject, Skill.Duration);
        }
        
        protected override void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Monster") && !hasHit)
            {
                hasHit = true;
                Attack(other.GetComponent<MonsterManager>());
                // Stop movement after hitting
                if (_spearRigidbody)
                {
                    _spearRigidbody.velocity = Vector2.zero;
                }
                // Destroy after hit with proper delay
                Destroy(gameObject, Skill.DestroyDelay);
            }
        }
        
        protected override void WhenAttackAfter()
        {
            // Ice spear effect after attack (could add freeze effect here)
        }
    }
}