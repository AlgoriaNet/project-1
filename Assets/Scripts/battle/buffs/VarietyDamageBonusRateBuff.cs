using entity;
using UnityEngine;

namespace battle
{
    public class VarietyDamageBonusBuffRate : Buff
    {
        private readonly DamageType _damageType;

        public VarietyDamageBonusBuffRate(string name, float duration, DamageType damageType, int value)
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