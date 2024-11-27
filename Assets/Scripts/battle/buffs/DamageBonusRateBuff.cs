using entity;
using UnityEngine;

namespace battle
{
    public class DamageBonusRateBuff : Buff
    {
        public DamageBonusRateBuff(string name, float duration, int value) : base(name, duration, value)
        {
        }

        protected override void OnEnable(Living target, GameObject player)
        { 
            target.DamageBonusRate += Value;
        }

        protected override void OnDisable(Living target, GameObject player)
        {
            target.DamageBonusRate -= Value;
        }
    }
}