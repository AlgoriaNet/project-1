using System.Collections.Generic;
using entity;
using UnityEngine;

namespace battle
{
    public class BuffManager
    {
        public readonly List<Buff> _buffs = new();

        public void AddBuff(Buff buff)
        {
            if (buff.Name == "Frozen")
            {
                var oldBuff = _buffs.Find(v => v.Name == "Frozen");
                if (oldBuff != null)
                {
                    var buffTimeRemaining = oldBuff.Duration - oldBuff.ElapsedTime;
                    if (buffTimeRemaining < buff.Duration)
                        oldBuff.Duration = oldBuff.ElapsedTime + buff.Duration;
                }
                else _buffs.Add(buff);
            }
            else _buffs.Add(buff);
        }

        public void RemoveBuff(string buffName)
        {
            _buffs.RemoveAll(buff => buff.Name == buffName && buff.IsActive);
        }

        public void EndBuff(string buffName, Living target, GameObject player)
        {
            _buffs.FindAll(buff => buff.Name == buffName && buff.IsActive)
                .ForEach(buff => buff.EndBuff(target, player));
            RemoveBuff(buffName);
        }

        public void TickAllBuffs(float deltaTime, Living target, GameObject player)
        {
            for (int i = _buffs.Count - 1; i >= 0; i--)
            {
                _buffs[i].Tick(deltaTime, target, player);
                if (!_buffs[i].IsActive)
                {
                    _buffs.RemoveAt(i); // 移除已过期的Buff
                }
            }
        }
    }
}