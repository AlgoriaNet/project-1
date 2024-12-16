using System.Collections.Generic;
using System.Linq;
using entity;
using UnityEngine;

namespace battle
{
    public class BuffManager
    {
        public readonly List<Buff> _buffs = new();

        public void AddBuff(Buff buff)
        {
            if(buff == null) return;
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
            foreach (var buff in _buffs.ToList())
            {
                buff.Tick(deltaTime, target, player);
                if (!buff.IsActive) _buffs.Remove(buff); // 移除已过期的Buff
            }
        }
    }
}