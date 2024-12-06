using entity;
using UnityEngine;

namespace battle
{
    public class VarietyDamageBonusRateBuff : Buff
    {
        private readonly DamageType _damageType;

        
        public VarietyDamageBonusRateBuff(float duration, DamageType damageType, int value)
            : base("VarietyDamageBonusBuff", duration, value)
        {
            _damageType = damageType;
        }
        
        public VarietyDamageBonusRateBuff(string name, float duration, DamageType damageType, int value)
            : base(name, duration, value)
        {
            _damageType = damageType;
        }

        protected override void OnEnable(Living target, GameObject player)
        {
            if (!target.VarietyDamageBonusRate.TryAdd(_damageType, Value))
                target.VarietyDamageBonusRate[_damageType] += Value;
        }

        protected override void OnDisable(Living target, GameObject player)
        {
            if (!target.VarietyDamageBonusRate.TryAdd(_damageType, Value))
                target.VarietyDamageBonusRate[_damageType] -= Value;
        }
    }
}