using System;
using model;

namespace battle
{
    public class BuffFactory
    {
        public static Buff CreateBuff(string buffCharacter)
        {
            try
            {
                var replace = buffCharacter.Replace("ADD_BUFF_", "");
                var strings = replace.Split("_");
                var buffType = strings[0];
                switch (buffType)
                {
                    case "BurnBuff":
                        return new BurnBuff(float.Parse(strings[1]), 1, int.Parse(strings[2]));
                    case "DamageBonusRateBuff":
                        return new DamageBonusRateBuff(float.Parse(strings[1]), int.Parse(strings[2]));
                    case "FrozenBuff":
                        return new FrozenBuff(float.Parse(strings[1]));
                    case "NumericalBonusBuff":
                        return new NumericalBonusBuff(float.Parse(strings[1]), Enum.Parse<AttributeType>(strings[2]),int.Parse(strings[3]));
                    case "RateBonusBuff":
                        return new RateBonusBuff(float.Parse(strings[1]), Enum.Parse<AttributeType>(strings[2]),int.Parse(strings[3]));
                    case "SufferedDamageBonusRateBuff":
                        return new SufferedDamageBonusRateBuff(float.Parse(strings[1]), int.Parse(strings[2]));
                    case "SufferedVarietyDamageBonusRateBuff": ;
                        return new SufferedVarietyDamageBonusRateBuff(float.Parse(strings[1]), Enum.Parse<DamageType>(strings[2]),int.Parse(strings[3]));
                    case "VarietyDamageBonusBuff":
                        return new VarietyDamageBonusBuff(float.Parse(strings[1]), Enum.Parse<DamageType>(strings[2]),int.Parse(strings[3]));
                    case "VarietyDamageBonusRateBuff":
                        return new VarietyDamageBonusRateBuff(float.Parse(strings[1]), Enum.Parse<DamageType>(strings[2]),int.Parse(strings[3]));
                    default: return null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        } 
    }
}