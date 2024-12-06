using entity;
using UnityEngine;

namespace battle
{
    public class BurnBuff : Buff
    {
        private readonly float _frequency;
        private float _timeData = 0;

        public BurnBuff(float duration, float frequency, int value) : base("Burn", duration, value)
        {
            _frequency = frequency;
        }

        protected override void OnTick(float deltaTime, Living target, GameObject player)
        {
            IsActive = true;
            _timeData += deltaTime;
            target.IsBurned = true;
            if (_timeData > _frequency)
            {
                target.Hp -= Value;
                if (player.gameObject.CompareTag("Monster"))
                {
                    var monsterManager = player.GetComponent<MonsterManager>();
                    monsterManager.BeHarmed(new DamageResult(DamageType.Burn, Value, false));
                    if (target.Hp <= 0)
                    {
                        IsActive = false;
                        monsterManager.CheckDead();
                    }
                }
                _timeData = 0;
            }
        }
        
        protected override void OnDisable(Living target, GameObject player)
        {
            target.IsBurned = false;
        }
    }
}