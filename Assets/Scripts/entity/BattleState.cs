using System.Collections.Generic;
using JetBrains.Annotations;

namespace entity
{
    public class BattleState
    {
        public List<int> UpgradeRequiredExperience;
        public int Experience;
        public float ExperienceRate = 0;
        public int MaxBattleLevel;
        public int BattleLevel;
        public int MaxHp;
        public int Hp;
        public float HpRate;

        public bool AddExperience(int experience)
        {
            Experience += experience;
            if (BattleLevel >= MaxBattleLevel) return false;
            if ( Experience >= UpgradeRequiredExperience[BattleLevel])
            {
                Experience -= UpgradeRequiredExperience[BattleLevel];
                BattleLevel++;
                return true;
            }

            ExperienceRate = (float)Experience / UpgradeRequiredExperience[BattleLevel];
            return false;
        }

        public bool ReduceHp(int hp)
        {
            Hp -= hp;
            HpRate = (float)Hp / MaxHp;
            HpRate = HpRate < 0 ? 0 : HpRate;
            return Hp <= 0;
        }

        [CanBeNull]
        public string LevelTxt()
        {
            if (BattleLevel < MaxBattleLevel)
                return "Lv." + BattleLevel;
            return "MAX";
        }
    }
}