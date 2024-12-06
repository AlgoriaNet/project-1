using entity;
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
            if (player.gameObject.CompareTag("Monster"))
            {
                var monsterManager = player.GetComponent<MonsterManager>();
                // monsterManager._animationController.timeScale = 0;
                monsterManager.ice.SetActive(true);
            }
        }

        protected override void OnDisable(Living target, GameObject player)
        {
            target.IsFrozen = false;
            if (player.gameObject.CompareTag("Monster"))
            {
                var monsterManager = player.GetComponent<MonsterManager>();
                // monsterManager._animationController.timeScale = 1;
                monsterManager.ice.SetActive(false);
            }
        }
    }
}