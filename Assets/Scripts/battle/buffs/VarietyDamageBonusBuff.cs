using entity;
using UnityEngine;

namespace battle
{
    public class VarietyDamageBonusBuff : Buff
    {
        private readonly DamageType _damageType;

        public VarietyDamageBonusBuff(string name, float duration, DamageType damageType, int value)
            : base(name, duration, value)
        {
            _damageType = damageType;
        }

        protected override void OnEnable(Living target, GameObject player)
        {
            if (!target.VarietyDamageBonus.TryAdd(_damageType, Value))
                target.VarietyDamageBonus[_damageType] += Value;
        }

        protected override void OnDisable(Living target, GameObject player)
        {
            if (!target.VarietyDamageBonus.TryAdd(_damageType, Value))
                target.VarietyDamageBonus[_damageType] -= Value;
        }
    }
}