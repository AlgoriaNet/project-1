using entity;
using UnityEngine;

namespace battle
{
    public abstract class BasicAttributeBuff : Buff
    {

        
        protected readonly AttributeType Type;

        protected BasicAttributeBuff(string name, float duration, AttributeType type , int value) : base(name, duration, value)
        {
            Type = type;
        }
        
    }
}