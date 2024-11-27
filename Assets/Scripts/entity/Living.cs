using System;
using System.Collections.Generic;
using battle;
using Unity.VisualScripting;

namespace entity
{
    public abstract class Living
    {
        public string Name { get; set; }

        //最大生命值
        public int MaxHp { get; set; }

        //当前生命值
        public int Hp { get; set; }

        //攻击相关
        public int Atk { get; set; }
        public int AtkBonus { get; set; } = 0;

        public int AtkBonusRate { get; set; } = 0;

        //防御力
        public int Def { get; set; }
        public int DefBonus { get; set; }

        public int DefBonusRate { get; set; }

        //暴击率
        public int CRI { get; set; }

        public int CRIBonus { get; set; }

        //暴击伤害
        public int CRT { get; set; } = 150;

        public int CRTBonus { get; set; }

        //移动速度
        public int Speed { get; set; }
        public int SpeedBonus { get; set; }

        public int SpeedBonusRate { get; set; }

        //最终伤害
        public int DamageBonusRate { get; set; }

        public int SufferedDamageBonusRate { get; set; }

        //各系伤害
        public readonly Dictionary<DamageType, int> VarietyDamage = new();

        //各系伤害加成
        public readonly Dictionary<DamageType, int> VarietyDamageBonus = new();

        //各系伤害加成率
        public readonly Dictionary<DamageType, int> VarietyDamageBonusRate = new();

        //受到伤害加成率
        public readonly Dictionary<DamageType, int> SufferedVarietyDamageBonusRate = new();

        public bool IsFrozen { get; set; }
        public bool IsBurned { get; set; }
        public bool IsSpeedCut { get; set; }

        public BuffManager BuffManager { get; } = new();

        public DamageResult AttackDamage(DamageType damageType, Living target)
        {
            int eventualAtk = (int)Math.Round((Atk + AtkBonus) * (1.0f + AtkBonusRate / 100f));
            eventualAtk += VarietyDamage.GetValueOrDefault(damageType, 0);
            eventualAtk += VarietyDamageBonus.GetValueOrDefault(damageType, 0);
            int eventualDef = (int)Math.Round((target.Def + target.DefBonus) * (1.0f + target.DefBonusRate / 100f));
            float randomFactor = (float)(new Random().NextDouble() * 0.2 - 0.1);
            
            double damage = (eventualAtk - eventualDef) *
                            (1.0 + VarietyDamageBonusRate.GetValueOrDefault(damageType, 0) / 100f -
                             target.SufferedVarietyDamageBonusRate.GetValueOrDefault(damageType, 0) / 100f) *
                            (1.0 + DamageBonusRate / 100f - target.SufferedDamageBonusRate / 100f) *
                            (1.0 + randomFactor);
            int damageInt = (int)Math.Round(damage);

            bool isCrit = new Random().Next(0, 100) <= CRI + CRIBonus;
            if (isCrit)
            {
                damageInt = (int)Math.Round(damageInt * (CRT + CRTBonus) / 100f);
            }
          
            
            target.Hp -= damageInt;
            return new DamageResult(damageType, damageInt, isCrit);
        }
    }
}