using entity;
using UnityEngine;

namespace battle
{
    public class RateBonusBuff : BasicAttributeBuff
    {
        public RateBonusBuff(string name, float duration, AttributeType type, int value) : base(name, duration, type, value)
        {
        }

        protected override void OnEnable(Living target, GameObject player)
        {
            switch (Type)
            {
                case AttributeType.Atk:
                    target.AtkBonusRate += Value;
                    break;
                case AttributeType.Def:
                    target.DefBonusRate += Value;
                    break;
                case AttributeType.CRI:
                    break;
                case AttributeType.CRT:
                    break;
                case AttributeType.Speed:
                    target.SpeedBonusRate += Value;
                    break;
                default:
                    throw new System.ArgumentOutOfRangeException();
            }
        }

        protected override void OnDisable(Living target, GameObject player)
        {
            switch (Type)
            {
                case AttributeType.Atk:
                    target.AtkBonusRate -= Value;
                    break;
                case AttributeType.Def:
                    target.DefBonusRate -= Value;
                    break;
                case AttributeType.CRI:
                    break;
                case AttributeType.CRT:
                    break;
                case AttributeType.Speed:
                    target.SpeedBonusRate -= Value;
                    break;
                default:
                    throw new System.ArgumentOutOfRangeException();
            }
        }
    }
}