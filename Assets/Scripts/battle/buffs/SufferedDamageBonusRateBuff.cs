using entity;
using UnityEngine;

namespace battle
{
    public class SufferedDamageBonusRateBuff : Buff
    {
        public SufferedDamageBonusRateBuff(string name, float duration, int value) : base(name, duration, value)
        {
        }

        protected override void OnEnable(Living target, GameObject player)
        {
            target.SufferedDamageBonusRate += Value;
        }

        protected override void OnDisable(Living target, GameObject player)
        {
            target.SufferedDamageBonusRate -= Value;
        }
    }
}