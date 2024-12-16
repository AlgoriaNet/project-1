using System;
using model;
using UnityEngine;

namespace battle
{
    public class NumericalBonusBuff : BasicAttributeBuff
    {
        public NumericalBonusBuff( float duration, AttributeType type, int value) : base("NumericalBonus", duration, type, value)
        {
        }
        
        public NumericalBonusBuff(string name, float duration, AttributeType type, int value) : base(name, duration, type, value)
        {
        }

        protected override void OnEnable(Living target, GameObject player)
        {
            switch (Type)
            {
                case AttributeType.Atk:
                    target.AtkBonus += Value;
                    break;
                case AttributeType.Def:
                    target.DefBonus += Value;
                    break;
                case AttributeType.Cri:
                    target.CriBonus += Value;
                    break;
                case AttributeType.Crt:
                    target.CrtBonus += Value;
                    break;
                case AttributeType.Speed:
                    target.SpeedBonus += Value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            target.DamageBonusRate += Value;
        }

        protected override void OnDisable(Living target, GameObject player)
        {
            switch (Type)
            {
                case AttributeType.Atk:
                    target.AtkBonus -= Value;
                    break;
                case AttributeType.Def:
                    target.DefBonus -= Value;
                    break;
                case AttributeType.Cri:
                    target.CriBonus -= Value;
                    break;
                case AttributeType.Crt:
                    target.CrtBonus -= Value;
                    break;
                case AttributeType.Speed:
                    target.SpeedBonus -= Value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}