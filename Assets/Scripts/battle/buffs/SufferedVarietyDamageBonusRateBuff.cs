using entity;
using UnityEngine;

namespace battle
{
    public class SufferedVarietyDamageBonusRateBuff : Buff
    {
        private readonly DamageType _damageType;

        public SufferedVarietyDamageBonusRateBuff(string name, float duration, DamageType damageType, int value)
            : base(name, duration, value)
        {
            _damageType = damageType;
        }

        protected override void OnEnable(Living target, GameObject player)
        {
            if (!target.SufferedVarietyDamageBonusRate.TryAdd(_damageType, Value))
                target.SufferedVarietyDamageBonusRate[_damageType] += Value;
        }

        protected override void OnDisable(Living target, GameObject player)
        {
            if (!target.SufferedVarietyDamageBonusRate.TryAdd(_damageType, Value))
                target.SufferedVarietyDamageBonusRate[_damageType] -= Value;
        }
    }
}