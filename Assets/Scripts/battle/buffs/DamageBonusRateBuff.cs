using model;
using UnityEngine;

namespace battle
{
    public class DamageBonusRateBuff : Buff
    {
        public DamageBonusRateBuff(float duration, int value) : base("DamageBonusRate", duration, value)
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