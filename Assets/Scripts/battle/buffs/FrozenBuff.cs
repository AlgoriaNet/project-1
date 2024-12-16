using model;
using UnityEngine;

namespace battle
{
    public class FrozenBuff : Buff
    {
        public FrozenBuff(float duration) : base("Frozen", duration, 0)
        {
            
        }
        
        protected override void OnEnable(Living target, GameObject player)
        {
            target.IsFrozen = true;
        }

        protected override void OnDisable(Living target, GameObject player)
        {
            target.IsFrozen = false;
        }
    }
}