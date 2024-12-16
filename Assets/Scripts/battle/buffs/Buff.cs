using model;
using UnityEngine;

namespace battle
{
    public abstract class Buff
    {
        public string Name { get; }
        public float Duration { get;  set; }
        public float ElapsedTime { get; private set; } = 0f;
        
        public int Value { get; private set; }
        public bool IsActive { get; protected set; } = false;

        protected Buff(string name, float duration, int value)
        {
            Name = name;
            Duration = duration;
            Value = value;
        }

        public virtual void Tick(float deltaTime, Living target, GameObject player)
        {
            ElapsedTime += deltaTime;
            if (ElapsedTime >= Duration)
                OnExpired(target, player);
            else
                OnTick(deltaTime, target, player);
        }

        protected virtual void OnTick(float deltaTime, Living target, GameObject player)
        {
            IsActive = true;
            OnEnable(target, player);
            // Buff的每帧逻辑，如持续伤害、属性加成等
        }

        protected virtual void OnExpired(Living target, GameObject player)
        {
            IsActive = false;
            OnDisable(target, player);
            // Buff过期时的逻辑，如移除效果等
        }
        
        protected virtual void OnEnable(Living target, GameObject player)
        {
            //激活时
        }
        
        protected virtual void OnDisable(Living target, GameObject player)
        {
            //取消激活时
        }

        // 可选的：提供一个方法来立即结束Buff
        public void EndBuff(Living target, GameObject player)
        {
            IsActive = false;
            OnExpired(target, player);
        }
        
        public Buff ShallowCopy()
        {
            return (Buff) MemberwiseClone();
        }
    }
}